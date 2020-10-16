#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Def.Models;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CopyGitLink.Shared.Core
{
    [Export(typeof(IRepositoryService))]
    internal sealed class RepositoryService : IRepositoryService, IDisposable
    {
        private const string GitFolderName = ".git";
        private const string GitConfigFileName = "config";
        private const string GitConfigFileRemoteOriginSection = "remote \"origin\"";
        private const string GitRemoteOriginUrl = "url";

        private readonly IEnumerable<Lazy<IGitOnlineService>> _gitOnlineServices;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly AsyncSemaphore _semaphore = new AsyncSemaphore(1);
        private readonly AsyncQueue<string> _filesRepositoryToDiscover = new AsyncQueue<string>();
        private readonly Dictionary<string, RepositoryInfo> _repositories = new Dictionary<string, RepositoryInfo>();

        [ImportingConstructor]
        internal RepositoryService(
            [ImportMany] IEnumerable<Lazy<IGitOnlineService>> gitOnlineServices)
        {
            _gitOnlineServices = gitOnlineServices;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }

        public void QueueRepositoryDiscovery(string filePath)
        {
            lock (_filesRepositoryToDiscover)
            {
                Requires.NotNullOrEmpty(filePath, nameof(filePath));
                _filesRepositoryToDiscover.TryEnqueue(filePath);
            }

            TreatRepositoryDiscoveryQueueAsync().Forget();
        }

        public async Task DiscoverRepositoryAsync(string filePath, CancellationToken cancellationToken)
        {
            Requires.NotNullOrEmpty(filePath, nameof(filePath));

            // Switch to background thread on purpose to avoid blocking the main thread.
            await TaskScheduler.Default;

            using (await _semaphore.EnterAsync(cancellationToken))
            {
                await DiscoverRepositoryInternalAsync(filePath, cancellationToken);
            }
        }

        public bool IsFilePartOfKnownRepository(string filePath)
        {
            return TryGetKnownRepositoryFolder(filePath, out _);
        }

        public bool TryGetKnownRepository(string filePath, out string repositoryFolder, out RepositoryInfo? repositoryInfo)
        {
            if (TryGetKnownRepositoryFolder(filePath, out repositoryFolder))
            {
                lock (_repositories)
                {
                    return _repositories.TryGetValue(repositoryFolder, out repositoryInfo);
                }
            }

            repositoryInfo = null;
            return false;
        }

        private async Task TreatRepositoryDiscoveryQueueAsync()
        {
            // Switch to background thread on purpose to avoid blocking the main thread.
            await TaskScheduler.Default;

            using (await _semaphore.EnterAsync(_cancellationTokenSource.Token))
            {
                while (!_filesRepositoryToDiscover.IsCompleted && _filesRepositoryToDiscover.Count > 0)
                {
                    try
                    {
                        string filePath = await _filesRepositoryToDiscover.DequeueAsync(_cancellationTokenSource.Token);
                        await DiscoverRepositoryInternalAsync(filePath, _cancellationTokenSource.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        // This is expected in the case that _documentsToAnalyze.Complete
                        // is called while this method is awaiting DequeueAsync on
                        // an empty queue.
                    }
                }
            }
        }

        /// <summary>
        /// Search the parent .git folder to a given <paramref name="filePath"/> and retrieve the Uri where this git repository pushes commits.
        /// </summary>
        /// <param name="filePath">Full path to a file on the hard drive.</param>
        private async Task DiscoverRepositoryInternalAsync(string filePath, CancellationToken cancellationToken)
        {
            // Check if this document if part of an already known repository to avoid having to do an IO access.
            if (IsFilePartOfKnownRepository(filePath))
            {
                return;
            }

            // Search for the closest .git folders.
            var directoryInfo = new DirectoryInfo(filePath);

            do
            {
                string gitConfigFilePath = Path.Combine(directoryInfo.FullName, GitFolderName, GitConfigFileName);

                if (File.Exists(gitConfigFilePath))
                {
                    try
                    {
                        var repositoryPushUriReader = new StringBuilder(255);

                        if (ErrorHandler.Succeeded(
                                NativeMethods.GetPrivateProfileString(
                                    lpAppName: GitConfigFileRemoteOriginSection,
                                    lpKeyName: GitRemoteOriginUrl,
                                    lpDefault: string.Empty,
                                    lpReturnedString: repositoryPushUriReader,
                                    nSize: 255,
                                    lpFileName: gitConfigFilePath)))
                        {
                            string repositoryPushUri = repositoryPushUriReader.ToString();

                            if (string.IsNullOrEmpty(repositoryPushUri))
                            {
                                // local repository detected, but it seems like it isn't tracked to a remote repository, so let's ignore it.
                                return;
                            }

                            string repositoryFolder = directoryInfo.FullName + "\\";
                            await RegisterRepositoryAsync(repositoryFolder, repositoryPushUri, cancellationToken);
                        }

                        return;
                    }
                    catch
                    {
                        // Just ignore it. Maybe the .git folder actually doesn't corresponds to a valid Git repository folder.
                    }
                }

                directoryInfo = directoryInfo.Parent;
            } while (directoryInfo != null);

            // Found nothing. This file isn't part of a Git repository.
        }


        private bool TryGetKnownRepositoryFolder(string filePath, out string repositoryFolder)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                repositoryFolder = string.Empty;
                return false;
            }


            lock (_repositories)
            {
                foreach (string knownRepositoryFolder in _repositories.Keys)
                {
                    var directoryPathInfo = new DirectoryInfo(filePath);
                    var repositoryFolderInfo = new DirectoryInfo(knownRepositoryFolder);

                    while (directoryPathInfo.Parent != null)
                    {
                        if (string.Equals(directoryPathInfo.Parent.FullName + "\\", repositoryFolderInfo.FullName, StringComparison.Ordinal))
                        {
                            repositoryFolder = knownRepositoryFolder;
                            return true;
                        }
                        else
                        {
                            directoryPathInfo = directoryPathInfo.Parent;
                        }
                    }
                }
            }

            repositoryFolder = string.Empty;
            return false;
        }

        private async Task RegisterRepositoryAsync(
            string repositoryFolder,
            string repositoryPushUri,
            CancellationToken cancellationToken)
        {
            foreach (var gitOnlineService in _gitOnlineServices)
            {
                if (await gitOnlineService.Value.TryDetectRepositoryInformationAsync(
                        repositoryFolder,
                        repositoryPushUri,
                        cancellationToken,
                        out RepositoryInfo? repositoryInfo)
                    && repositoryInfo != null)
                {
                    lock (_repositories)
                    {
                        _repositories[repositoryFolder] = repositoryInfo;
                    }
                    return;
                }
            }
        }
    }
}

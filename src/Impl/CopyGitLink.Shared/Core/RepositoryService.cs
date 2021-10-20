#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Def.Models;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Telemetry;
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
        private const string InvitedUserToCreateRepositoryEvent = "vs/ide/copygitlink/invitedusertocreaterepository";
        private const string UserCreatedRepositoryEvent = "vs/ide/copygitlink/usercreatedrepository";
        private const string UserCreatedRepositoryFaultEvent = "vs/ide/copygitlink/usercreatedrepositoryfault";
        private const string IsLocalRepositoryPropertyName = "CopyGitLink.IsLocalRepository";
        private const string ElapsedMinutesSinceInvitationToCreateRepositoryPropertyName = "CopyGitLink.ElapsedMinutesSinceInvitationToCreateRepository";

        private const string GitFolderName = ".git";
        private const string GitConfigFileName = "config";
        private const string GitConfigFileRemoteOriginSection = "remote \"origin\"";
        private const string GitRemoteOriginUrl = "url";

        private readonly Dictionary<string, FileSystemWatcher> _monitoredVolumes = new Dictionary<string, FileSystemWatcher>();
        private readonly IEnumerable<Lazy<IGitOnlineService>> _gitOnlineServices;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly AsyncSemaphore _semaphore = new AsyncSemaphore(1);
        private readonly AsyncQueue<string> _filesRepositoryToDiscover = new AsyncQueue<string>();
        private readonly Dictionary<string, RepositoryInfo> _remoteRepositories = new Dictionary<string, RepositoryInfo>();
        private readonly HashSet<string> _localRepositories = new HashSet<string>();

        private DateTime _lastStartListeningForRepositoryCreationInvokeDateTime;

        [ImportingConstructor]
        internal RepositoryService(
            [ImportMany] IEnumerable<Lazy<IGitOnlineService>> gitOnlineServices)
        {
            _gitOnlineServices = gitOnlineServices;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _semaphore.Dispose();
            foreach (var item in _monitoredVolumes)
            {
                item.Value.Dispose();
            }
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

        public bool IsFilePartOfKnownRemoteRepository(string filePath)
        {
            return TryGetKnownRemoteRepository(filePath, out _, out _);
        }

        public bool TryGetKnownRemoteRepository(string filePath, out string repositoryFolder, out RepositoryInfo? repositoryInfo)
        {
            if (TryGetKnownRepositoryFolder(filePath, out repositoryFolder))
            {
                lock (_remoteRepositories)
                {
                    return _remoteRepositories.TryGetValue(repositoryFolder, out repositoryInfo);
                }
            }

            repositoryInfo = null;
            return false;
        }

        public void StartListeningForRepositoryCreation(string baseDirectory)
        {
            var volumeFolder = Directory.GetDirectoryRoot(baseDirectory);
            lock (_monitoredVolumes)
            {
                if (!_monitoredVolumes.ContainsKey(volumeFolder))
                {
                    var fileWatcher = new FileSystemWatcher(volumeFolder)
                    {
                        NotifyFilter
                            = NotifyFilters.CreationTime
                              | NotifyFilters.DirectoryName,
                        Filter = ".git",
                        IncludeSubdirectories = true,
                        EnableRaisingEvents = true
                    };

                    // File watcher won't raise an event for files inside of the .git repository.
                    // So we're looking for when a .git folder is created instead of when a `.git\config` file
                    // has changed.
                    fileWatcher.Created += FileWatcher_Created;
                    Log(InvitedUserToCreateRepositoryEvent);
                    _lastStartListeningForRepositoryCreationInvokeDateTime = DateTime.Now;

                    _monitoredVolumes.Add(volumeFolder, fileWatcher);
                }
            }
        }

        private void FileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Task.Run(async () =>
            {
                // File watcher won't raise an event for files inside of the .git repository.
                // So we're now trying to wait that the `.git\config` is created.
                var configFilePath = Path.Combine(e.FullPath, "config");
                for (int i = 0; i < 10; i++)
                {
                    if (!File.Exists(configFilePath))
                    {
                        await Task.Delay(1000).ConfigureAwait(false);
                    }
                    else
                    {
                        break;
                    }
                }

                // Wait 5 seconds to give time to VS to finish writing into the `.git\config` file, in case...
                // Sometimes it takes time before the remote Git information arrive in the file.
                // The telemtry may be flacky but it's acceptable from PM perspective.
                await Task.Delay(5000).ConfigureAwait(false);

                if (!IsFilePartOfKnownRemoteRepository(e.FullPath) && !IsFilePartOfKnownLocalRepository(e.FullPath))
                {
                    await DiscoverRepositoryAsync(e.FullPath, _cancellationTokenSource.Token);

                    bool isLocalRepository = IsFilePartOfKnownLocalRepository(e.FullPath);
                    if (isLocalRepository || IsFilePartOfKnownRemoteRepository(e.FullPath))
                    {
                        // It appears that the user created a local or remote Git repository.
                        Log(
                            UserCreatedRepositoryEvent,
                            new Dictionary<string, object>
                            {
                                { IsLocalRepositoryPropertyName, isLocalRepository },
                                { ElapsedMinutesSinceInvitationToCreateRepositoryPropertyName, (DateTime.Now - _lastStartListeningForRepositoryCreationInvokeDateTime).TotalMinutes }
                            });
                    }
                }
            }).FileAndForget(UserCreatedRepositoryFaultEvent);
        }

        private bool IsFilePartOfKnownLocalRepository(string filePath)
        {
            if (TryGetKnownRepositoryFolder(filePath, out string repositoryFolder))
            {
                lock (_remoteRepositories)
                {
                    return !_remoteRepositories.TryGetValue(repositoryFolder, out _);
                }
            }

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
            if (IsFilePartOfKnownRemoteRepository(filePath))
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
                            string repositoryFolder = directoryInfo.FullName + "\\";
                            string repositoryPushUri = repositoryPushUriReader.ToString();

                            if (string.IsNullOrEmpty(repositoryPushUri))
                            {
                                // local repository detected, but it seems like it isn't tracked to a remote repository.
                                lock (_localRepositories)
                                {
                                    _localRepositories.Add(repositoryFolder);
                                }
                            }
                            else
                            {
                                await RegisterRemoteRepositoryAsync(repositoryFolder, repositoryPushUri, cancellationToken);
                            }
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

            var directoryPathInfo = new DirectoryInfo(filePath);
            string fileParentDirectoryFullPath = directoryPathInfo.Parent.FullName + "\\";

            lock (_remoteRepositories)
            {
                foreach (string knownRepositoryFolder in _remoteRepositories.Keys)
                {
                    var repositoryFolderInfo = new DirectoryInfo(knownRepositoryFolder);

                    if (fileParentDirectoryFullPath.StartsWith(repositoryFolderInfo.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        repositoryFolder = knownRepositoryFolder;
                        return true;
                    }
                }
            }

            lock (_localRepositories)
            {
                foreach (string knownRepositoryFolder in _localRepositories)
                {
                    var repositoryFolderInfo = new DirectoryInfo(knownRepositoryFolder);

                    if (fileParentDirectoryFullPath.StartsWith(repositoryFolderInfo.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        repositoryFolder = knownRepositoryFolder;
                        return true;
                    }
                }
            }

            repositoryFolder = string.Empty;
            return false;
        }

        private async Task RegisterRemoteRepositoryAsync(
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
                    lock (_remoteRepositories)
                    {
                        _remoteRepositories[repositoryFolder] = repositoryInfo;
                    }
                    return;
                }
            }
        }

        private void Log(string eventName, Dictionary<string, object>? properties = null)
        {
            var telemetryEvent = new TelemetryEvent(eventName);
            if (properties != null)
            {
                foreach (var item in properties)
                {
                    telemetryEvent.Properties.Add(item.Key, item.Value);
                }
            }

            TelemetryService.DefaultSession.PostEvent(telemetryEvent);
        }
    }
}

#nullable enable

using CopyGitLink.Def.Models;
using System.Threading;
using System.Threading.Tasks;

namespace CopyGitLink.Def
{
    /// <summary>
    /// Provides a set of methods to manage repositories discovered during the user session.
    /// </summary>
    public interface IRepositoryService
    {
        /// <summary>
        /// Given a <paramref name="filePath"/>, look for the Git repository this file is part of and cache some information about it.
        /// </summary>
        /// <remarks>
        /// This method queue a task to discover a repository. The task won't be done by the time this method ends.
        /// If you want to be sure the work is done by the time the method call ends, please use <see cref="DicoverRepositoryAsync"/> instead.
        /// </remarks>
        /// <param name="filePath">Full path to a file on the hard drive.</param>
        void QueueRepositoryDiscovery(string filePath);

        /// <summary>
        /// Given a <paramref name="filePath"/>, look for the Git repository this file is part of and cache some information about it.
        /// </summary>
        /// <param name="filePath">Full path to a file on the hard drive.</param>
        Task DiscoverRepositoryAsync(string filePath, CancellationToken cancellationToken);

        /// <summary>
        /// Detects whether a given <paramref name="filePath"/> is part of a previously discovered repository.
        /// </summary>
        /// <remarks>
        /// This doesn't do any IO access because it only check for the cache and doesn't queue a repository discovery task.
        /// </remarks>
        /// <param name="filePath">Full path to a file on the hard drive.</param>
        /// <returns>Returns <code>True</code> if the file path is part of a known Git repository.</returns>
        bool IsFilePartOfKnownRemoteRepository(string filePath);

        /// <summary>
        /// Returns the repository information of a previously discovered repository a given <paramref name="filePath"/> is part of.
        /// This doesn't do any IO access because it only check for the cache and doesn't queue a repository discovery task.
        /// </summary>
        /// <param name="filePath">Full path to a file on the hard drive.</param>
        /// <param name="repositoryFolder">The full path to the root folder of the repository on the local machine.</param>
        /// <param name="repositoryInfo">The information about the repository the given <paramref name="filePath"/> is part of.</param>
        /// <returns>Returns <code>True</code> if the file path is part of a known Git repository.</returns>
        bool TryGetKnownRemoteRepository(string filePath, out string repositoryFolder, out RepositoryInfo? repositoryInfo);

        /// <summary>
        /// Starts monitor the given <paramref name="baseDirectory"/> and its parents until a `.git` folder appears.
        /// </summary>
        void StartListeningForRepositoryCreation(string baseDirectory);
    }
}

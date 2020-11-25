#nullable enable

using CopyGitLink.Def.Models;
using System.Threading;
using System.Threading.Tasks;

namespace CopyGitLink.Def
{
    /// <summary>
    /// Provides a set of Git online service provider specific methods.
    /// </summary>
    public interface IGitOnlineService
    {
        /// <summary>
        /// Gets the name of the Git online service.
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Tries to generate a <see cref="RepositoryInfo"/> based on the repository push Uri and the repository folder.
        /// </summary>
        /// <param name="repositoryFolder">The location of the repository on the hard drive.</param>
        /// <param name="repositoryUri">The location of the repository online.</param>
        /// <param name="repositoryInfo">The detected information about the repository.</param>
        /// <returns>Returns <code>True</code> if information have been detected.</returns>
        Task<bool> TryDetectRepositoryInformationAsync(
            string repositoryFolder,
            string repositoryUri,
            CancellationToken cancellationToken,
            out RepositoryInfo? repositoryInfo);

        /// <summary>
        /// Generates a link to the Git online service provider.
        /// </summary>
        /// <param name="repositoryFolder">The root folder of the repository</param>
        /// <param name="repositoryInfo">The information about the repository.</param>
        /// <param name="filePath">The path to the file for which a link should be generated.</param>
        /// <param name="startLineNumber">The line where the selection starts</param>
        /// <param name="startColumnNumber">The column where the selection starts</param>
        /// <param name="endLineNumber">The line where the selection ends</param>
        /// <param name="endColumnNumber">The column where the selection ends</param>
        /// <returns>Returns a generated link.</returns>
        Task<string> GenerateLinkAsync(
            string repositoryFolder,
            RepositoryInfo repositoryInfo,
            string filePath,
            long? startLineNumber = null,
            long? startColumnNumber = null,
            long? endLineNumber = null,
            long? endColumnNumber = null);
    }
}

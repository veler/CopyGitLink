using System.Threading.Tasks;

namespace CopyGitLink.Def
{
    /// <summary>
    /// Provides a set of methods to run some pre-defined Git commands on the local machine.
    /// </summary>
    public interface IGitCommandService
    {
        /// <summary>
        /// Retrieves the closest Git commit from the active branch that exists online.
        /// </summary>
        /// <param name="repositoryFolder">The root folder of the local repository</param>
        /// <returns>Returns the hash of a Git commit.</returns>
        Task<string> GetBestGitCommitAsync(string repositoryFolder);
    }
}

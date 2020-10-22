#nullable enable

using CopyGitLink.Def;
using Microsoft.VisualStudio.Threading;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CopyGitLink.Shared.Core
{
    [Export(typeof(IGitCommandService))]
    internal sealed class GitCommandService : IGitCommandService
    {
        private const string HeadBranch = "HEAD";

        public async Task<string> GetBestRemoteGitBranchAsync(string repositoryFolder)
        {
            // Switch to background thread on purpose to avoid blocking the main thread.
            await TaskScheduler.Default;

            // retrieve the current branch.
            var currentBranch = GetCurrentBranchName(repositoryFolder);

            if (string.Equals(currentBranch, HeadBranch, System.StringComparison.Ordinal))
            {
                // HEAD isn't a valid branch.
                return GetDefaultBranchName(repositoryFolder);
            }

            // check whether this branch exists online.
            if (BranchExistsOnline(repositoryFolder, currentBranch))
            {
                return currentBranch;
            }

            // if not, get the default branch of the repository.
            return GetDefaultBranchName(repositoryFolder);
        }

        private bool BranchExistsOnline(string gitFolder, string branchName)
        {
            return string.IsNullOrWhiteSpace(RunGitCommand(gitFolder, @"git show-ref refs/heads/" + branchName));
        }

        private string GetCurrentBranchName(string gitFolder)
        {
            return RunGitCommand(gitFolder, "rev-parse --abbrev-ref HEAD");
        }

        private string GetDefaultBranchName(string gitFolder)
        {
            var result = RunGitCommand(gitFolder, @"symbolic-ref refs/remotes/origin/HEAD");
            return result.Replace("refs/remotes/origin/", string.Empty);
        }

        private string RunGitCommand(string repositoryFolder, string gitCommand)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("git");
                processStartInfo.WorkingDirectory = repositoryFolder;
                processStartInfo.Arguments = gitCommand;
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.CreateNoWindow = true;

                Process process = Process.Start(processStartInfo);
                string output = process.StandardOutput.ReadLine();

                process.WaitForExit();

                return output ?? string.Empty;
            }
            catch
            {
                // Fail silently.
            }

            return string.Empty;
        }
    }
}

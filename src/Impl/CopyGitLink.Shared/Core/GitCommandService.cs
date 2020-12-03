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
        public async Task<string> GetBestGitCommitAsync(string repositoryFolder)
        {
            // Switch to background thread on purpose to avoid blocking the main thread.
            await TaskScheduler.Default;

            // retrieve the current commit.
            var commitId = GetCurrentCommitId(repositoryFolder);

            return commitId;
        }

        private string GetCurrentCommitId(string gitFolder)
        {
            return RunGitCommand(gitFolder, "rev-parse HEAD");
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

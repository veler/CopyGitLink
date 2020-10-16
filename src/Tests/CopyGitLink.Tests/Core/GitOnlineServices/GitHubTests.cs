using CopyGitLink.Def.Models;
using CopyGitLink.Shared.Core.GitOnlineServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace CopyGitLink.Tests.Core.GitOnlineServices
{
    [TestClass]
    public class GitHubTests
    {
        [TestMethod]
        public async Task GitHubUriAsync()
        {
            var gitHub = new GitHub(null);
            Assert.IsTrue(await gitHub.TryDetectRepositoryInformationAsync(string.Empty, "https://github.com/octokit/octokit.net.git", CancellationToken.None, out RepositoryInfo repositoryInfo));
            Assert.AreEqual("octokit", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("octokit.net", repositoryInfo.Properties["Repository"]);

            Assert.IsTrue(await gitHub.TryDetectRepositoryInformationAsync(string.Empty, "https://github.com/dotnet/roslyn.git", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("dotnet", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("roslyn", repositoryInfo.Properties["Repository"]);

            Assert.IsTrue(await gitHub.TryDetectRepositoryInformationAsync(string.Empty, "http://github.com/microsoft/vscode.git", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("microsoft", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("vscode", repositoryInfo.Properties["Repository"]);
        }

        [TestMethod]
        public async Task BadGitHubUrisAsync()
        {
            var gitHub = new GitHub(null);
            Assert.IsFalse(await gitHub.TryDetectRepositoryInformationAsync(string.Empty, "http://github.com/microsoft/vscode", CancellationToken.None, out _));
            Assert.IsFalse(await gitHub.TryDetectRepositoryInformationAsync(string.Empty, "http://contoso.visualstudio.com/Contoso", CancellationToken.None, out _));
            Assert.IsFalse(await gitHub.TryDetectRepositoryInformationAsync(string.Empty, "https://facebook.com", CancellationToken.None, out _));
            Assert.IsFalse(await gitHub.TryDetectRepositoryInformationAsync(string.Empty, "www.github.com/dotnet/roslyn.git", CancellationToken.None, out _));
            Assert.IsFalse(await gitHub.TryDetectRepositoryInformationAsync(string.Empty, "hello I am not a uri!", CancellationToken.None, out _));
        }
    }
}

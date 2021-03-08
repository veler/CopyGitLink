using CopyGitLink.Def.Models;
using CopyGitLink.Shared.Core.GitOnlineServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace CopyGitLink.Tests.Core.GitOnlineServices
{
    [TestClass]
    public class GitHubnLabTests
    {
        [TestMethod]
        public async Task GitHubUriAsync()
        {
            var gitHubnLab = new GitHubnLab(null);
            Assert.IsTrue(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "https://github.com/octokit/octokit.net.git", CancellationToken.None, out RepositoryInfo repositoryInfo));
            Assert.AreEqual("octokit", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("octokit.net", repositoryInfo.Properties["Repository"]);

            Assert.IsTrue(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "https://github.com/dotnet/roslyn.git", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("dotnet", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("roslyn", repositoryInfo.Properties["Repository"]);

            Assert.IsTrue(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "http://github.com/microsoft/vscode.git", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("microsoft", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("vscode", repositoryInfo.Properties["Repository"]);

            Assert.IsTrue(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "http://github.com/microsoft/vscode", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("microsoft", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("vscode", repositoryInfo.Properties["Repository"]);

            Assert.IsTrue(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "git@github.com:dotnet/roslyn.git", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("dotnet", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("roslyn", repositoryInfo.Properties["Repository"]);

            Assert.IsTrue(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "git@git.constoso.eu:dotnet/lang/csharp.git", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("dotnet/lang", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("csharp", repositoryInfo.Properties["Repository"]);
            Assert.AreEqual("git.constoso.eu", repositoryInfo.Properties["Host"]);

            Assert.IsTrue(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "git@git.constoso.eu:dotnet/lang/csharp", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("dotnet/lang", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("csharp", repositoryInfo.Properties["Repository"]);
            Assert.AreEqual("git.constoso.eu", repositoryInfo.Properties["Host"]);

            Assert.IsTrue(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "ssh@git.constoso.eu:dotnet/lang/csharp.git", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("dotnet/lang", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("csharp", repositoryInfo.Properties["Repository"]);
            Assert.AreEqual("git.constoso.eu", repositoryInfo.Properties["Host"]);
        }

        [TestMethod]
        public async Task BadGitHubUrisAsync()
        {
            var gitHubnLab = new GitHubnLab(null);
            Assert.IsFalse(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "gitfoo@git.constoso.eu:dotnet/lang/csharp.git", CancellationToken.None, out _));
            Assert.IsFalse(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "sshfoo@git.constoso.eu:dotnet/lang/csharp.git", CancellationToken.None, out _));
            Assert.IsFalse(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "http://contoso.visualstudio.com/Contoso", CancellationToken.None, out _));
            Assert.IsFalse(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "https://facebook.com", CancellationToken.None, out _));
            Assert.IsFalse(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "www.github.com/dotnet/roslyn.git", CancellationToken.None, out _));
            Assert.IsFalse(await gitHubnLab.TryDetectRepositoryInformationAsync(string.Empty, "hello I am not a uri!", CancellationToken.None, out _));
        }
    }
}

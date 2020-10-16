using System.Threading;
using System.Threading.Tasks;
using CopyGitLink.Def.Models;
using CopyGitLink.Shared.Core.GitOnlineServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CopyGitLink.Tests.Core.GitOnlineServices
{
    [TestClass]
    public class AzureDevOpsTests
    {
        [TestMethod]
        public async Task DevAzureComAsync()
        {
            var azureDevOps = new AzureDevOps(null);
            Assert.IsTrue(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://dev.azure.com/Contoso/DefaultCollection/Contoso/_git/MyRepository/?foor=bar", CancellationToken.None, out RepositoryInfo repositoryInfo));
            Assert.AreEqual("Contoso", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("Contoso", repositoryInfo.Properties["Project"]);
            Assert.AreEqual("MyRepository", repositoryInfo.Properties["Repository"]);
            Assert.AreEqual("https://dev.azure.com/Contoso/", repositoryInfo.Properties["OrganizationUrl"]);
            Assert.AreEqual("https://dev.azure.com/Contoso/Contoso/_git/MyRepository/", repositoryInfo.Properties["RepositoryUrl"]);

            Assert.IsTrue(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://dev.azure.com/Contoso/DefaultCollection/Contoso/_git/MyRepository?foor=bar", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("Contoso", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("Contoso", repositoryInfo.Properties["Project"]);
            Assert.AreEqual("MyRepository", repositoryInfo.Properties["Repository"]);
            Assert.AreEqual("https://dev.azure.com/Contoso/", repositoryInfo.Properties["OrganizationUrl"]);
            Assert.AreEqual("https://dev.azure.com/Contoso/Contoso/_git/MyRepository/", repositoryInfo.Properties["RepositoryUrl"]);

            Assert.IsTrue(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://dev.azure.com/Contoso/DefaultCollection/Contoso/_git/MyRepository", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("Contoso", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("Contoso", repositoryInfo.Properties["Project"]);
            Assert.AreEqual("MyRepository", repositoryInfo.Properties["Repository"]);
            Assert.AreEqual("https://dev.azure.com/Contoso/", repositoryInfo.Properties["OrganizationUrl"]);
            Assert.AreEqual("https://dev.azure.com/Contoso/Contoso/_git/MyRepository/", repositoryInfo.Properties["RepositoryUrl"]);
        }

        [TestMethod]
        public async Task VisualStudioComAsync()
        {
            var azureDevOps = new AzureDevOps(null);
            Assert.IsTrue(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://john.doe@contoso.visualstudio.com/DefaultCollection/Contoso/_git/MyRepository/?foor=bar", CancellationToken.None, out RepositoryInfo repositoryInfo));
            Assert.AreEqual("contoso", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("Contoso", repositoryInfo.Properties["Project"]);
            Assert.AreEqual("MyRepository", repositoryInfo.Properties["Repository"]);
            Assert.AreEqual("https://contoso.visualstudio.com/", repositoryInfo.Properties["OrganizationUrl"]);
            Assert.AreEqual("https://contoso.visualstudio.com/Contoso/_git/MyRepository/", repositoryInfo.Properties["RepositoryUrl"]);

            Assert.IsTrue(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://john.doe@contoso.visualstudio.com/DefaultCollection/Contoso/_git/MyRepository?foor=bar", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("contoso", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("Contoso", repositoryInfo.Properties["Project"]);
            Assert.AreEqual("MyRepository", repositoryInfo.Properties["Repository"]);
            Assert.AreEqual("https://contoso.visualstudio.com/", repositoryInfo.Properties["OrganizationUrl"]);
            Assert.AreEqual("https://contoso.visualstudio.com/Contoso/_git/MyRepository/", repositoryInfo.Properties["RepositoryUrl"]);

            Assert.IsTrue(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://john.doe@contoso.visualstudio.com/DefaultCollection/Contoso/_git/MyRepository", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("contoso", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("Contoso", repositoryInfo.Properties["Project"]);
            Assert.AreEqual("MyRepository", repositoryInfo.Properties["Repository"]);
            Assert.AreEqual("https://contoso.visualstudio.com/", repositoryInfo.Properties["OrganizationUrl"]);
            Assert.AreEqual("https://contoso.visualstudio.com/Contoso/_git/MyRepository/", repositoryInfo.Properties["RepositoryUrl"]);
        }

        [TestMethod]
        public async Task PrivateServerAsync()
        {
            var azureDevOps = new AzureDevOps(null);
            Assert.IsTrue(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://tfs.contoso.com:8080/tfs/Contoso/_git/MyRepository/?foor=bar", CancellationToken.None, out RepositoryInfo repositoryInfo));
            Assert.AreEqual("tfs.contoso.com", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("Contoso", repositoryInfo.Properties["Project"]);
            Assert.AreEqual("MyRepository", repositoryInfo.Properties["Repository"]);
            Assert.AreEqual("https://tfs.contoso.com:8080/tfs/", repositoryInfo.Properties["OrganizationUrl"]);
            Assert.AreEqual("https://tfs.contoso.com:8080/tfs/Contoso/_git/MyRepository/", repositoryInfo.Properties["RepositoryUrl"]);

            Assert.IsTrue(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://tfs.contoso.com:8080/tfs/Contoso/_git/MyRepository?foor=bar", CancellationToken.None, out repositoryInfo));
            Assert.AreEqual("tfs.contoso.com", repositoryInfo.Properties["Organization"]);
            Assert.AreEqual("Contoso", repositoryInfo.Properties["Project"]);
            Assert.AreEqual("MyRepository", repositoryInfo.Properties["Repository"]);
            Assert.AreEqual("https://tfs.contoso.com:8080/tfs/", repositoryInfo.Properties["OrganizationUrl"]);
            Assert.AreEqual("https://tfs.contoso.com:8080/tfs/Contoso/_git/MyRepository/", repositoryInfo.Properties["RepositoryUrl"]);
        }

        [TestMethod]
        public async Task BadUrisAsync()
        {
            var azureDevOps = new AzureDevOps(null);
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/Hello", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/Hello/", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/Hello/_git", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/Hello/_git/", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/_git", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/_git/", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/_git/Hello", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/Hello?dfghfhg=fghj", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/Hello/?dfghfhg=fghj", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/Hello/_git?dfghfhg=fghj", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/Hello/_git/?dfghfhg=fghj", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/_git?dfghfhg=fghj", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/_git/?dfghfhg=fghj", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/_git/Hello?dfghfhg=fghj", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/Contoso/_git/Hello/?dfghfhg=fghj", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://contoso.visualstudio.com/", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "https://dev.azure.com/{organization}/", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "http://www.bing.com", CancellationToken.None, out _));
            Assert.IsFalse(await azureDevOps.TryDetectRepositoryInformationAsync(string.Empty, "http://www.bing.com:8080", CancellationToken.None, out _));
        }
    }
}

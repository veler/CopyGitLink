#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Def.Models;
using Microsoft;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CopyGitLink.Shared.Core.GitOnlineServices
{
    [Export(typeof(IGitOnlineService))]
    internal sealed class GitHubnLab : IGitOnlineService
    {
        private const string Organization = "Organization";
        private const string Repository = "Repository";
        private const string Host = "Host";
        private const string RemoteGitEnding = ".git";
        private const string UrlPrefixGit = "git";
        private const string UrlPrefixSsh = "ssh";

        private readonly IGitCommandService _gitCommandService;

        [ImportingConstructor]
        internal GitHubnLab(IGitCommandService gitCommandService)
        {
            _gitCommandService = gitCommandService;
        }

        public Task<bool> TryDetectRepositoryInformationAsync(
            string repositoryFolder,
            string repositoryUri,
            CancellationToken cancellationToken,
            out RepositoryInfo? repositoryInfo)
        {
            if (!TryParseGitUri(repositoryUri, out IDictionary<string, string>? properties)
                || properties == null)
            {
                repositoryInfo = null;
                return Task.FromResult(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            repositoryInfo = new RepositoryInfo(repositoryUri, properties, this);
            return Task.FromResult(true);
        }

        public async Task<string> GenerateLinkAsync(
            string repositoryFolder,
            RepositoryInfo repositoryInfo,
            string filePath,
            long? startLineNumber = null,
            long? startColumnNumber = null,
            long? endLineNumber = null,
            long? endColumnNumber = null)
        {
            Requires.NotNullOrEmpty(repositoryFolder, nameof(repositoryFolder));
            Requires.NotNull(repositoryInfo, nameof(repositoryInfo));
            Requires.NotNullOrEmpty(filePath, nameof(filePath));

            string branchName
                = Uri.EscapeDataString(
                    await _gitCommandService.GetBestRemoteGitBranchAsync(repositoryFolder)
                    .ConfigureAwait(false));

            var relativePath
                = Uri.EscapeDataString(
                    filePath.Substring(repositoryFolder.Length))
                .Replace("%5C", "/");

            var repositoryName = repositoryInfo.Properties[Repository];
            var organization = repositoryInfo.Properties[Organization];
            var host = repositoryInfo.Properties[Host];

            Requires.NotNullOrEmpty(organization, nameof(organization));
            Requires.NotNullOrEmpty(repositoryName, nameof(repositoryName));
            Requires.NotNullOrEmpty(host, nameof(host));
            Requires.NotNullOrEmpty(branchName, nameof(branchName));

            // Link to a file without line to select.
            string url = $"https://{host}/{organization}/{repositoryName}/blob/{branchName}/{relativePath}";

            if (startLineNumber.HasValue)
            {
                // Link to a file with line to select.
                url += $"#L{endLineNumber + 1}";
            }

            if (endLineNumber.HasValue && host.Contains("github"))
            {
                url += $"-L{startLineNumber + 1}";
            }

            return url;
        }

        /// <summary>
        /// Parses a GitHub/GitLab/Self-Managed Url and returns information detected from this Url.
        /// </summary>
        /// <param name="repositoryUriString">A string that is supposed to be a SCM Url.</param>
        /// <param name="properties">Returns a dictionary containing info about the <paramref name="repositoryUriString"/>.</param>
        /// <returns>Returns <c>False</c> if <paramref name="repositoryUriString"/> is not a valid SCM Url.</returns>
        private bool TryParseGitUri(string repositoryUriString, out IDictionary<string, string>? properties)
        {
            if (string.IsNullOrWhiteSpace(repositoryUriString))
            {
                properties = null;
                return false;
            }

            if (repositoryUriString.StartsWith(UrlPrefixSsh, StringComparison.OrdinalIgnoreCase) ||
                repositoryUriString.StartsWith(UrlPrefixGit, StringComparison.OrdinalIgnoreCase))
            {
                repositoryUriString = ConvertSshUrlToHttpUrl(repositoryUriString);
            }

            Uri repositoryUri;
            try
            {
                repositoryUri = new Uri(repositoryUriString);
            }
            catch
            {
                // The string isn't a URI.
                properties = null;
                return false;
            }

            // Detect the scheme. We only accept http and https.
            if (!string.Equals(repositoryUri.Scheme, "http", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(repositoryUri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                properties = null;
                return false;
            }

            properties = new Dictionary<string, string>();

            // The host can be github, gitlab or a Self-Managed version of both.
            // It will be in the form of 
            // https://{github|gitlab|Self-Managed}.{extension}/{org or user}/{repo name}.git
            // Must have .git uri ending
            var repositoryNameIndex = repositoryUri.Segments.Length - 1;
            if (repositoryUri.Segments[repositoryNameIndex].IndexOf(RemoteGitEnding, StringComparison.Ordinal) > 0)
            {
                // Trims the .git suffix
                properties[Host] = repositoryUri.Host;
                properties[Repository] = repositoryUri.Segments[repositoryNameIndex].Substring(0, repositoryUri.Segments[repositoryNameIndex].Length - 4);
                var organizationInfo = repositoryUri.Segments
                    .Skip(1)
                    .Take(repositoryUri.Segments.Length - 2);

                properties[Organization] = string.Join("", organizationInfo).TrimEnd('/');
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// convert github, gitlab or self-managed version repository SSH url to http url
        /// </summary>
        /// <param name="repositoryUriString"></param>
        /// <returns></returns>
        private string ConvertSshUrlToHttpUrl(string repositoryUriString)
        {
            if (repositoryUriString.Length > 4)
            {
                return $"http://{repositoryUriString.Substring(4).Replace(':', '/')}";
            }
            return repositoryUriString;
        }
    }
}

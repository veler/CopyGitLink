#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Def.Models;
using Microsoft;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CopyGitLink.Shared.Core.GitOnlineServices
{
    [Export(typeof(IGitOnlineService))]
    internal sealed class GitHub : IGitOnlineService
    {
        private const string Organization = "Organization";
        private const string Repository = "Repository";
        private const string RemoteGitEnding = ".git";

        private readonly IGitCommandService _gitCommandService;

        [ImportingConstructor]
        internal GitHub(IGitCommandService gitCommandService)
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

            Requires.NotNullOrEmpty(organization, nameof(organization));
            Requires.NotNullOrEmpty(repositoryName, nameof(repositoryName));
            Requires.NotNullOrEmpty(branchName, nameof(branchName));

            // Link to a file without line to select.
            string url = $"https://github.com/{organization}/{repositoryName}/blob/{branchName}/{relativePath}";

            if (startLineNumber.HasValue && endLineNumber.HasValue)
            {
                // Link to a file with line to select.
                url += $"#L{startLineNumber + 1}-L{endLineNumber + 1}";
            }

            return url;
        }

        /// <summary>
        /// Parses a GitHub Url and returns information detected from this Url.
        /// </summary>
        /// <param name="repositoryUriString">A string that is supposed to be an GitHub Url.</param>
        /// <param name="properties">Returns a dictionary containing info about the <paramref name="repositoryUriString"/>.</param>
        /// <returns>Returns <c>False</c> if <paramref name="repositoryUriString"/> is not a valid GitHub Url.</returns>
        private bool TryParseGitUri(string repositoryUriString, out IDictionary<string, string>? properties)
        {
            if (string.IsNullOrWhiteSpace(repositoryUriString))
            {
                properties = null;
                return false;
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

            // Detect if the host corresponds to GitHub.
            // It will be in the form of 
            // https://github.com/{org or user}/{repo name}.git
            if (string.Equals(repositoryUri.Host, "github.com", StringComparison.OrdinalIgnoreCase)
                && repositoryUri.Segments.Length == 3)
            {
                // Must have .git uri ending
                if (repositoryUri.Segments[2].IndexOf(RemoteGitEnding, StringComparison.Ordinal) > 0)
                {
                    // Trims the .git suffix
                    properties[Repository] = repositoryUri.Segments[2].Substring(0, repositoryUri.Segments[2].Length - 4);
                }
                else
                {
                    return false;
                }

                properties[Organization] = repositoryUri.Segments[1].TrimEnd('/');
                return true;
            }

            properties = null;
            return false;
        }
    }
}

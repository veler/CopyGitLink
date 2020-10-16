#nullable enable

using Microsoft;
using System.Collections.Generic;

namespace CopyGitLink.Def.Models
{
    /// <summary>
    /// Represents a repository and its information.
    /// </summary>
    public sealed class RepositoryInfo
    {
        /// <summary>
        /// Gets the Uri of repository online.
        /// </summary>
        public string Uri { get; }

        /// <summary>
        /// Gets some properties specific to the Git online service provider.
        /// </summary>
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Gets the Git online service provider associated to this repository.
        /// </summary>
        public IGitOnlineService Service { get; }

        public RepositoryInfo(string uri, IDictionary<string, string> properties, IGitOnlineService service)
        {
            Requires.NotNullOrEmpty(uri, nameof(uri));
            Uri = uri;
            Properties = Requires.NotNull(properties, nameof(properties));
            Service = Requires.NotNull(service, nameof(service));
        }
    }
}

#nullable enable

namespace CopyGitLink.Def
{
    /// <summary>
    /// Provides a set of methods to handle item selection in Solution Explorer (solution and folder view).
    /// It will also listens to selected items and try to discover repositories asynchronously.
    /// </summary>
    public interface ISolutionExplorerSelectionService
    {
        /// <summary>
        /// Gets the full path of the current selected item in the Solution Explorer.
        /// </summary>
        string CurrentSelectedItemFullPath { get; }
    }
}

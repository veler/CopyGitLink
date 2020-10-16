#nullable enable

using Microsoft.VisualStudio.Shell.Interop;

namespace CopyGitLink.Def
{
    /// <summary>
    /// Provides a set of methods to manage opened documents.
    /// It will also listens to opened document to try to discover repositories asynchronously.
    /// </summary>
    public interface IOpenedDocumentService
    {
        /// <summary>
        /// Gets the full path of the active document in the IDE.
        /// </summary>
        /// <returns>Returns the full path to the document.</returns>
        string GetActiveDocumentFullPath();

        /// <summary>
        /// Gets the frame of the active document in the IDE.
        /// </summary>
        /// <returns>Returns the <see cref="IVsWindowFrame"/> of the active document.</returns>
        IVsWindowFrame? GetActiveDocumentFrame();
    }
}

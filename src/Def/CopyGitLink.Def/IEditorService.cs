#nullable enable

using Microsoft.VisualStudio.TextManager.Interop;

namespace CopyGitLink.Def
{
    /// <summary>
    /// Provides a set of methods to handle the text editor of the IDE.
    /// </summary>
    public interface IEditorService
    {
        /// <summary>
        /// Gets the text view of the active document.
        /// </summary>
        IVsTextView? GetActiveDocumentTextView();
    }
}

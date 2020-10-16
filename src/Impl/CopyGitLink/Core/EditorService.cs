#nullable enable

using CopyGitLink.Def;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System.ComponentModel.Composition;

namespace CopyGitLink.Core
{
    [Export(typeof(IEditorService))]
    internal sealed class EditorService : IEditorService
    {
        private readonly IOpenedDocumentService _openedDocumentService;

        [ImportingConstructor]
        internal EditorService(IOpenedDocumentService openedDocumentService)
        {
            _openedDocumentService = openedDocumentService;
        }

        public IVsTextView? GetActiveDocumentTextView()
        {
            IVsWindowFrame? windowFrame = _openedDocumentService.GetActiveDocumentFrame();

            if (windowFrame != null)
            {
                IVsTextView? textView = VsShellUtilities.GetTextView(windowFrame);
                return textView;
            }

            return null;
        }
    }
}

#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Shared;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;

namespace CopyGitLink.Commands
{
    [Export(typeof(ICommandBase))]
    internal sealed class CopyLinkToSelectionCommand : CommandBase
    {
        private readonly IOpenedDocumentService _openedDocumentService;
        private readonly IRepositoryService _repositoryService;
        private readonly ICopyLinkService _copyLinkService;
        private readonly IEditorService _editorService;

        protected override int CommandId => PkgIds.CopyLinkToSelectionCommandId;

        protected override Guid CommandSet => PkgIds.ContextMenuCommandSetGuid;

        [ImportingConstructor]
        public CopyLinkToSelectionCommand(
            IOpenedDocumentService openedDocumentService,
            IRepositoryService repositoryService,
            ICopyLinkService copyLinkService,
            IEditorService editorService)
            : base()
        {
            _openedDocumentService = openedDocumentService;
            _repositoryService = repositoryService;
            _copyLinkService = copyLinkService;
            _editorService = editorService;
        }

        protected override void BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var command = (OleMenuCommand)sender;

            string activeDocumentFilePath = _openedDocumentService.GetActiveDocumentFullPath();

            if (_repositoryService.IsFilePartOfKnownRepository(activeDocumentFilePath))
            {
                command.Visible = true;
                command.Enabled = true;
            }
            else
            {
                command.Visible = false;
                command.Enabled = false;
            }
        }

        protected override void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string activeDocumentFilePath = _openedDocumentService.GetActiveDocumentFullPath();

            if (string.IsNullOrEmpty(activeDocumentFilePath))
            {
                return;
            }

            IVsTextView? textEditor = _editorService.GetActiveDocumentTextView();

            if (textEditor != null
                && ErrorHandler.Succeeded(
                        textEditor.GetSelection(
                            out int startLineNumber,
                            out int startColumnNumber,
                            out int endLineNumber,
                            out int endColumnNumber)))
            {
                _copyLinkService.GenerateLinkAsync(
                    "CopyToSelection",
                    activeDocumentFilePath,
                    startLineNumber,
                    startColumnNumber,
                    endLineNumber,
                    endColumnNumber,
                    copyToClipboard: true)
                    .Forget();
            }
        }
    }
}

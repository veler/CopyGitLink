#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Dialogs;
using CopyGitLink.Shared;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;
using System.IO;

namespace CopyGitLink.Commands
{
    [Export(typeof(ICommandBase))]
    internal sealed class CopyLinkToSelectionCommand : CommandBase
    {
        private readonly IRepositoryService _repositoryService;
        private readonly IOpenedDocumentService _openedDocumentService;
        private readonly ICopyLinkService _copyLinkService;
        private readonly IEditorService _editorService;

        protected override int CommandId => PkgIds.CopyLinkToSelectionCommandId;

        protected override Guid CommandSet => PkgIds.ContextMenuCommandSetGuid;

        [ImportingConstructor]
        public CopyLinkToSelectionCommand(
            IRepositoryService repositoryService,
            IOpenedDocumentService openedDocumentService,
            ICopyLinkService copyLinkService,
            IEditorService editorService)
            : base()
        {
            _repositoryService = repositoryService;
            _openedDocumentService = openedDocumentService;
            _copyLinkService = copyLinkService;
            _editorService = editorService;
        }

        protected override void BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var command = (OleMenuCommand)sender;

            bool activeDocumentFullPathFound = !string.IsNullOrEmpty(_openedDocumentService.GetActiveSolutionDocumentFullPath());

            // Always enable Copy Git Link menu when the active document is part of a solution (not miscellaneous), even if there
            // is no Git repository.
            command.Visible = activeDocumentFullPathFound;
            command.Enabled = activeDocumentFullPathFound;
        }

        protected override void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string activeDocumentFilePath = _openedDocumentService.GetActiveSolutionDocumentFullPath();

            if (string.IsNullOrEmpty(activeDocumentFilePath))
            {
                return;
            }

            if (!_repositoryService.IsFilePartOfKnownRemoteRepository(activeDocumentFilePath))
            {
                _repositoryService.StartListeningForRepositoryCreation(Directory.GetParent(activeDocumentFilePath).FullName);
                var dialog = new CreateGitRepositoryDialog();
                dialog.ShowDialog();
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

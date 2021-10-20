#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Dialogs;
using CopyGitLink.Shared;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;
using System.IO;

namespace CopyGitLink.Commands
{
    [Export(typeof(ICommandBase))]
    internal sealed class CopyLinkToThisFileEditorCommand : CommandBase
    {
        private readonly IOpenedDocumentService _openedDocumentService;
        private readonly IRepositoryService _repositoryService;
        private readonly ICopyLinkService _copyLinkService;

        protected override int CommandId => PkgIds.CopyLinkToThisFileEditorCommandId;

        protected override Guid CommandSet => PkgIds.ContextMenuCommandSetGuid;

        [ImportingConstructor]
        internal CopyLinkToThisFileEditorCommand(
            IOpenedDocumentService openedDocumentService,
            IRepositoryService repositoryService,
            ICopyLinkService copyLinkService)
        {
            _openedDocumentService = openedDocumentService;
            _repositoryService = repositoryService;
            _copyLinkService = copyLinkService;
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

            _copyLinkService.GenerateLinkAsync(
                "CopyToFileFromEditorTab",
                activeDocumentFilePath,
                copyToClipboard: true)
                .Forget();
        }
    }
}

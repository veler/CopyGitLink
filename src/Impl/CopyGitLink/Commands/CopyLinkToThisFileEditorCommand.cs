#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Shared;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;

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

            var activeDocumentFullFilePath = _openedDocumentService.GetActiveDocumentFullPath();

            if (_repositoryService.IsFilePartOfKnownRepository(activeDocumentFullFilePath))
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

            _copyLinkService.GenerateAndCopyLinkAsync(
                "CopyToFileFromEditorTab",
                activeDocumentFilePath)
                .Forget();
        }
    }
}

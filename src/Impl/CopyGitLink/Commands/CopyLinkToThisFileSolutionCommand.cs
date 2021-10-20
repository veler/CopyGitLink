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
    internal class CopyLinkToThisFileSolutionCommand : CommandBase
    {
        private readonly ISolutionExplorerSelectionService _solutionExplorerSelectionService;
        private readonly IRepositoryService _repositoryService;
        private readonly ICopyLinkService _copyLinkService;

        protected override int CommandId => PkgIds.CopyLinkToThisFileSolutionCommandId;

        protected override Guid CommandSet => PkgIds.ContextMenuCommandSetGuid;

        [ImportingConstructor]
        internal CopyLinkToThisFileSolutionCommand(
            ISolutionExplorerSelectionService solutionExplorerSelectionService,
            IRepositoryService repositoryService,
            ICopyLinkService copyLinkService)
        {
            _solutionExplorerSelectionService = solutionExplorerSelectionService;
            _repositoryService = repositoryService;
            _copyLinkService = copyLinkService;
        }

        protected override void BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var command = (OleMenuCommand)sender;

            bool currentSolutionExplorerSelectedItemFullPath = !string.IsNullOrEmpty(_solutionExplorerSelectionService.CurrentSelectedItemFullPath);

            // Always enable Copy Git Link menu, even if there is no Git repository. But don't enable the menu if the selected item is a miscellaneous file.
            command.Visible = currentSolutionExplorerSelectedItemFullPath;
            command.Enabled = currentSolutionExplorerSelectedItemFullPath;
        }

        protected override void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string currentSolutionExplorerSelectedItemFullPath = _solutionExplorerSelectionService.CurrentSelectedItemFullPath;

            if (string.IsNullOrEmpty(currentSolutionExplorerSelectedItemFullPath))
            {
                return;
            }

            if (!_repositoryService.IsFilePartOfKnownRemoteRepository(currentSolutionExplorerSelectedItemFullPath))
            {
                _repositoryService.StartListeningForRepositoryCreation(Directory.GetParent(currentSolutionExplorerSelectedItemFullPath).FullName);
                var dialog = new CreateGitRepositoryDialog();
                dialog.ShowDialog();
                return;
            }

            _copyLinkService.GenerateLinkAsync(
                "CopyToFileFromSolutionExplorer",
                currentSolutionExplorerSelectedItemFullPath,
                copyToClipboard: true)
                .Forget();
        }
    }
}

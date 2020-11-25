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

            string currentSolutionExplorerSelectedItemFullPath = _solutionExplorerSelectionService.CurrentSelectedItemFullPath;

            if (_repositoryService.IsFilePartOfKnownRepository(currentSolutionExplorerSelectedItemFullPath))
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

            string currentSolutionExplorerSelectedItemFullPath = _solutionExplorerSelectionService.CurrentSelectedItemFullPath;

            if (string.IsNullOrEmpty(currentSolutionExplorerSelectedItemFullPath))
            {
                return;
            }

            _copyLinkService.GenerateAndCopyLinkAsync(
                "CopyToFileFromSolutionExplorer",
                currentSolutionExplorerSelectedItemFullPath)
                .Forget();
        }
    }
}

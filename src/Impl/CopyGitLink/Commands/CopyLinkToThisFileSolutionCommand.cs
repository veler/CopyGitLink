#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Def.Models;
using CopyGitLink.Shared;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace CopyGitLink.Commands
{
    [Export(typeof(ICommandBase))]
    internal class CopyLinkToThisFileSolutionCommand : CommandBase
    {
        private readonly ISolutionExplorerSelectionService _solutionExplorerSelectionService;
        private readonly IRepositoryService _repositoryService;

        protected override int CommandId => PkgIds.CopyLinkToThisFileSolutionCommandId;

        protected override Guid CommandSet => PkgIds.ContextMenuCommandSetGuid;

        [ImportingConstructor]
        internal CopyLinkToThisFileSolutionCommand(
            ISolutionExplorerSelectionService solutionExplorerSelectionService,
            IRepositoryService repositoryService)
        {
            _solutionExplorerSelectionService = solutionExplorerSelectionService;
            _repositoryService = repositoryService;
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

            Task.Run(async () =>
            {
                if (_repositoryService.TryGetKnownRepository(currentSolutionExplorerSelectedItemFullPath, out string repositoryFolder, out RepositoryInfo? repositoryInfo)
                    && repositoryInfo != null)
                {
                    string url
                        = await repositoryInfo.Service.GenerateLinkAsync(
                            repositoryFolder,
                            repositoryInfo,
                            currentSolutionExplorerSelectedItemFullPath)
                        .ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(url))
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        Clipboard.SetText(url);
                    }
                }
            }).Forget();
        }
    }
}

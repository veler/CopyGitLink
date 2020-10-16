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
    internal sealed class CopyLinkToThisFileEditorCommand : CommandBase
    {
        private readonly IOpenedDocumentService _openedDocumentService;
        private readonly IRepositoryService _repositoryService;

        protected override int CommandId => PkgIds.CopyLinkToThisFileEditorCommandId;

        protected override Guid CommandSet => PkgIds.ContextMenuCommandSetGuid;

        [ImportingConstructor]
        internal CopyLinkToThisFileEditorCommand(
            IOpenedDocumentService openedDocumentService,
            IRepositoryService repositoryService)
        {
            _openedDocumentService = openedDocumentService;
            _repositoryService = repositoryService;
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

            Task.Run(async () =>
            {
                if (_repositoryService.TryGetKnownRepository(activeDocumentFilePath, out string repositoryFolder, out RepositoryInfo? repositoryInfo)
                    && repositoryInfo != null)
                {
                    string url
                        = await repositoryInfo.Service.GenerateLinkAsync(
                            repositoryFolder,
                            repositoryInfo,
                            activeDocumentFilePath)
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

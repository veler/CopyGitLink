#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Def.Models;
using CopyGitLink.Shared;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace CopyGitLink.Commands
{
    [Export(typeof(ICommandBase))]
    internal sealed class CopyLinkToSelectionCommand : CommandBase
    {
        private readonly IOpenedDocumentService _openedDocumentService;
        private readonly IRepositoryService _repositoryService;
        private readonly IEditorService _editorService;

        protected override int CommandId => PkgIds.CopyLinkToSelectionCommandId;

        protected override Guid CommandSet => PkgIds.ContextMenuCommandSetGuid;

        [ImportingConstructor]
        public CopyLinkToSelectionCommand(
            IOpenedDocumentService openedDocumentService,
            IRepositoryService repositoryService,
            IEditorService editorService)
            : base()
        {
            _openedDocumentService = openedDocumentService;
            _repositoryService = repositoryService;
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
                Task.Run(async () =>
                {
                    if (_repositoryService.TryGetKnownRepository(activeDocumentFilePath, out string repositoryFolder, out RepositoryInfo? repositoryInfo)
                        && repositoryInfo != null)
                    {
                        // if we do a selection from the bottom to the top, the endLineNumber and startLineNumber are inverted
                        if (startLineNumber > endLineNumber)
                        {
                            (startLineNumber, endLineNumber) = SwapLineNumber(startLineNumber, endLineNumber);
                        }
                        string url = await repositoryInfo.Service.GenerateLinkAsync(
                               repositoryFolder,
                               repositoryInfo,
                               activeDocumentFilePath,
                               startLineNumber,
                               startColumnNumber,
                               endLineNumber,
                               endColumnNumber)
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

        private (int, int) SwapLineNumber(int startLineNumber, int endLineNumber)
        {
            return (endLineNumber, startLineNumber);
        }
    }
}

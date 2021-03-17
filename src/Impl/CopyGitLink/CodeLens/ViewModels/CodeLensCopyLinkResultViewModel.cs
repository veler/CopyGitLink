#nullable enable

using CopyGitLink.Def;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CopyGitLink.CodeLens.ViewModels
{
    public sealed class CodeLensCopyLinkResultViewModel : INotifyPropertyChanged
    {
        private readonly ICopyLinkService _copyLinkService;

        private string? _url;
        private bool _linkGenerated;

        /// <summary>
        /// Gets the generated Url.
        /// </summary>
        public string? Url
        {
            get => _url;
            private set
            {
                _url = value;
                RaisePropertyChangedAsync().Forget();
            }
        }

        /// <summary>
        /// Gets whether the link has been generated.
        /// </summary>
        public bool LinkGenerated
        {
            get => _linkGenerated;
            private set
            {
                _linkGenerated = value;
                RaisePropertyChangedAsync().Forget();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public CodeLensCopyLinkResultViewModel(ICopyLinkService copyLinkService, ITextView textView, Span applicableSpan)
        {
            _copyLinkService = copyLinkService;

            CopyToClipboardCommand = new ActionCommand(ExecuteCopyToClipboardCommand);

            GenerateLinkAsync(textView, applicableSpan).Forget();
        }

        #region CopyToClipboardCommand

        public ActionCommand CopyToClipboardCommand { get; }

        private void ExecuteCopyToClipboardCommand()
        {
            if (Url != null && !string.IsNullOrEmpty(Url))
            {
                _copyLinkService.PushToClipboardAsync(Url).Forget();
            }
        }

        #endregion

        private async Task GenerateLinkAsync(ITextView textView, Span applicableSpan)
        {
            DateTime startTime = DateTime.Now;

            // Switch to background thread on purpose to avoid blocking the main thread.
            await TaskScheduler.Default;

            if (textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument  textDocument))
            {
                string filePath = textDocument.FilePath;
                ITextSnapshotLine startLine = textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(applicableSpan.Start);
                ITextSnapshotLine endLine = textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(applicableSpan.Start + applicableSpan.Length);

                if (startLine != null
                    && endLine != null
                    && !string.IsNullOrEmpty(filePath))
                {
                    string url
                        = await _copyLinkService.GenerateLinkAsync(
                            "CodeLens",
                            filePath,
                            startLine.LineNumber,
                            startColumnNumber: 0,
                            endLine.LineNumber,
                            endColumnNumber: endLine.Length,
                            copyToClipboard: false)
                        .ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        Url = url;
                    }
                }
            }

            // Make sure the method took at least 1 sec to run.
            // We do this so the users sees the "generating link in progress" experience which forces the user
            // to slow down so he realizes he needs do to a secondary click to get the generated URL to the clipboard.
            // We're not sending the URL to the clipboard as soon as we generated it because of too
            // many misclicks on the CodeLens button.
            DateTime endTime = DateTime.Now;
            TimeSpan timeLeft = TimeSpan.FromSeconds(1) - (endTime - startTime);
            if (timeLeft > TimeSpan.Zero)
            {
                await Task.Delay(timeLeft).ConfigureAwait(false);
            }

            LinkGenerated = true;
        }

        private async Task RaisePropertyChangedAsync([CallerMemberName] string? propertyName = null)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

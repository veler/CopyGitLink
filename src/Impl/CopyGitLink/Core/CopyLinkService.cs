#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Def.Models;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Telemetry;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;

namespace CopyGitLink.Core
{
    [Export(typeof(ICopyLinkService))]
    internal sealed class CopyLinkService : ICopyLinkService
    {
        private const string CopyGitLinkEvent = "vs/ide/copygitlink/generateandcopylink";
        private const string CopyGitLinkFaultEvent = "vs/ide/copygitlink/generateandcopylinkfault";
        private const string GitServiceNamePropertyName = "CopyGitLink.ServiceName";
        private const string CallerCommandNamePropertyName = "CopyGitLink.CallerCommandName";

        private readonly IRepositoryService _repositoryService;

        [ImportingConstructor]
        internal CopyLinkService(IRepositoryService repositoryService)
        {
            _repositoryService = repositoryService;
        }

        public async Task<string> GenerateLinkAsync(
            string callerCommandName,
            string filePath,
            long? startLineNumber = null,
            long? startColumnNumber = null,
            long? endLineNumber = null,
            long? endColumnNumber = null,
            bool copyToClipboard = true)
        {
            try
            {
                Requires.NotNullOrWhiteSpace(callerCommandName, nameof(callerCommandName));
                Requires.NotNullOrWhiteSpace(filePath, nameof(filePath));

                // Make sure we get out of the UI thread.
                await TaskScheduler.Default;
                ThreadHelper.ThrowIfOnUIThread();

                if (_repositoryService.TryGetKnownRepository(filePath, out string repositoryFolder, out RepositoryInfo? repositoryInfo)
                    && repositoryInfo != null)
                {
                    // if we do a selection from the bottom to the top, the endLineNumber and startLineNumber are inverted
                    if (startLineNumber > endLineNumber)
                    {
                        (startLineNumber, endLineNumber, startColumnNumber, endColumnNumber)
                            = SwapLineNumber(startLineNumber, endLineNumber, startColumnNumber, endColumnNumber);
                    }

                    // generate a link
                    string url = await repositoryInfo.Service.GenerateLinkAsync(
                           repositoryFolder,
                           repositoryInfo,
                           filePath,
                           startLineNumber,
                           startColumnNumber,
                           endLineNumber,
                           endColumnNumber)
                        .ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(url))
                    {
                        Log(callerCommandName, repositoryInfo.Service.ServiceName);

                        if (copyToClipboard)
                        {
                            await PushToClipboardAsync(url).ConfigureAwait(false);
                        }
                        return url;
                    }
                }
            }
            catch (Exception ex)
            {
                var telemetryEvent = new FaultEvent(CopyGitLinkFaultEvent, ex.Message, ex);
                TelemetryService.DefaultSession.PostEvent(telemetryEvent);
            }

            return string.Empty;
        }

        public async System.Threading.Tasks.Task PushToClipboardAsync(string url)
        {
            Requires.NotNullOrEmpty(url, nameof(url));

            // copy the link to the clipboard. UI Thread is required.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Clipboard.SetText(url);
        }

        private (long?, long?, long?, long?) SwapLineNumber(long? startLineNumber, long? endLineNumber, long? startColumnNumber, long? endColumnNumber)
        {
            return (endLineNumber, startLineNumber, endColumnNumber, startColumnNumber);
        }

        private void Log(string callerCommandName, string gitServiceName)
        {
            var telemetryEvent = new TelemetryEvent(CopyGitLinkEvent);
            telemetryEvent.Properties.Add(CallerCommandNamePropertyName, callerCommandName);
            telemetryEvent.Properties.Add(GitServiceNamePropertyName, gitServiceName);

            TelemetryService.DefaultSession.PostEvent(telemetryEvent);
        }
    }
}

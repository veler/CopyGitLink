#nullable enable

using CopyGitLink.Def;
using Microsoft;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;

namespace CopyGitLink.Core
{
    [Export(typeof(IOpenedDocumentService))]
    internal sealed class OpenedDocumentService : IOpenedDocumentService, IVsWindowFrameEvents, IDisposable
    {
        private readonly JoinableTaskFactory _joinableTaskFactory;
        private readonly SVsServiceProvider _serviceProvider;
        private readonly IRepositoryService _repositoryService;

        private IVsMonitorSelection? _monitorSelection;
        private IVsUIShell? _uiShell;
        private IVsUIShell7? _uiShell7;
        private uint _uiShellCookie = VSConstants.VSCOOKIE_NIL;

        [ImportingConstructor]
        internal OpenedDocumentService(
            SVsServiceProvider serviceProvider,
            JoinableTaskContext joinableTaskContext,
            IRepositoryService repositoryService)
        {
            _serviceProvider = serviceProvider;
            _joinableTaskFactory = joinableTaskContext.Factory;
            _repositoryService = repositoryService;

            StartListeningToWindowFrameEventsAsync().Forget();
        }

        public IVsWindowFrame? GetActiveDocumentFrame()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_monitorSelection != null
                && ErrorHandler.Succeeded(_monitorSelection.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out var frame)))
            {
                if (frame is IVsWindowFrame windowFrame)
                {
                    return windowFrame;
                }
            }

            return null;
        }

        public string GetActiveSolutionDocumentFullPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsWindowFrame? windowFrame = GetActiveDocumentFrame();

            if (windowFrame != null)
            {
                return GetDocumentNameFromWindowFrame(windowFrame);
            }

            return string.Empty;
        }

        public void OnFrameCreated(IVsWindowFrame frame)
        {
            // We can't do this immediately since not everything is up to date at the time the frame event is raised.
            // So we use SwitchToMainThreadAsync(alwaysYield: true) which will get the UI thread once it's on idle (and so that the frame is ready).
            _ = _joinableTaskFactory.RunAsync(async delegate
            {
                await _joinableTaskFactory.SwitchToMainThreadAsync(alwaysYield: true);

                string documentFullPath = GetDocumentNameFromWindowFrame(frame);
                QueueDocumentForRepositoryDiscovery(documentFullPath);
            });
        }

        public void OnFrameDestroyed(IVsWindowFrame frame)
        {
        }

        public void OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible)
        {
        }

        public void OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen)
        {
        }

        public void OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame)
        {
        }

        public void Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            StopListeningToWindowFrameEvents();
        }

        private void EnsureServicesInitialized()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_uiShell7 == null)
            {
                _uiShell = _serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
                Assumes.Present(_uiShell);
                _uiShell7 = _uiShell as IVsUIShell7;
            }

            if (_monitorSelection == null)
            {
                _monitorSelection = _serviceProvider.GetService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
                Assumes.Present(_monitorSelection);
            }
        }

        private async System.Threading.Tasks.Task StartListeningToWindowFrameEventsAsync()
        {
            if (_uiShellCookie == VSConstants.VSCOOKIE_NIL || _uiShell7 == null)
            {
                await _joinableTaskFactory.SwitchToMainThreadAsync();

                EnsureServicesInitialized();

                if (_uiShellCookie == VSConstants.VSCOOKIE_NIL && _uiShell7 != null)
                {
                    _uiShellCookie = _uiShell7.AdviseWindowFrameEvents(this);
                }
            }

            QueueAllOpenedDocuments();
        }

        private void StopListeningToWindowFrameEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_uiShellCookie != VSConstants.VSCOOKIE_NIL && _uiShell7 != null)
            {
                _uiShell7.UnadviseWindowFrameEvents(_uiShellCookie);
                _uiShellCookie = VSConstants.VSCOOKIE_NIL;
            }
        }

        /// <summary>
        /// Queue all opened documents to detect their repository information.
        /// </summary>
        private void QueueAllOpenedDocuments()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Retrieve all opened documents.
            if (_uiShell != null
                && ErrorHandler.Succeeded(_uiShell.GetDocumentWindowEnum(out IEnumWindowFrames windowFramesEnum)))
            {
                windowFramesEnum.Reset(); // Just in case. We want to enumerate from the beginning.

                var frames = new IVsWindowFrame[1];

                while (ErrorHandler.Succeeded(windowFramesEnum.Next((uint)frames.Length, frames, out uint fetched)) && (fetched == 1))
                {
                    string documentFullPath = GetDocumentNameFromWindowFrame(frames[0]);
                    QueueDocumentForRepositoryDiscovery(documentFullPath);
                }
            }
        }

        private string GetDocumentNameFromWindowFrame(IVsWindowFrame windowFrame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the IVsHierarchy corresponding to the window frame.
            if (windowFrame != null
                && ErrorHandler.Succeeded(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_Hierarchy, out var hierarchyVar))
                && hierarchyVar is IVsHierarchy hierarchy)
            {
                // Get the project ID
                if (hierarchy is IPersist persist && ErrorHandler.Succeeded(persist.GetClassID(out Guid projectId)))
                {
                    // Make sure this isn't a Miscellaneous file
                    if (projectId != VSConstants.CLSID.MiscellaneousFilesProject_guid)
                    {
                        // Retrieve the full path of the document.
                        if (ErrorHandler.Succeeded(windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out var pvar))
                            && (pvar is string moniker)
                            && !string.IsNullOrWhiteSpace(moniker))
                        {
                            return moniker;
                        }
                    }
                }
            }

            return string.Empty;
        }

        private void QueueDocumentForRepositoryDiscovery(string documentFullPath)
        {
            if (string.IsNullOrEmpty(documentFullPath))
            {
                return;
            }

            _repositoryService.QueueRepositoryDiscovery(documentFullPath);
        }
    }
}

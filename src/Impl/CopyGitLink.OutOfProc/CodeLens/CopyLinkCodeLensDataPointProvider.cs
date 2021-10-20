#nullable enable

using CopyGitLink.Def;
using CopyGitLink.OutOfProc.Composition;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CopyGitLink.OutOfProc.CodeLens
{
    [Export(typeof(IAsyncCodeLensDataPointProvider))]
    [Name(nameof(CopyLinkCodeLensDataPointProvider))]
    [ContentType(StandardContentTypeNames.Code)]
    [Priority(1000)] // place it at the right of everything else.
    internal sealed class CopyLinkCodeLensDataPointProvider : IAsyncCodeLensDataPointProvider
    {
        private IRepositoryService? _repositoryService;

        public async Task<bool> CanCreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext descriptorContext, CancellationToken token)
        {
            // Initialize MEF to import our own interfaces.
            MefHostCompositionService.Instance.SatisfyImportsOnce(this);

            if (_repositoryService == null)
            {
                _repositoryService = MefHostCompositionService.Instance.Container.GetExportedValue<IRepositoryService>();
            }

            // Because we're out of proc here, the content of this instance of IRepositoryService is different
            // from the one from devenv.exe.
            // Therefore, we have to make sure we discover the repository here because the service in this
            // process may have not discovered it yet.
            // Additionally, since this method returns a boolean to determine whether the CodeLens provider should render
            // or not in the UI, we call DiscoverRepositoryAsync and wait that we finished to discover the repository.
            // This means we're blocking the thread and will do an IO access the very first time this method is call
            // (and potentially in the future too).
            await _repositoryService.DiscoverRepositoryAsync(descriptor.FilePath, token).ConfigureAwait(false);

            // Always enable Copy Git Link To's CodeLens menu, even if the current document isn't part of a repository.
            return true;
        }

        public Task<IAsyncCodeLensDataPoint?> CreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext descriptorContext, CancellationToken token)
        {
            return Task.FromResult<IAsyncCodeLensDataPoint?>(new CopyLinkCodeLensDataPoint(descriptor));
        }
    }
}

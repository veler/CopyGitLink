#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Def.Models;
using Microsoft;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CopyGitLink.OutOfProc.CodeLens
{
    internal sealed class CopyLinkCodeLensDataPoint : IAsyncCodeLensDataPoint
    {
        private readonly IRepositoryService _repositoryService;

        public CodeLensDescriptor Descriptor { get; }

        public event AsyncEventHandler? InvalidatedAsync;

        public CopyLinkCodeLensDataPoint(
            CodeLensDescriptor descriptor,
            IRepositoryService repositoryService)
        {
            Descriptor = Requires.NotNull(descriptor, nameof(descriptor));
            _repositoryService = repositoryService;
        }

        public Task<CodeLensDataPointDescriptor?> GetDataAsync(CodeLensDescriptorContext descriptorContext, CancellationToken token)
        {
            var response = new CodeLensDataPointDescriptor
            {
                Description = string.Format(Strings.CodeLensCopyLinkToThisMember, Descriptor.Kind.ToString().ToLower()),
                TooltipText = string.Format(Strings.CodeLensCopyLinkToThisMemberTooltip, Descriptor.Kind.ToString().ToLower()),
                IntValue = null, // no int value
                ImageId = new ImageId(KnownImageIds.ImageCatalogGuid, KnownImageIds.Link)
            };

            return Task.FromResult<CodeLensDataPointDescriptor?>(response);
        }

        public Task<CodeLensDetailsDescriptor> GetDetailsAsync(CodeLensDescriptorContext descriptorContext, CancellationToken token)
        {
            var args = new CodeLensCopyLinkResult
            {
                ApplicableSpan = descriptorContext.ApplicableSpan.GetValueOrDefault()
            };

            var response = new CodeLensDetailsDescriptor
            {
                Headers = new List<CodeLensDetailHeaderDescriptor>(),
                Entries = new List<CodeLensDetailEntryDescriptor>(),
                PaneNavigationCommands = new List<CodeLensDetailPaneCommand>(),
                CustomData = new List<CodeLensCopyLinkResult> { args }
            };

            return Task.FromResult(response);
        }
    }
}

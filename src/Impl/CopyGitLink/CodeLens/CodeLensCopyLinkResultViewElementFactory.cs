#nullable enable

using CopyGitLink.CodeLens.Views;
using CopyGitLink.Def;
using CopyGitLink.Def.Models;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace CopyGitLink.CodeLens
{
    [Export(typeof(IViewElementFactory))]
    [Name(nameof(CodeLensCopyLinkResultViewElementFactory))]
    [TypeConversion(from: typeof(CodeLensCopyLinkResult), to: typeof(FrameworkElement))]
    [Order]
    internal sealed class CodeLensCopyLinkResultViewElementFactory : IViewElementFactory
    {
        private readonly ICopyLinkService _copyLinkService;

        [ImportingConstructor]
        internal CodeLensCopyLinkResultViewElementFactory(
            ICopyLinkService copyLinkService)
        {
            _copyLinkService = copyLinkService;
        }

        public TView? CreateViewElement<TView>(ITextView textView, object model) where TView : class
        {
            // Should never happen if the service's code is correct, but it's good to be paranoid.
            if (typeof(FrameworkElement) != typeof(TView))
            {
                throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
            }

            if (model is CodeLensCopyLinkResult result)
            {
                var detailsUI = new CodeLensCopyLinkResultView(_copyLinkService, textView, result);
                return detailsUI as TView;
            }

            return null;
        }
    }
}

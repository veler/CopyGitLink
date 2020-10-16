#nullable enable

using System;
using System.Runtime.InteropServices;
using System.Threading;
using CopyGitLink.Def;
using CopyGitLink.Shared;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace CopyGitLink
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.FolderOpened_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PkgIds.PackageGuidString)]
    public sealed class CopyGitLinkPackage : AsyncPackage
    {
        internal static IServiceProvider? ServiceProvider { get; private set; }

        public CopyGitLinkPackage()
        {
            ServiceProvider = this;
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            IComponentModel componentModel = (IComponentModel)GetGlobalService(typeof(SComponentModel));

            // Load and initialize all commands.
            componentModel.GetExtensions<ICommandBase>();
        }
    }
}

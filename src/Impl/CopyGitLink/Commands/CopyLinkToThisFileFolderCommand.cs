#nullable enable

using CopyGitLink.Def;
using CopyGitLink.Shared;
using System.ComponentModel.Composition;

namespace CopyGitLink.Commands
{
    [Export(typeof(ICommandBase))]
    internal class CopyLinkToThisFileFolderCommand : CopyLinkToThisFileSolutionCommand
    {
        protected override int CommandId => PkgIds.CopyLinkToThisFileOpenFolderCommandId;

        [ImportingConstructor]
        internal CopyLinkToThisFileFolderCommand(
            ISolutionExplorerSelectionService solutionExplorerSelectionService,
            IRepositoryService repositoryService)
            : base(solutionExplorerSelectionService, repositoryService)
        {
        }
    }
}

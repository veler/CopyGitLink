#nullable enable

using System;

namespace CopyGitLink.Shared
{
    internal static class PkgIds
    {
        internal const string PackageGuidString = "11556a30-04ba-44b2-be8c-ec933438e34b";
        internal const string ContextMenuCommandSetGuidString = "dfc4ce1e-b94a-42c1-ba44-23d7e4d59d24";

        internal static readonly Guid ContextMenuCommandSetGuid = new Guid(ContextMenuCommandSetGuidString);

        internal const int CopyLinkToSelectionCommandId = 0x200;
        internal const int CopyLinkToThisFileEditorCommandId = 0x201;
        internal const int CopyLinkToThisFileSolutionCommandId = 0x202;
        internal const int CopyLinkToThisFileOpenFolderCommandId = 0x0213;
    }
}

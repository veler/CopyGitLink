#nullable enable

using System.Threading.Tasks;

namespace CopyGitLink.Def
{
    /// <summary>
    /// Provides a set of methods to generate and copy a link in the clipboard.
    /// </summary>
    public interface ICopyLinkService
    {
        /// <summary>
        /// Generates and copy a link for the given <paramref name="filePath"/>.
        /// </summary>
        /// <param name="callerCommandName">The name of the command or user action that ask to generate and copy a link. This is used for telemetry.</param>
        /// <param name="filePath">The path to the file for which a link should be generated.</param>
        /// <param name="startLineNumber">The line where the selection starts</param>
        /// <param name="startColumnNumber">The column where the selection starts</param>
        /// <param name="endLineNumber">The line where the selection ends</param>
        /// <param name="endColumnNumber">The column where the selection ends</param>
        /// <returns>Returns the Url that has been generated and copied, or null if no link has been generated.</returns>
        Task<string> GenerateLinkAsync(
            string callerCommandName,
            string filePath,
            long? startLineNumber = null,
            long? startColumnNumber = null,
            long? endLineNumber = null,
            long? endColumnNumber = null,
            bool copyToClipboard = true);

        /// <summary>
        /// Sends the given <paramref name="url"/> to the Windows' clipboard.
        /// </summary>
        /// <param name="url">A url</param>
        /// <returns>A task.</returns>
        Task PushToClipboardAsync(string url);
    }
}

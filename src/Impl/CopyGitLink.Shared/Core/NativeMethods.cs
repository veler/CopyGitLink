#nullable enable

using System.Runtime.InteropServices;
using System.Text;

namespace CopyGitLink.Shared.Core
{
    internal static class NativeMethods
    {
        /// <summary>
        /// Retrieves a string from the specified section in an initialization file (.INI).
        /// </summary>
        /// <remarks>https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring</remarks>
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        internal static extern int GetPrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            int nSize,
            string lpFileName);
    }
}

using System;
using System.Runtime.InteropServices;

namespace PlaywrightSharp.Helpers.Linux
{
    internal static class LinuxSysCall
    {
        internal const FileAccessPermissions ExecutableFilePermissions =
            FileAccessPermissions.UserRead |
            FileAccessPermissions.UserWrite |
            FileAccessPermissions.UserExecute |
            FileAccessPermissions.GroupRead |
            FileAccessPermissions.GroupExecute |
            FileAccessPermissions.OtherRead |
            FileAccessPermissions.OtherExecute;

        [DllImport("libc", SetLastError = true, EntryPoint = "chmod", CharSet = CharSet.None)]
        internal static extern int Chmod(string path, FileAccessPermissions mode);
    }
}

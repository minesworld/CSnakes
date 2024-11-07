using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython.CAPI;

internal unsafe partial class Proxy
{
    internal static int GetNativeThreadId()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetCurrentThreadId();
        else
            return -1;
    }

    [LibraryImport("kernel32.dll")]
    private static partial int GetCurrentThreadId();
}

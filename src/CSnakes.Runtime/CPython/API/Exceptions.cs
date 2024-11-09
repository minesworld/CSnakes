namespace CSnakes.Runtime.CPython;

public unsafe partial class API
{
    /// <summary>
    /// Has an error occured. Caller must hold the GIL.
    /// </summary>
    /// <returns></returns>
    public static bool IsPyErrOccurred() => PyErr_Occurred() != IntPtr.Zero;
}

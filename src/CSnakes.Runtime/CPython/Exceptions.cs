﻿namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    /// <summary>
    /// Has an error occured. Caller must hold the GIL.
    /// </summary>
    /// <returns></returns>
    internal static bool IsPyErrOccurred() => PyErr_Occurred() != IntPtr.Zero;
}

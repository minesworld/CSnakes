namespace CSnakes.Runtime.CPython.CAPI;
using pyoPtr = nint;

public unsafe partial class Proxy
{
    /// <summary>
    /// Get the None object.
    /// </summary>
    /// <returns>A new reference to None. In newer versions of Python, None is immortal anyway.</returns>
    public static pyoPtr GetNone()
    {
        if (_PyNone == IntPtr.Zero)
        {
            throw new InvalidOperationException("Python is not initialized. You cannot call this method outside of a Python Environment context.");
        }
        Py_IncRef(_PyNone);
        return _PyNone;
    }
}

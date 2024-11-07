using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython.CAPI;
using pyoPtr = nint;

internal unsafe partial class Proxy
{
    /// <summary>
    /// Calls PyLong_AsLongLong and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    internal static long LongLongFromPyLong(pyoPtr p)
    {
        long result = PyLong_AsLongLong(p);
        if (result == -1 && PyErr_Occurred() != IntPtr.Zero)
        {
            throw CreateExceptionWrappingPyErr  ("Error converting Python object to int, check that the object was a Python int or that the value wasn't too large. See InnerException for details.");
        }
        return result;
    }

    /// <summary>
    /// Calls PyLong_AsLong and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    internal static long LongFromPyLong(pyoPtr p)
    {
        long result = PyLong_AsLong(p);
        if (result == -1 && PyErr_Occurred() != IntPtr.Zero)
        {
            throw CreateExceptionWrappingPyErr("Error converting Python object to int, check that the object was a Python int or that the value wasn't too large. See InnerException for details.");
        }
        return result;
    }
}

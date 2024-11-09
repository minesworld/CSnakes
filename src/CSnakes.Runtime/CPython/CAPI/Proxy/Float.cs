namespace CSnakes.Runtime.CPython.CAPI;
using pyoPtr = nint;

public unsafe partial class Proxy
{
    public static double DoubleFromPyFloat(pyoPtr p)
    {
        double result = PyFloat_AsDouble(p);
        if (result == -1 && PyErr_Occurred() != IntPtr.Zero)
        {
            throw CreateExceptionWrappingPyErr("Error converting Python object to double, check that the object was a Python float. See InnerException for details.");
        }
        return result;
    }

}

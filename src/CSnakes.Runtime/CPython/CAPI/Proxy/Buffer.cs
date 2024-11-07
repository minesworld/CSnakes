namespace CSnakes.Runtime.CPython.CAPI;
using pyoPtr = nint;

internal unsafe partial class Proxy
{
    public static PyBuffer? GetBuffer(pyoPtr p)
    {
        PyBuffer view = default;
        if (PyObject_GetBuffer(p, &view, (int)(PyBUF.Format | PyBUF.CContiguous)) != 0)
        {
            return null;
        }
        return view;
    }

    public static void ReleaseBuffer(PyBuffer view) => PyBuffer_Release(&view);
}

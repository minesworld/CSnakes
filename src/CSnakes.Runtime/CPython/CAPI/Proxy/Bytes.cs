using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython.CAPI;
using pyoPtr = nint;

public unsafe partial class Proxy
{
    public static pyoPtr ByteSpanToPyBytes(Span<byte> bytes)
    {
        fixed (byte* b = bytes)
        {
            return PyBytes_FromStringAndSize(b, bytes.Length);
        }
    }

    public static byte[] ByteArrayFromPyBytes(pyoPtr pyBytesPtr)
    {
        byte* ptr = PyBytes_AsString(pyBytesPtr);
        nint size = PyBytes_Size(pyBytesPtr);
        byte[] byteArray = new byte[size];
        Marshal.Copy((IntPtr)ptr, byteArray, 0, (int)size);
        return byteArray;
    }
}

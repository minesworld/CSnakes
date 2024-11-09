namespace CSnakes.Runtime.CPython.CAPI;
using pyoPtr = nint;

public unsafe partial class Proxy
{
    public static pyoPtr AsPyUnicodeObject(string s)
    {
        fixed (char* c = s)
        {
            return PyUnicode_DecodeUTF16(c, s.Length * sizeof(char), 0, 0);
        }
    }

    /// <summary>
    /// Calls PyUnicode_AsUTF8 and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string StringFromPyUnicodeToUTF8(pyoPtr s)
    {
        var bytes = PyUnicode_AsUTF8(s);
        if (bytes is null) throw CreateExceptionWrappingPyErr();
        var result = NonFreeUtf8StringMarshaller.ConvertToManaged(bytes);
        if (result is null) throw new Exception("xxx");
        return result;
    }
}

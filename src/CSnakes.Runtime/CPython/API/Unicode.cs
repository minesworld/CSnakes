namespace CSnakes.Runtime.CPython;

public unsafe partial class API
{
    /// <summary>
    /// Calls PyUnicode_AsUTF8 and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string StringFromPyUnicodeToUTF8(ReferenceObject s) => StringFromPyUnicodeToUTF8(s.DangerousGetHandle());
}

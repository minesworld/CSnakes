namespace CSnakes.Runtime.CPython;

public unsafe partial class API
{
    /// <summary>
    /// Calls PyLong_AsLongLong and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static long LongLongFromPyLong(ReferenceObject p) => LongLongFromPyLong(p.DangerousGetHandle());

    /// <summary>
    /// Calls PyLong_AsLong and throws a Python Exception if an error occurs.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static long LongFromPyLong(ReferenceObject p) => LongFromPyLong(p.DangerousGetHandle());
}

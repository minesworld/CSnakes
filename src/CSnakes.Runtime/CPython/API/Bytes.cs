namespace CSnakes.Runtime.CPython;

public unsafe partial class API
{
    public static byte[] ByteArrayFromPyBytes(ReferenceObject ob) => ByteArrayFromPyBytes(ob.DangerousGetHandle());
}

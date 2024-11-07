namespace CSnakes.Runtime.CPython;
internal unsafe partial class API
{
    internal static byte[] ByteArrayFromPyBytes(ReferenceObject ob) => ByteArrayFromPyBytes(ob.DangerousGetHandle());
}

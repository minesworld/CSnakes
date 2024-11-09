namespace CSnakes.Runtime.CPython;

public unsafe partial class API
{
    public static double DoubleFromPyFloat(ReferenceObject p) => DoubleFromPyFloat(p.DangerousGetHandle());

}

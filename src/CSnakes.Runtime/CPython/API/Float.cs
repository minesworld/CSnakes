namespace CSnakes.Runtime.CPython;

internal unsafe partial class API
{
    internal static double DoubleFromPyFloat(ReferenceObject p) => DoubleFromPyFloat(p.DangerousGetHandle());

}

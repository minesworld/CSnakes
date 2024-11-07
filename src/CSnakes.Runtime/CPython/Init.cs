namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI : Unmanaged.CAPI
{
    public CAPI(string pythonLibraryPath, Version version, Func<string?, Exception>? createExceptionWrappingPyErrFunc=null) : base(pythonLibraryPath, version, createExceptionWrappingPyErrFunc) {}

}

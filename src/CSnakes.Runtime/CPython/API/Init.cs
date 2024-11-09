namespace CSnakes.Runtime.CPython;

public unsafe partial class API : CAPI.Proxy
{
    public API(string pythonLibraryPath, Version version, Func<string?, Exception>? createExceptionWrappingPyErrFunc=null) : base(pythonLibraryPath, version, createExceptionWrappingPyErrFunc) {}

}

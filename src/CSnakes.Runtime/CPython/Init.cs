namespace CSnakes.Runtime.CPython;

internal unsafe partial class API : CAPI.Delegate
{
    public API(string pythonLibraryPath, Version version, Func<string?, Exception>? createExceptionWrappingPyErrFunc=null) : base(pythonLibraryPath, version, createExceptionWrappingPyErrFunc) {}

}

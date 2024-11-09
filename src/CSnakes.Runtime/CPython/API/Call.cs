namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

public unsafe partial class API
{
    public static nint Call(ReferenceObject callable, Span<pyoPtr> args) => Call(callable.DangerousGetHandle(), args);

    public static IntPtr Call(ReferenceObject callable, Span<pyoPtr> args, Span<string> kwnames, Span<pyoPtr> kwvalues)
        => Call(callable.DangerousGetHandle(), args, kwnames, kwvalues);

}

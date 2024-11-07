namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

internal unsafe partial class API
{
    internal static nint Call(ReferenceObject callable, Span<pyoPtr> args) => Call(callable.DangerousGetHandle(), args);

    internal static IntPtr Call(ReferenceObject callable, Span<pyoPtr> args, Span<string> kwnames, Span<pyoPtr> kwvalues)
        => Call(callable.DangerousGetHandle(), args, kwnames, kwvalues);

}

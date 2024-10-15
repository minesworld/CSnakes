﻿namespace CSnakes.Runtime.CPython;

internal unsafe partial class CAPI
{
    internal static double DoubleFromPyFloat(MPyOPtr p) => DoubleFromPyFloat(p.DangerousGetHandle());

    internal static bool IsPyFloat(MPyOPtr p) => IsInstance(p, _PyFloatType);
}

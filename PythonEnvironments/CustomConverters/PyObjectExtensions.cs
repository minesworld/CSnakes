﻿using Python.Runtime;

namespace PythonEnvironments.CustomConverters;
internal static class PyObjectExtensions
{
    public static IEnumerable<T> AsEnumerable<T>(this PyObject obj) =>
        new PyIterable(obj.GetIterator()).Select(item => item.As<T>()).ToArray();
}

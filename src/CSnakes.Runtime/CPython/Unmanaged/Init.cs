﻿using CSnakes.Runtime.Python;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI : IDisposable
{
    protected const string PythonLibraryName = "csnakes_python";
    public string PythonPath { get; internal set; } = string.Empty;

    private static string? pythonLibraryPath = null;
    private static readonly object initLock = new();
    private static Version PythonVersion = new("0.0.0");
    private bool disposedValue = false;

    public CAPI(string pythonLibraryPath, Version version)
    {
        PythonVersion = version;
        CAPI.pythonLibraryPath = pythonLibraryPath;
        try
        {
            NativeLibrary.SetDllImportResolver(typeof(CAPI).Assembly, DllImportResolver);
        }
        catch (InvalidOperationException)
        {
            // TODO: Work out how to call setdllimport resolver only once to avoid raising exceptions. 
            // Already set. 
        }
    }

    [Flags]
    private enum RTLD : int
    {
        LOCAL = 0,
        LAZY = 1,
        NOW = 2,
        NOLOAD = 4,
        DEEPBIND = 8,
        GLOBAL = 0x00100
    }

    [LibraryImport("libdl.so.2", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint dlopen(string path, int flags);

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == PythonLibraryName)
        {
            // Override default dlopen flags on linux to allow global symbol resolution (required in extension modules)
            // See https://github.com/tonybaloney/CSnakes/issues/112#issuecomment-2290643468
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
                return dlopen(pythonLibraryPath!, (int)(RTLD.LAZY | RTLD.GLOBAL));
            }

            return NativeLibrary.Load(pythonLibraryPath!, assembly, null);
        }
        return NativeLibrary.Load(libraryName, assembly, searchPath);
    }

    internal void Initialize()
    {
        if (IsInitialized)
            return;

        InitializeEmbeddedPython();
    }

    private void InitializeEmbeddedPython()
    {
        lock (initLock)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Py_SetPath_UCS2_UTF16(PythonPath);
            else
                Py_SetPath_UCS4_UTF32(PythonPath);
            Debug.WriteLine($"Initializing Python on thread {GetNativeThreadId()}");
            Py_Initialize();

            // Setup type statics
            using (GIL.Acquire())
            {
                if (PyErr_Occurred() != IntPtr.Zero)
                    throw new InvalidOperationException("Python initialization failed.");

                if (!IsInitialized)
                    throw new InvalidOperationException("Python initialization failed.");

                _PyUnicodeType = PyObject_Type(AsPyUnicodeObject(string.Empty));
                _PyTrue = PyBool_FromLong(1);
                _PyFalse = PyBool_FromLong(0);
                _PyBoolType = PyObject_Type(_PyTrue);
                _PyEmptyTuple = PyTuple_New(0);
                _PyTupleType = PyObject_Type(_PyEmptyTuple);
                _PyFloatType = PyObject_Type(PyFloat_FromDouble(0.0));
                _PyLongType = PyObject_Type(PyLong_FromLongLong(0));
                _PyListType = PyObject_Type(PyList_New(0));
                _PyDictType = PyObject_Type(PyDict_New());
                _PyBytesType = PyObject_Type(ByteSpanToPyBytes(new byte[] { }));
                _ItemsStr = AsPyUnicodeObject("items");
                _PyNone = GetBuiltin("None");
            }
            PyEval_SaveThread();
        }
    }

    internal static bool IsInitialized => Py_IsInitialized() == 1;

    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NonFreeUtf8StringMarshaller))]
    internal static partial string? Py_GetVersion();

    [LibraryImport(PythonLibraryName, EntryPoint = "Py_SetPath")]
    internal static partial void Py_SetPath_UCS2_UTF16([MarshalAs(UnmanagedType.LPWStr)] string path);

    [LibraryImport(PythonLibraryName, EntryPoint = "Py_SetPath", StringMarshallingCustomType = typeof(Utf32StringMarshaller), StringMarshalling = StringMarshalling.Custom)]
    internal static partial void Py_SetPath_UCS4_UTF32(string path);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                lock (initLock)
                {
                    if (!IsInitialized)
                        return;

                    Debug.WriteLine("Calling Py_Finalize()");

                    // Acquire the GIL only to dispose it immediately because `PyGILState_Release`
                    // is not available after `Py_Finalize` is called. This is done primarily to
                    // trigger the disposal of handles that have been queued before the Python
                    // runtime is finalized.

                    GIL.Acquire().Dispose();

                    PyGILState_Ensure();
                    Py_Finalize();
                }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
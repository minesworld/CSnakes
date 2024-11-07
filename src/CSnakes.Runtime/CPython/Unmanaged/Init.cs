using CSnakes.Runtime.Python;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI : IDisposable
{
    private static Func<string?, Exception> CreateExceptionWrappingPyErrFunc = CreatePlainExceptionWrappingPyErr;
    public static Exception CreateExceptionWrappingPyErr(string? message = null) => CreateExceptionWrappingPyErrFunc(message);

    protected const string PythonLibraryName = "csnakes_python";
    public string PythonPath { get; internal set; } = string.Empty;

    private static string? pythonLibraryPath = null;
    private static readonly object initLock = new();
    private static Version PythonVersion = new("0.0.0");
    private bool disposedValue = false;

    public CAPI(string pythonLibraryPath, Version version, Func<string?, Exception>? createExceptionWrappingPyErrFunc = null)
    {
        PythonVersion = version;
        CAPI.pythonLibraryPath = pythonLibraryPath;
        CreateExceptionWrappingPyErrFunc = (createExceptionWrappingPyErrFunc != null) ? createExceptionWrappingPyErrFunc : CreatePlainExceptionWrappingPyErr;

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
            Debug.WriteLine($"Initializing Python on thread {GetNativeThreadId()}");
            Py_Initialize();

            // Setup type statics
            using (GIL.Acquire())
            {
                if (PyErr_Occurred() != IntPtr.Zero)
                    throw new InvalidOperationException("Python initialization failed.");

                if (!IsInitialized)
                    throw new InvalidOperationException("Python initialization failed.");

                InitializeTypesAndStaticObjects();

                // update sys.path by adding missing paths of PythonPath
                AppendMissingPathsToSysPath(PythonPath.Split(Path.PathSeparator));
            }
            PyEval_SaveThread();
        }
    }

    internal static bool IsInitialized => Py_IsInitialized() == 1;

    [LibraryImport(PythonLibraryName, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(NonFreeUtf8StringMarshaller))]
    internal static partial string? Py_GetVersion();


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

    #region Exceptions
    static public bool FetchAndClearPyErr(out pyoPtr excType, out pyoPtr excValue, out pyoPtr excTraceback)
    {
        excType = excValue = excTraceback = 0;

        if (PyErr_Occurred() == 0)
        {
            return false;
        }
        PyErr_Fetch(out excType, out excValue, out excTraceback); // Deprecated since version 3.12 ... might be replaced in the future

        if (excType == 0)
        {
            // is it sure to that excValue and excTraceback are null?
            return false;
        }

        PyErr_Clear();

        return true;
    }

    static public Exception CreatePlainExceptionWrappingPyErr(string? message)
    {
        if (FetchAndClearPyErr(out nint excType, out nint excValue, out nint excTraceback) == false)
        {
            return new InvalidDataException("An error occurred in Python, but no exception was set.");
        }

        var exception = new Exception(message);

        Py_DecRef(excType);
        if (excValue != 0) Py_DecRef(excValue); ;
        if (excValue != 0) Py_DecRef(excTraceback);

        return exception;
    }
    #endregion
}

using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using CSnakes.EnvironmentBuilder;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

public class CPythonEnvironment
{
    private readonly CPython.API api;
    private bool disposedValue;

    protected static CPythonEnvironment? pythonEnvironment;
    protected readonly static object locker = new();

    public string Version
    {
        get
        {
            return API.Py_GetVersion() ?? "No version available";
        }
    }



    public static CPythonEnvironment GetCPythonEnvironmentFromExecutedPlan(EnvironmentPlan plan)
    {
        if (pythonEnvironment is null)
        {
            lock (locker)
            {
                pythonEnvironment ??= new CPythonEnvironment(plan);
            }
        }

        return pythonEnvironment;
    }

    protected CPythonEnvironment(EnvironmentPlan plan)
    {
        api = SetupStandardLibrary(plan);
        api.Initialize();
    }


    private API SetupStandardLibrary(EnvironmentPlan plan, ILogger? logger=null)
    {
        string pythonDll = plan.PythonLocation.LibPythonPath;
        string pythonPath = plan.GetPythonPath();

        logger?.LogDebug("Python DLL: {PythonDLL}", pythonDll);
        logger?.LogDebug("Python path: {PythonPath}", pythonPath);

        var api = new API(pythonDll, plan.PythonLocation.Version, PythonObject.CreatePythonExceptionWrappingPyErr)
        {
            PythonPath = pythonPath
        };
        return api;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                api.Dispose();
                if (pythonEnvironment is not null)
                {
                    lock (locker)
                    {
                        if (pythonEnvironment is not null)
                        {
                            pythonEnvironment = null;
                        }
                    }
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

    public bool IsDisposed()
    {
        return disposedValue;
    }
}

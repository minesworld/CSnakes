﻿using CSnakes.Runtime.CPython;
using CSnakes.EnvironmentBuilder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CSnakes.Runtime;

public class CPythonEnvironment
{
    private readonly CAPI api;
    private bool disposedValue;

    protected static CPythonEnvironment? pythonEnvironment;
    protected readonly static object locker = new();


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


    private CAPI SetupStandardLibrary(EnvironmentPlan plan, ILogger? logger=null)
    {
        string pythonDll = plan.PythonLocation.LibPythonPath;
        string pythonPath = plan.GetPythonPath();

        logger?.LogDebug("Python DLL: {PythonDLL}", pythonDll);
        logger?.LogDebug("Python path: {PythonPath}", pythonPath);

        var api = new CAPI(pythonDll, plan.PythonLocation.Version)
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
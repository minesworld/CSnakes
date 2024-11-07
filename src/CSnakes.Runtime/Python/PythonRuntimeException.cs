﻿using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;
public class PythonRuntimeException : Exception
{
    private readonly PythonObject? pythonTracebackObject;
    private string[]? formattedStackTrace = null;

    public PythonRuntimeException(PythonObject? exception, PythonObject? traceback): base(exception?.ToString(), GetPythonInnerException(exception))
    {
        pythonTracebackObject = traceback;
        if (traceback is null)
        {
            return;
        }

        Data["locals"] = traceback.GetAttr("tb_frame").GetAttr("f_locals").As<IReadOnlyDictionary<string, PythonObject>>();
        Data["globals"] = traceback.GetAttr("tb_frame").GetAttr("f_globals").As<IReadOnlyDictionary<string, PythonObject>>();
    }

    private static PythonRuntimeException? GetPythonInnerException(PythonObject? exception)
    {
        if (exception is null)
        {
            return null;
        }
        if (exception.HasAttr("__cause__") && !exception.GetAttr("__cause__").IsNone())
        {
            return new PythonRuntimeException(exception.GetAttr("__cause__"), null);
        }
        return null;
    }

    public string[] PythonStackTrace
    {
        get
        {
            if (pythonTracebackObject is null)
            {
                return [];
            }
            // This is lazy because it's expensive to format the stack trace.
            formattedStackTrace ??= FormatPythonStackTrace(pythonTracebackObject);
            return formattedStackTrace;
        }
    }

    private static string[] FormatPythonStackTrace(PythonObject pythonStackTrace)
    {
        if (!API.IsInitialized)
        {
            return [];
        }

        using (GIL.Acquire())
        {
            using var tracebackModule = Import.ImportModule("traceback");
            using var formatTbFunction = PythonObject.Create(tracebackModule.GetAttr("format_tb"));
            using var formattedStackTrace = formatTbFunction.Call(pythonStackTrace);

            return [.. formattedStackTrace.As<IReadOnlyList<string>>()];
        }
    }

    public override string? StackTrace => string.Join(Environment.NewLine, PythonStackTrace);
}

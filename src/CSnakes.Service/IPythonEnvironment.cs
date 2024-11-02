using CSnakes.Runtime.CPython;
using Microsoft.Extensions.Logging;

namespace CSnakes.Service;

public interface IPythonEnvironment : IDisposable
{
    public string Version
    {
        get
        {
            return CAPI.Py_GetVersion() ?? "No version available";
        }
    }


    public bool IsDisposed();

    public ILogger<IPythonEnvironment> Logger { get; }
}

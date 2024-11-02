using Microsoft.Extensions.Logging;

namespace CSnakes.Service;

public interface IPythonEnvironment : IDisposable
{
    public string Version { get; }

    public bool IsDisposed();

    public ILogger<IPythonEnvironment> Logger { get; }
}

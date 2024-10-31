using Microsoft.Extensions.Logging;

namespace CSnakes.EnvironmentBuilder;
public class EnvironmentPlan : ILogger<IEnvironmentPlanner>
{
    public string Folder { get; internal set; } = string.Empty;
    public Version Version { get; internal set; } = new Version();
    public string LibPythonPath { get; internal set; } = string.Empty;
    public string PythonPath { get; internal set; } = string.Empty;
    public string PythonBinaryPath { get; internal set; } = string.Empty;

    public bool Debug { get; internal set; } = false;
    public bool FreeThreaded { get; internal set; } = false;


    protected List<string> Paths = new List<string>();

    public bool AddPath(string path)
    {
        if (Paths.Contains(path)) return false;

        Paths.Add(path);
        return true;
    }

    public List<string> GetPaths() => new List<string>(Paths);


    #region ILogger
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => System.Diagnostics.Debug.WriteLine(formatter(state, exception));

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }
    #endregion
}

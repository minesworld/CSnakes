using Microsoft.Extensions.Logging;

namespace CSnakes.EnvironmentBuilder;
public class EnvironmentPlan(ILogger logger, CancellationToken cancellationToken)
{
    public CancellationToken CancellationToken { get => cancellationToken; }
    public ILogger Logger {  get => logger; }

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
}

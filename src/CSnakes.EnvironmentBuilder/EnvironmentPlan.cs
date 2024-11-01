using Microsoft.Extensions.Logging;

namespace CSnakes.EnvironmentBuilder;
public class EnvironmentPlan(ILogger logger, CancellationToken cancellationToken)
{
    public CancellationToken CancellationToken { get => cancellationToken; }
    public ILogger Logger {  get => logger; }


    protected bool canExecute = true;
    public bool CanExecute { get => canExecute && CancellationToken.IsCancellationRequested != true; }
    virtual public void ExecutionFailed(Exception? ex=null) { canExecute = false; }



    protected PythonLocation? pythonLocation = null;
    virtual public PythonLocation PythonLocation
    {
        get {
            if (pythonLocation == null) throw new InvalidOperationException("no PythonLocation set");
            return pythonLocation;
        }
        set {
            if (pythonLocation != null) throw new InvalidOperationException("PythonLocation already set");
            pythonLocation = value;
        }
    }

    public bool HasPythonLocation { get => pythonLocation != null; }


    protected string? homePath = Environment.CurrentDirectory;
    virtual public string HomePath
    {
        get => string.IsNullOrEmpty(homePath) ? PythonLocation.HomePath : homePath;
        set => homePath = value;
    }



    protected List<string> searchPaths = new List<string>();

    public bool AddSearchPath(string path)
    {
        if (searchPaths.Contains(path)) return false;

        searchPaths.Add(path);
        return true;
    }

    public List<string> GetSearchPaths()
    {
        var result = new List<string>(searchPaths);

        if (string.IsNullOrEmpty(homePath) == false && result.Contains(homePath) == false)
            result.Add(homePath);

        if (result.Contains(WorkingDir))
            result.Remove(WorkingDir);
        result.Add(WorkingDir);

        return result;
    }

    public string GetPythonPath() => string.Join(Path.PathSeparator, GetSearchPaths());


    protected string? workingDir = null;
    virtual public string WorkingDir
    {
        get
        {
            if (workingDir == null) throw new InvalidOperationException("no WorkingDir set");
            return workingDir;
        }
        set
        {
            if (workingDir != null) throw new InvalidOperationException("WorkingDir already set");
            workingDir = value;
        }
    }
}

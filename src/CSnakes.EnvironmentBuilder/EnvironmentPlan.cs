using Microsoft.Extensions.Logging;

namespace CSnakes.EnvironmentBuilder;
public class EnvironmentPlan(ILogger logger, CancellationToken cancellationToken)
{
    public CancellationToken CancellationToken { get => cancellationToken; }
    public ILogger? Logger {  get => logger; }


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


    protected string workingDirectory = Path.GetFullPath(Environment.CurrentDirectory);
    virtual public string WorkingDirectory { get => workingDirectory; set => workingDirectory = Path.GetFullPath(value); }



    protected List<string> searchPaths = new List<string>();
    public bool AddSearchPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (searchPaths.Contains(path)) return false;

        searchPaths.Add(path);
        return true;
    }

    public List<string> GetSearchPaths()
    {
        var result = new List<string>(searchPaths);

        if (string.IsNullOrEmpty(WorkingDirectory) == false && result.Contains(WorkingDirectory) == false)
            result.Add(WorkingDirectory);

        return result;
    }

    public string GetPythonPath() => string.Join(Path.PathSeparator, GetSearchPaths());

}

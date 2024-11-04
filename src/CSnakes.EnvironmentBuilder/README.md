# CSnakes.EnvironmentBuilder

## create an environment for CSnakes.Runtime or pythonnet

```using CSnakes.EnvironmentBuilder;
using CSnakes.EnvironmentBuilder.Locators;
using CSnakes.EnvironmentBuilder.EnvironmentManagement;
using CSnakes.EnvironmentBuilder.PackageManagement;

var homeDir = @"test"; // should be an absolute path
var venvDir = @"test\.venv";

ILogger? logger = null;

var plan = new EnvironmentPlan(null, CancellationToken.None);

await NuGetLocator.WithVersion("3.12.7").WorkOnPlanAsync(plan);
await VenvEnvironmentManagement.AtFolder(venvDir).WorkOnPlanAsync(plan);
await WorkDirSetter.WithPath(workingDir).WorkOnPlanAsync(plan);
await PipInstaller.WithRequirements(Path.Join(workingDir, "requirements.txt"), venvDir).WorkOnPlanAsync(plan);

if (plan.CanExecute == false)
    throw new Exception("failed to setup CPython environment");```


## using create environment from CSnakes.Runtime

```var python = CPythonEnvironment.GetCPythonEnvironmentFromExecutedPlan(plan);```


## using created environment from pythonnet

```using Python.Runtime;

Runtime.PythonDLL = plan.PythonLocation.LibPythonPath;

PythonEngine.Initialize();

PythonEngine.PythonHome = plan.WorkingDirectory;
PythonEngine.PythonPath = plan.GetPythonPath();```

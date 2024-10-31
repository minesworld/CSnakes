var homeDir = @"C:\Users\Michael\Develop\windows\cncpt\NetCPython\test";
var venvDir = @"C:\Users\Michael\Develop\windows\cncpt\NetCPython\test\.venv";
var mainScript = Path.Join(homeDir, "test.py");

using StreamReader reader = new(mainScript, System.Text.Encoding.UTF8);
string code = reader.ReadToEnd();

Debug.WriteLine($"started {args}");

var pythonLocator = CPythonUtilities.Locators.NuGetLocator.WithVersion("3.12.7");
var location = pythonLocator.LocatePython();

var venv = VenvEnvironmentManagement.AtFolder(venvDir);
venv.CreateEnvironment(location);

var pip = PipInstaller.WithRequirementsFile(Path.Join(homeDir, "requirements.txt"));
await pip.InstallPackages(homeDir, venv);

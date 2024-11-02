using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.Locators;

public class SourceLocator(string folder, Version version, bool debug = true, bool freeThreaded = false) : PythonLocator, IEnvironmentPlanner
{
    public void UpdatePlan(EnvironmentPlan plan)
    {
        if (IsSupported == false) return;

        var buildFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(folder, "PCbuild", "amd64") : folder;

        if (string.IsNullOrEmpty(buildFolder) || !Directory.Exists(buildFolder))
        {
            throw new DirectoryNotFoundException($"Python {Version} not found in {buildFolder}.");
        }

        LocatePythonInternal(plan, buildFolder, freeThreaded);
    }

    protected override Version Version { get; } = version;

    public override Task WorkOnPlanAsync(EnvironmentPlan plan) => IEnvironmentPlanner.WorkOnPlanAsync(this, plan, IsSupported == false | plan.HasPythonLocation);

    protected bool Debug => debug;

    protected override string GetLibPythonPath(string folder, bool freeThreaded = false)
    {
        string suffix = freeThreaded ? "t" : "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(folder, $"python{Version.Major}{Version.Minor}{suffix}{(debug ? "_d" : string.Empty)}.dll");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(folder, "lib", $"libpython{Version.Major}.{Version.Minor}{suffix}{(debug ? "d" : string.Empty)}.dylib");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(folder, "lib", $"libpython{Version.Major}.{Version.Minor}{suffix}{(debug ? "d" : string.Empty)}.so");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported platform.");
        }
    }

    protected override void AddPythonPaths(EnvironmentPlan plan, string folder, bool freeThreaded = false)
    {
        plan.AddSearchPath(Path.Combine(folder, "..", "..", "Lib"));
        plan.AddSearchPath(folder);
    }

    protected override string GetPythonExecutablePath(string folder, bool freeThreaded = false)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Debug)
        {
            return Path.Combine(folder, "python_d.exe");
        }
        return base.GetPythonExecutablePath(folder);
    }

    internal override bool IsSupported { get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows); }
}

using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.Locators;
internal class MacOSInstallerLocator(Version version, bool freeThreaded = false) : PythonLocator, IEnvironmentPlanner
{
    protected override Version Version { get; } = version;

    public override Task WorkOnPlanAsync(EnvironmentPlan plan) => ((IEnvironmentPlanner)this).WorkOnPlanAsync(plan);

    public void UpdatePlan(EnvironmentPlan plan)
    {
        string framework = freeThreaded ? "PythonT.framework" : "Python.framework";
        string mappedVersion = $"{Version.Major}.{Version.Minor}";
        string pythonPath = Path.Combine($"/Library/Frameworks/{framework}/Versions", mappedVersion);
        LocatePythonInternal(plan, pythonPath, freeThreaded);
    }

    internal override bool IsSupported { get => RuntimeInformation.IsOSPlatform(OSPlatform.OSX); }
}

using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.Locators;
internal class MacOSInstallerLocator(Version version, bool freeThreaded = false) : PythonLocator
{
    protected override Version Version { get; } = version;

    public IEnvironmentPlanner UpdatePlan(EnvironmentPlan plan)
    {
        string framework = freeThreaded ? "PythonT.framework" : "Python.framework";
        string mappedVersion = $"{Version.Major}.{Version.Minor}";
        string pythonPath = Path.Combine($"/Library/Frameworks/{framework}/Versions", mappedVersion);
        return LocatePythonInternal(plan, pythonPath, freeThreaded);
    }

    internal override bool IsSupported { get => RuntimeInformation.IsOSPlatform(OSPlatform.OSX); }
}

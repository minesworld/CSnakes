using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.Locators;
internal class WindowsStoreLocator(Version version) : PythonLocator
{
    protected override Version Version { get; } = version;

    public IEnvironmentPlanner UpdatePlan(EnvironmentPlan plan)
    {
        var windowsStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", $"Python{Version.Major}.{Version.Minor}");
        return LocatePythonInternal(plan, windowsStorePath);
    }

    internal override bool IsSupported { get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows); }
}

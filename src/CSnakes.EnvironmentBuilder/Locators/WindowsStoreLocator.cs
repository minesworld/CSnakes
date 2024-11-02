using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.Locators;
public class WindowsStoreLocator(Version version) : PythonLocator, IEnvironmentPlanner
{
    protected override Version Version { get; } = version;

    public override Task WorkOnPlanAsync(EnvironmentPlan plan) => IEnvironmentPlanner.WorkOnPlanAsync(this, plan, IsSupported == false | plan.HasPythonLocation);

    public void UpdatePlan(EnvironmentPlan plan)
    {
        var windowsStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python", $"Python{Version.Major}.{Version.Minor}");
        LocatePythonInternal(plan, windowsStorePath);
    }

    internal override bool IsSupported { get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows); }
}

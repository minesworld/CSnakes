using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.Locators;
internal class WindowsInstallerLocator(Version version) : PythonLocator, IEnvironmentPlanner
{
    protected override Version Version { get; } = version;

    readonly string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    public void UpdatePlan(EnvironmentPlan plan)
    {
        var officialInstallerPath = Path.Combine(programFilesPath, "Python", $"{Version.Major}.{Version.Minor}");

        LocatePythonInternal(plan, officialInstallerPath);
    }

    internal override bool IsSupported { get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows); }
}

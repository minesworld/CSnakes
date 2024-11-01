using System.Runtime.InteropServices;
using CSnakes.EnvironmentBuilder;

namespace CSnakes.EnvironmentBuilder.Locators;

public class NuGetLocator(string nugetVersion, Version version) : PythonLocator, IEnvironmentPlanner
{
    static public IEnvironmentPlanner WithVersion(string version)
    {
        // See https://github.com/tonybaloney/CSnakes/issues/154#issuecomment-2352116849
        version = version.Replace("alpha.", "a").Replace("beta.", "b").Replace("rc.", "rc");

        // If a supplied version only consists of 2 tokens - e.g., 1.10 or 2.14 - then append an extra token
        if (version.Count(c => c == '.') < 2)
        {
            version = $"{version}.0";
        }

        return new NuGetLocator(version, ServiceCollectionExtensions.ParsePythonVersion(version));
    }


    protected override Version Version { get; } = version;

    public void UpdatePlan(EnvironmentPlan plan)
    {
        if (IsSupported == false) return;

        var globalNugetPackagesPath = (NuGetPackages: Environment.GetEnvironmentVariable("NUGET_PACKAGES"),
                                       UserProfile  : Environment.GetEnvironmentVariable("USERPROFILE")) switch
            {
                (NuGetPackages : { Length: > 0 } path, _) => path,
                (_, UserProfile: { Length: > 0 } path) => Path.Combine(path, ".nuget", "packages"),
                _ => throw new DirectoryNotFoundException("Neither NUGET_PACKAGES or USERPROFILE environments variable were found, which are needed to locate the NuGet package cache.")
            };
        // TODO : Load optional path from nuget settings. https://learn.microsoft.com/en-us/nuget/consume-packages/managing-the-global-packages-and-cache-folders
        string nugetPath = Path.Combine(globalNugetPackagesPath, "python", nugetVersion, "tools");
        LocatePythonInternal(plan, nugetPath);
    }

    internal override bool IsSupported { get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows); }
}

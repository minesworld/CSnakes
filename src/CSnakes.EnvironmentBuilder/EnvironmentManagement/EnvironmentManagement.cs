using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.EnvironmentManagement;
public abstract class EnvironmentManagement
{
    virtual public string EnvironmentPath { get; protected set; } = string.Empty;

    abstract public Task CreateEnvironmentAsync(EnvironmentPlan plan);

    virtual public Task PrepareWithPlanAsync(EnvironmentPlan plan) => Task.FromResult(plan.CancellationToken.IsCancellationRequested != true);
    virtual public Task ExecutePlanAsync(EnvironmentPlan plan) => CreateEnvironmentAsync(plan);



    public bool AddExtraPackagePaths(EnvironmentPlan plan, string basePath)
    {
        EnvironmentPath = Path.GetFullPath(basePath);

        var sitePackagesPath = String.Empty;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            sitePackagesPath = Path.Combine(EnvironmentPath, "Lib", "site-packages");
        else
        {
            var pl = plan.PythonLocation;
            string suffix = pl.FreeThreaded ? "t" : "";
            sitePackagesPath = Path.Combine(EnvironmentPath, "lib", $"python{pl.Version.Major}.{pl.Version.Minor}{suffix}", "site-packages");
        }
        plan.Logger.LogDebug("Adding environment site-packages to extra paths: {VenvLibPath}", sitePackagesPath);
        return plan.AddSearchPath(sitePackagesPath);
    }

}

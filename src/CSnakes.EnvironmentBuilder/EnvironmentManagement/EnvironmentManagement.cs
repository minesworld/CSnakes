using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.EnvironmentManagement;
public abstract class EnvironmentManagement
{
    abstract public Task CreateEnvironmentAsync(EnvironmentPlan plan);

    virtual public Task PrepareWithPlanAsync(EnvironmentPlan plan) => Task.FromResult(plan.CancellationToken.IsCancellationRequested != true);
    virtual public Task ExecutePlanAsync(EnvironmentPlan plan) => CreateEnvironmentAsync(plan);



    public bool AddExtraPackagePaths(EnvironmentPlan plan, string basePath)
    {
        var envLibPath = string.Empty;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            envLibPath = Path.Combine(basePath, "Lib", "site-packages");
        else
        {
            var pl = plan.PythonLocation;
            string suffix = pl.FreeThreaded ? "t" : "";
            envLibPath = Path.Combine(basePath, "lib", $"python{pl.Version.Major}.{pl.Version.Minor}{suffix}", "site-packages");
        }
        plan.Logger.LogDebug("Adding environment site-packages to extra paths: {VenvLibPath}", envLibPath);
        plan.HomePath = envLibPath;
        return plan.AddSearchPath(envLibPath);
    }

}

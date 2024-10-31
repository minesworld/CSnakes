using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.EnvironmentManagement;
public interface IEnvironmentManagement : IEnvironmentPlanner
{
    new public async Task<bool> ExecutePlanAsync(EnvironmentPlan plan) => await CreateEnvironmentAsync(plan);


    abstract public Task<bool> CreateEnvironmentAsync(EnvironmentPlan plan);

    public bool AddExtraPackagePaths(EnvironmentPlan plan, string basePath)
    {
        var envLibPath = string.Empty;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            envLibPath = Path.Combine(basePath, "Lib", "site-packages");
        else
        {
            string suffix = plan.FreeThreaded ? "t" : "";
            envLibPath = Path.Combine(basePath, "lib", $"python{plan.Version.Major}.{plan.Version.Minor}{suffix}", "site-packages");
        }
        plan.Logger.LogDebug("Adding environment site-packages to extra paths: {VenvLibPath}", envLibPath);
        return plan.AddPath(envLibPath);
    }

}

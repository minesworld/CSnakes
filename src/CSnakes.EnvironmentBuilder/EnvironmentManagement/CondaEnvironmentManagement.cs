using CSnakes.EnvironmentBuilder.Locators;
using Microsoft.Extensions.Logging;

namespace CSnakes.EnvironmentBuilder.EnvironmentManagement;
#pragma warning disable CS9113 // Parameter is unread. There for future use.
public class CondaEnvironmentManagement(string name, CondaLocator conda, string? environmentSpecPath, bool ensureExists=false) : EnvironmentManagement, IEnvironmentPlanner
#pragma warning restore CS9113 // Parameter is unread.
{
    public void UpdatePlan(EnvironmentPlan plan) => AddExtraPackagePaths(plan, GetPath());

    override public Task CreateEnvironmentAsync(EnvironmentPlan plan)
    {
        var basePath = GetPath();
        if (!Directory.Exists(basePath))
        {
            plan.Logger.LogError("Cannot find conda environment at {basePath}.", basePath);
            // TODO: Automate the creation of the conda environments. 
            //var result = conda.ExecuteCondaShellCommand($"env create -n {name} -f {environmentSpecPath}");
            //if (!result)
            //{
            //    logger.LogError("Failed to create conda environment.");
            //    throw new InvalidOperationException("Could not create conda environment");
            //}
        }
        else
        {
            plan.Logger.LogDebug("Conda environment already exists at {basePath}", basePath);
            // TODO: Check if the environment is up to date
        }

        return Task.CompletedTask;
    }

    protected string GetPath()
    {
        // TODO: Conda environments are not always in the same location. Resolve the path correctly. 
        return Path.GetFullPath(Path.Combine(conda.CondaHome, "envs", name));
    }
}

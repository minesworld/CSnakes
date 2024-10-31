using CSnakes.EnvironmentBuilder.Locators;
using Microsoft.Extensions.Logging;

namespace CSnakes.EnvironmentBuilder.EnvironmentManagement;
#pragma warning disable CS9113 // Parameter is unread. There for future use.
internal class CondaEnvironmentManagement(string name, CondaLocator conda, string? environmentSpecPath) : IEnvironmentManagement
#pragma warning restore CS9113 // Parameter is unread.
{
    public IEnvironmentPlanner UpdatePlan(EnvironmentPlan plan)
    {
        ((IEnvironmentManagement)this).AddExtraPackagePaths(plan, GetPath());
        return this;
    }

    public async Task<bool> CreateEnvironmentAsync(EnvironmentPlan plan, CancellationToken cancellationToken)
    {
        var basePath = GetPath();
        if (!Directory.Exists(basePath))
        {
            plan.LogError("Cannot find conda environment at {basePath}.", basePath);
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
            plan.LogDebug("Conda environment already exists at {basePath}", basePath);
            // TODO: Check if the environment is up to date
        }

        return cancellationToken.IsCancellationRequested != true;
    }

    protected string GetPath()
    {
        // TODO: Conda environments are not always in the same location. Resolve the path correctly. 
        return Path.GetFullPath(Path.Combine(conda.CondaHome, "envs", name));
    }
}

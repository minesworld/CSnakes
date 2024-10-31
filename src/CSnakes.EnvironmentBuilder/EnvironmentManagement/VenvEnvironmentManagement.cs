using Microsoft.Extensions.Logging;

namespace CSnakes.EnvironmentBuilder.EnvironmentManagement;
public class VenvEnvironmentManagement(string path) : IEnvironmentManagement
{
    static public IEnvironmentManagement AtFolder(string path)
    {
        return new VenvEnvironmentManagement(path);
    }

    public IEnvironmentPlanner UpdatePlan(EnvironmentPlan plan)
    {
        ((IEnvironmentManagement)this).AddExtraPackagePaths(plan, path);
        return this;
    }

    
    public async Task<bool> CreateEnvironmentAsync(EnvironmentPlan plan)
    {
        bool success = true;
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                plan.Logger.LogError("Virtual environment location is not set but it was requested to be created.");
                throw new ArgumentNullException(nameof(path), "Virtual environment location is not set.");
            }
            var fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(path))
            {
                plan.Logger.LogInformation("Creating virtual environment at {VirtualEnvPath} using {PythonBinaryPath}", fullPath, plan.PythonBinaryPath);
                var (exitCode1, _, _) = await ProcessUtils.ExecutePythonCommandAsync($"-VV", plan);
                var (exitCode2, _, error) = await ProcessUtils.ExecutePythonCommandAsync($"-m venv {fullPath}", plan);

                if (exitCode1 != 0 || exitCode2 != 0)
                {
                    plan.Logger.LogError("Failed to create virtual environment.");
                    throw new InvalidOperationException($"Could not create virtual environment. {error}");
                }
            }
            else
            {
                plan.Logger.LogDebug("Virtual environment already exists at {VirtualEnvPath}", fullPath);
            }
        }
        catch (Exception ex)
        {
            success = false;
        }

        return success && plan.CancellationToken.IsCancellationRequested != true;
    }
}

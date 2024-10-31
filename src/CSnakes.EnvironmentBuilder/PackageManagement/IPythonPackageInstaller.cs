namespace CSnakes.EnvironmentBuilder.PackageManagement;

/// <summary>
/// Represents an interface for installing Python packages.
/// </summary>
public interface IPythonPackageInstaller : IEnvironmentPlanner
{
    new public async Task<bool> ExecutePlanAsync(EnvironmentPlan plan) => await InstallPackagesAsync(plan);


    /// <summary>
    /// Installs the specified packages.
    /// </summary>
    /// <param name="home">The home directory.</param>
    /// <param name="virtualEnvironmentLocation">The location of the virtual environment (optional).</param>
    /// <returns>A task representing the asynchronous package installation operation.</returns>
    abstract public Task<bool> InstallPackagesAsync(EnvironmentPlan plan);
}

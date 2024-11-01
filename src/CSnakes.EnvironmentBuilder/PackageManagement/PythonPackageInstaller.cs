namespace CSnakes.EnvironmentBuilder.PackageManagement;

/// <summary>
/// Represents an interface for installing Python packages.
/// </summary>
public abstract class PythonPackageInstaller
{
    /// <summary>
    /// Installs the specified packages.
    /// </summary>
    /// <param name="home">The home directory.</param>
    /// <param name="virtualEnvironmentLocation">The location of the virtual environment (optional).</param>
    /// <returns>A task representing the asynchronous package installation operation.</returns>
    abstract public Task InstallPackagesAsync(EnvironmentPlan plan);


    virtual public Task PrepareWithPlanAsync(EnvironmentPlan plan) => Task.FromResult(plan.CancellationToken.IsCancellationRequested != true);
    virtual public Task ExecutePlanAsync(EnvironmentPlan plan) => InstallPackagesAsync(plan);
}

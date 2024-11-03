namespace CSnakes.EnvironmentBuilder.PackageManagement;

/// <summary>
/// Represents an interface for installing Python packages.
/// </summary>
public abstract class PythonPackageInstaller
{
    virtual public string EnvironmentPath { get; set; } = String.Empty;


    /// <summary>
    /// Installs the specified packages.
    /// </summary>
    /// <param name="home">The home directory.</param>
    /// <param name="virtualEnvironmentLocation">The location of the virtual environment (optional).</param>
    /// <returns>A task representing the asynchronous package installation operation.</returns>
    abstract public Task InstallPackagesAsync(EnvironmentPlan plan);


    virtual public Task PrepareWithPlanAsync(EnvironmentPlan plan) => Task.FromResult(plan.CancellationToken.IsCancellationRequested != true);
    virtual public async Task ExecutePlanAsync(EnvironmentPlan plan) { await InstallPackagesAsync(plan); }
}

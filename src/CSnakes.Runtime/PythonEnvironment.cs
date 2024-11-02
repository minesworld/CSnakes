using CSnakes.EnvironmentBuilder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CSnakes.Runtime;

public class PythonEnvironment : CPythonEnvironment, IPythonEnvironment
{
    public ILogger<IPythonEnvironment> Logger { get; private set; }

    public static async Task<IPythonEnvironment> GetPythonEnvironmentAsync(IEnumerable<EnvironmentBuilder.Locators.PythonLocator> locators, IEnumerable<IPythonPackageInstaller> packageInstallers, PythonEnvironmentOptions options, ILogger<IPythonEnvironment> logger, IEnvironmentManagement? environmentManager = null)
    {

        if (pythonEnvironment != null) return (IPythonEnvironment)pythonEnvironment;

        EnvironmentPlan plan = new EnvironmentPlan(logger, CancellationToken.None);

        foreach (var locator in locators)
        {
            await locator.WorkOnPlanAsync(plan);
        }

        if (plan.HasPythonLocation == false)
        {
            logger.LogError("Python installation not found. There were {LocatorCount} locators registered.", locators.Count());
            throw new InvalidOperationException("Python installation not found.");
        }

        string home = options.Home;
        string[] extraPaths = options.ExtraPaths;

        home = Path.GetFullPath(home);
        if (!Directory.Exists(home))
        {
            logger.LogError("Python home directory does not exist: {Home}", home);
            throw new DirectoryNotFoundException("Python home directory does not exist.");
        }

        if (environmentManager is not null)
        {
            await environmentManager.WorkOnPlanAsync(plan);
        }

        logger.LogInformation("Setting up Python environment from {PythonLocation} using home of {Home}", plan.PythonLocation.HomePath, home);

        foreach (var installer in packageInstallers)
        {
            await installer.WorkOnPlanAsync(plan);
        }

        lock (locker)
        {
            pythonEnvironment ??= new PythonEnvironment(logger, plan);
        }

        return (IPythonEnvironment)pythonEnvironment;
    }

    private PythonEnvironment(ILogger<IPythonEnvironment> logger, EnvironmentPlan plan) : base(plan)
    {
        Logger = logger;
    }
}

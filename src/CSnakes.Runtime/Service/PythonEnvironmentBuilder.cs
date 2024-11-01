﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime;

internal partial class PythonEnvironmentBuilder(IServiceCollection services) : IPythonEnvironmentBuilder
{
    private List<string> extraPaths = new();
    private string home = Environment.CurrentDirectory;

    public IServiceCollection Services { get; } = services;

    public IPythonEnvironmentBuilder WithVirtualEnvironment(string path, bool ensureExists = true)
    {
        Services.AddSingleton<IEnvironmentManagement>(
            sp =>
            {
                var logger = sp.GetRequiredService<ILogger<VenvEnvironmentManagement>>();
                return new VenvEnvironmentManagement(path, ensureExists);
            });
        return this;
    }

    public IPythonEnvironmentBuilder WithCondaEnvironment(string name, string? environmentSpecPath = null, bool ensureEnvironment = false)
    {
        if (ensureEnvironment)
            throw new InvalidOperationException("Automated Conda environment creation not yet supported. Conda environments must be created manually.");

        Services.AddSingleton<IEnvironmentManagement>(
            sp => {
                try
                {
                    var condaLocator = sp.GetRequiredService<EnvironmentBuilder.Locators.CondaLocator>();
                    var logger = sp.GetRequiredService<ILogger<CondaEnvironmentManagement>>();
                    var condaEnvManager = new CondaEnvironmentManagement(name, condaLocator, environmentSpecPath, ensureEnvironment);
                    return condaEnvManager;
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException("Conda environments much be used with Conda Locator.");
                }
            });
        return this;
    }

    public IPythonEnvironmentBuilder WithHome(string home)
    {
        this.home = home;
        return this;
    }

    public IPythonEnvironmentBuilder WithExtraPaths(List<string> paths)
    {
        if (paths != null) this.extraPaths = paths;
        return this;
    }

    public PythonEnvironmentOptions GetOptions() =>
        new(home, extraPaths.ToArray());
}

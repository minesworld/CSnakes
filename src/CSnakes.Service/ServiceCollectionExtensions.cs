﻿using CSnakes.EnvironmentBuilder;
using CSnakes.EnvironmentBuilder.Locators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSnakes.Service;
/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to configure Python-related services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Python-related services to the service collection with the specified Python home directory.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="home">The Python home directory.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IPythonEnvironmentBuilder WithPython(this IServiceCollection services)
    {
        var pythonBuilder = new PythonEnvironmentBuilder(services);

        services.AddSingleton<IPythonEnvironmentBuilder>(pythonBuilder);
        services.AddSingleton<IPythonEnvironment>(sp =>
        {
            var envBuilder = sp.GetRequiredService<IPythonEnvironmentBuilder>();
            var locators = sp.GetServices<PythonLocator>();
            var installers = sp.GetServices<IPythonPackageInstaller>();
            var logger = sp.GetRequiredService<ILogger<IPythonEnvironment>>();
            var environmentManager = sp.GetService<IEnvironmentManagement>();

            var options = envBuilder.GetOptions();

            return PythonEnvironment.GetPythonEnvironment(locators, installers, options, logger, environmentManager);
        });

        return pythonBuilder;
    }

    /// <summary>
    /// Adds a Python locator using Python from a NuGet packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the NuGet package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromNuGet(this IPythonEnvironmentBuilder builder, string version)
    {
        // See https://github.com/tonybaloney/CSnakes/issues/154#issuecomment-2352116849
        version = version.Replace("alpha.", "a").Replace("beta.", "b").Replace("rc.", "rc");

        // If a supplied version only consists of 2 tokens - e.g., 1.10 or 2.14 - then append an extra token
        if (version.Count(c => c == '.') < 2)
        {
            version = $"{version}.0";
        }

        builder.Services.AddSingleton<PythonLocator>(new NuGetLocator(version, VersionParser.ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from the Windows Store packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the Windows Store package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromWindowsStore(this IPythonEnvironmentBuilder builder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new WindowsStoreLocator(VersionParser.ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from the Windows Installer packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the Windows Installer package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromWindowsInstaller(this IPythonEnvironmentBuilder builder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new WindowsInstallerLocator(VersionParser.ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from the Official macOS Installer packages to the service collection with the specified version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="version">The version of the Windows Installer package.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromMacOSInstallerLocator(this IPythonEnvironmentBuilder builder, string version, bool freeThreaded = false)
    {
        builder.Services.AddSingleton<PythonLocator>(new MacOSInstallerLocator(VersionParser.ParsePythonVersion(version), freeThreaded));
        return builder;
    }

    public static IPythonEnvironmentBuilder FromSource(this IPythonEnvironmentBuilder builder, string folder, string version, bool debug = true, bool freeThreaded = false)
    {
        builder.Services.AddSingleton<PythonLocator>(new SourceLocator(folder, VersionParser.ParsePythonVersion(version), debug, freeThreaded));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from an environment variable to the service collection with the specified environment variable name and version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="environmentVariable">The name of the environment variable.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromEnvironmentVariable(this IPythonEnvironmentBuilder builder, string environmentVariable, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new EnvironmentVariableLocator(environmentVariable, VersionParser.ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from a specific folder to the service collection with the specified folder path and version.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="folder">The path to the folder.</param>
    /// <param name="version">The version of the Python installation.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromFolder(this IPythonEnvironmentBuilder builder, string folder, string version)
    {
        builder.Services.AddSingleton<PythonLocator>(new FolderLocator(folder, VersionParser.ParsePythonVersion(version)));
        return builder;
    }

    /// <summary>
    /// Adds a Python locator using Python from a conda environment
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the locator to.</param>
    /// <param name="condaBinaryPath">The path to the conda binary.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder FromConda(this IPythonEnvironmentBuilder builder, string condaBinaryPath)
    {
        builder.Services.AddSingleton<CondaLocator>(
            sp =>
            {
                 return new CondaLocator(condaBinaryPath);
            }
        );
        builder.Services.AddSingleton<PythonLocator>(
            sp =>
            {
                var condaLocator = sp.GetRequiredService<CondaLocator>();
                return condaLocator;
            }
        );
        return builder;
    }

    /// <summary>
    /// Adds a pip package installer to the service collection.
    /// </summary>
    /// <param name="builder">The <see cref="IPythonEnvironmentBuilder"/> to add the installer to.</param>
    /// <param name="requirementsPath">The path to the requirements file.</param>
    /// <returns>The modified <see cref="IPythonEnvironmentBuilder"/>.</returns>
    public static IPythonEnvironmentBuilder WithPipInstaller(this IPythonEnvironmentBuilder builder, string requirementsPath = "requirements.txt")
    {
        builder.Services.AddSingleton<IPythonPackageInstaller>(
            sp =>
            {
                var logger = sp.GetRequiredService<ILogger<PipInstaller>>();
                return new PipInstaller(requirementsPath);
            }
        );
        return builder;
    }
}

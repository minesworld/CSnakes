﻿using CSnakes.Runtime.Locators;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime.EnvironmentManagement;
internal class CondaEnvironmentManagement(string name, bool ensureExists, CondaLocator conda, string environmentSpecPath) : IEnvironmentManagement
{
    public void EnsureEnvironment(ILogger logger, PythonLocationMetadata pythonLocation)
    {
        if (!ensureExists)
            return;


        var fullPath = Path.GetFullPath(GetPath());
        if (!Directory.Exists(fullPath))
        {
            logger.LogInformation("Creating conda environment at {fullPath} using {PythonBinaryPath}", fullPath, pythonLocation.PythonBinaryPath);
            // TODO: Shell escape the name
            var (process, _, error) = conda.ExecuteCondaCommand($"env create -n {name} -f {environmentSpecPath}");
            if (process.ExitCode != 0)
            {
                logger.LogError("Failed to create conda environment {Error}.", error);
                process.Dispose();
                throw new InvalidOperationException($"Could not create conda environment : {error}");
            }
            process.Dispose();
        }
        else
        {
            logger.LogDebug("Conda environment already exists at {fullPath}", fullPath);
            // TODO: Check if the environment is up to date
        }
    }

    public string GetPath()
    {
        // TODO: Conda environments are not always in the same location. Resolve the path correctly. 
        return Path.Combine(conda.CondaHome, "envs", name);
    }
}

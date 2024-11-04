using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.PackageManagement;

public class PipInstaller(string requirementsFileName) : PythonPackageInstaller, IEnvironmentPlanner
{
    static readonly string pipBinaryName = $"pip{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "")}";

    static public IEnvironmentPlanner WithRequirements(string requirementsFileName, string? environmentPath)
    {
        var pipInstaller = new PipInstaller(requirementsFileName);
        if (String.IsNullOrEmpty(requirementsFileName) == false)
        {
            pipInstaller.EnvironmentPath = environmentPath;
        }
        return pipInstaller;
    }

    public void UpdatePlan(EnvironmentPlan plan) { }

    override public Task InstallPackagesAsync(EnvironmentPlan plan)
    {
        // TODO:Allow overriding of the requirements file name.
        string requirementsPath = Path.GetFullPath(Path.Combine(plan.WorkingDirectory, requirementsFileName));
        if (File.Exists(requirementsPath))
        {
            plan.Logger?.LogInformation("File {Requirements} was found.", requirementsPath);
            return InstallPackagesWithPipAsync(plan);
        }
        else
        {
            plan.Logger?.LogWarning("File {Requirements} was not found.", requirementsPath);
            return Task.CompletedTask;
        }
    }

    private async Task InstallPackagesWithPipAsync(EnvironmentPlan plan)
    {
        ProcessStartInfo startInfo = new()
        {
            WorkingDirectory = plan.WorkingDirectory,
            FileName = pipBinaryName,
            Arguments = $"install -r {requirementsFileName} --disable-pip-version-check"
        };

        if (String.IsNullOrEmpty(EnvironmentPath) == false)
        {
            string virtualEnvironmentLocation = Path.GetFullPath(EnvironmentPath);
            plan.Logger.LogInformation("Using virtual environment at {VirtualEnvironmentLocation} to install packages with pip.", virtualEnvironmentLocation);
            string venvScriptPath = Path.Combine(virtualEnvironmentLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Scripts" : "bin");
            // TODO: Check that the pip executable exists, and if not, raise an exception with actionable steps.
            startInfo.FileName = Path.Combine(venvScriptPath, pipBinaryName);
            startInfo.EnvironmentVariables["PATH"] = $"{venvScriptPath};{Environment.GetEnvironmentVariable("PATH")}";
        }

        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        using Process process = new() { StartInfo = startInfo };
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                plan.Logger.LogInformation("{Data}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                plan.Logger.LogWarning("{Data}", e.Data);
            }
        };

        process.Start();

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        // await process.WaitForExitAsync(plan.CancellationToken);
        process.WaitForExit(); // await will fail - somehow with ANOTHER await will be continued... WTF ??

        if (process.ExitCode != 0)
        {
            plan.Logger.LogError("Failed to install packages.");
            throw new InvalidOperationException("Failed to install packages.");
        }
    }
}

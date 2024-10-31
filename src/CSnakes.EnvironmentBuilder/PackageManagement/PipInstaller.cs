using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.PackageManagement;

public class PipInstaller(string requirementsFileName, string home, string? environmentPath) : IPythonPackageInstaller
{
    static readonly string pipBinaryName = $"pip{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "")}";

    static public PipInstaller AtHomeFullfillRequirements(string home, string requirementsFileName, string? environmentPath)
    {
        return new PipInstaller(requirementsFileName, home, environmentPath);
    }

    public async Task<bool> InstallPackagesAsync(EnvironmentPlan plan, CancellationToken cancellationToken)
    {
        bool success = true;
        try
        {
            // TODO:Allow overriding of the requirements file name.
            string requirementsPath = Path.GetFullPath(Path.Combine(home, requirementsFileName));
            if (File.Exists(requirementsPath))
            {
                plan.LogInformation("File {Requirements} was found.", requirementsPath);
                await InstallPackagesWithPipAsync(plan, home, environmentPath, cancellationToken);
            }
            else
            {
                plan.LogWarning("File {Requirements} was not found.", requirementsPath);
            }
        }
        catch (Exception ex)
        {
            success = false;
        }

        return success;
    }

    private async Task InstallPackagesWithPipAsync(EnvironmentPlan plan, string home, string? environmentPath, CancellationToken cancellationToken)
    {
        ProcessStartInfo startInfo = new()
        {
            WorkingDirectory = home,
            FileName = pipBinaryName,
            Arguments = $"install -r {requirementsFileName} --disable-pip-version-check"
        };

        if (environmentPath is not null)
        {
            string virtualEnvironmentLocation = Path.GetFullPath(environmentPath);
            plan.LogInformation("Using virtual environment at {VirtualEnvironmentLocation} to install packages with pip.", virtualEnvironmentLocation);
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
                plan.LogInformation("{Data}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                plan.LogWarning("{Data}", e.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            plan.LogError("Failed to install packages.");
            throw new InvalidOperationException("Failed to install packages.");
        }
    }
}

using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CSnakes.EnvironmentBuilder;
internal static class ProcessUtils
{
    internal static Task<(int exitCode, string? result, string? errors)> ExecutePythonCommandAsync(string arguments, EnvironmentPlan plan)
    {
        ProcessStartInfo startInfo = new()
        {
            WorkingDirectory = plan.WorkingDirectory,
            FileName = plan.PythonLocation.PythonBinaryPath,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        return ExecuteCommandAsync(startInfo, plan);
    }

    internal static Task<(int exitCode, string? result, string? errors)> ExecuteCommandAsync(string fileName, string arguments, EnvironmentPlan plan)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        return ExecuteCommandAsync(startInfo, plan);
    }

    internal static async Task<bool> ExecuteShellCommandAsync(string fileName, string arguments, EnvironmentPlan plan)
    {
        plan.Logger.LogInformation("Executing shell command {FileName} {Arguments}", fileName, arguments);
        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
        };
        using Process process = new() { StartInfo = startInfo };
        process.Start();
        await process.WaitForExitAsync(plan.CancellationToken);
        return process.ExitCode == 0;
    }


    private static async Task<(int exitCode, string? result, string? errors)> ExecuteCommandAsync(ProcessStartInfo startInfo, EnvironmentPlan plan) { 
        using Process process = new() { StartInfo = startInfo };
        string? result = null;
        string? errors = null;
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                result += e.Data;
                plan.Logger?.LogInformation("{Data}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errors += e.Data;
                plan.Logger?.LogError("{Data}", e.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync(plan.CancellationToken);
        return (process.ExitCode, result, errors);
    }
}

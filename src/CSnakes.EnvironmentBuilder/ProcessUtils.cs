using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CSnakes.EnvironmentBuilder;
internal static class ProcessUtils
{
    internal static Task<(int exitCode, string? result, string? errors)> ExecutePythonCommandAsync(EnvironmentPlan plan, string arguments, CancellationToken cancellationToken)
    {
        ProcessStartInfo startInfo = new()
        {
            WorkingDirectory = plan.Folder,
            FileName = plan.PythonBinaryPath,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        return ExecuteCommandAsync(plan, startInfo, cancellationToken);
    }

    internal static Task<(int exitCode, string? result, string? errors)> ExecuteCommandAsync(EnvironmentPlan plan, string fileName, string arguments, CancellationToken cancellationToken)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        return ExecuteCommandAsync(plan, startInfo, cancellationToken);
    }

    internal static async Task<bool> ExecuteShellCommandAsync(EnvironmentPlan plan, string fileName, string arguments, CancellationToken cancellationToken)
    {
        plan.LogInformation("Executing shell command {FileName} {Arguments}", fileName, arguments);
        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
        };
        using Process process = new() { StartInfo = startInfo };
        process.Start();
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode == 0;
    }


    private static async Task<(int exitCode, string? result, string? errors)> ExecuteCommandAsync(EnvironmentPlan plan, ProcessStartInfo startInfo, CancellationToken cancellationToken) { 
        using Process process = new() { StartInfo = startInfo };
        string? result = null;
        string? errors = null;
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                result += e.Data;
                plan.LogInformation("{Data}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errors += e.Data;
                plan.LogError("{Data}", e.Data);
            }
        };

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync(cancellationToken);
        return (process.ExitCode, result, errors);
    }
}

using System.Text.Json.Nodes;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Numerics;

namespace CSnakes.EnvironmentBuilder.Locators;

internal class CondaLocator(string condaBinaryPath) : PythonLocator
{
    private string folder;
    private Version version;

    protected override Version Version { get { return version; } }

    public async Task<bool> PrepareWithPlanAsync(EnvironmentPlan plan)
    {
        bool success = true;
        try
        {
            var (exitCode, result, errors) = await ExecuteCondaCommandAsync($"info --json", plan);
            if (exitCode != 0)
            {
                plan.Logger.LogError("Failed to determine Python version from Conda {Error}.", errors);
                throw new InvalidOperationException("Could not determine Python version from Conda.");
            }

            // Parse JSON output to get the version
            var json = JsonNode.Parse(result ?? "")!;
            var versionAttribute = json["python_version"]?.GetValue<string>() ?? string.Empty;

            if (string.IsNullOrEmpty(versionAttribute))
            {
                throw new InvalidOperationException("Could not determine Python version from Conda.");
            }

            var basePrefix = json["root_prefix"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrEmpty(basePrefix))
            {
                throw new InvalidOperationException("Could not determine Conda home.");
            }

            version = ServiceCollectionExtensions.ParsePythonVersion(versionAttribute);
            folder = basePrefix;
        }
        catch (Exception ex)
        {
            success = false;
        }

        return success;
    }

    internal Task<(int process, string? output, string? errors)> ExecuteCondaCommandAsync(string arguments, EnvironmentPlan plan) => ProcessUtils.ExecuteCommandAsync(condaBinaryPath, arguments, plan);

    internal Task<bool> ExecuteCondaShellCommandAsync(string arguments, EnvironmentPlan plan) => ProcessUtils.ExecuteShellCommandAsync(condaBinaryPath, arguments, plan);

    public IEnvironmentPlanner UpdatePlan(EnvironmentPlan plan) => LocatePythonInternal(plan, folder);

    public string CondaHome { get { return folder; } }
}

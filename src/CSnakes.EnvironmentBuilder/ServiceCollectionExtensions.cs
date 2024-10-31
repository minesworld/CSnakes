using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace CSnakes.EnvironmentBuilder;
/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to configure Python-related services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    public static Version ParsePythonVersion(string version)
    {
        // Remove non -numeric characters except .
        Match versionMatch = VersionParseExpr().Match(version);
        if (!versionMatch.Success)
        {
            throw new InvalidOperationException($"Invalid Python version: '{version}'");
        }

        if (!Version.TryParse(versionMatch.Value, out Version? parsed))
        {
            throw new InvalidOperationException($"Failed to parse Python version: '{version}'");
        }

        if (parsed.Build == -1)
        {
            return new Version(parsed.Major, parsed.Minor, 0, 0);
        }

        if (parsed.Revision == -1)
        {
            return new Version(parsed.Major, parsed.Minor, parsed.Build, 0);
        }

        return parsed;
    }


    [GeneratedRegex("^(\\d+(\\.\\d+)*)")]
    private static partial Regex VersionParseExpr();
}

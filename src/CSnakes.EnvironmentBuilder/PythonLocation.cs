using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSnakes.EnvironmentBuilder;

/// <summary>
/// Metadata about the location of a Python installation.
/// </summary>
/// <param name="Version">Version of Python being used from the location.</param>
/// <param name="Debug">True if the Python installation is a debug build.</param>
public sealed record PythonLocation(
    Version Version,
    string LibPythonPath,
    string PythonBinaryPath,
    bool Debug = false,

    bool FreeThreaded = false);


public partial class VersionParser
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

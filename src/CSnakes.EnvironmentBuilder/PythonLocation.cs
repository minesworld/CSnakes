using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSnakes.EnvironmentBuilder;

/// <summary>
/// Metadata about the location of a Python installation.
/// </summary>
/// <param name="Version">Version of Python being used from the location.</param>
/// <param name="Debug">True if the Python installation is a debug build.</param>
public sealed record PythonLocation(
    Version Version,
    string HomePath,
    string LibPythonPath,
    string PythonBinaryPath,
    bool Debug = false,
    bool FreeThreaded = false);

using System.Runtime.InteropServices;

namespace CSnakes.EnvironmentBuilder.Locators;

/// <summary>
/// Abstract class for locating Python installations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PythonLocator"/> class.
/// </remarks>
/// <param name="version">The version of Python.</param>
public abstract class PythonLocator : IEnvironmentPlanner
{
    /// <summary>
    /// Gets the version of Python.
    /// </summary>
    protected abstract Version Version { get; }

    /*
    /// <summary>
    /// Locates the Python installation.
    /// </summary>
    /// <returns>The metadata of the located Python installation.</returns>
    public abstract PythonLocationMetadata LocatePython();
    */

    protected virtual string GetPythonExecutablePath(string folder, bool freeThreaded = false)
    {
        string suffix = freeThreaded ? "t" : "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(folder, $"python{suffix}.exe");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(folder, "bin", $"python3{suffix}");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(folder, "bin", $"python3{suffix}");
        }

        throw new PlatformNotSupportedException($"Unsupported platform: '{RuntimeInformation.OSDescription}'.");
    }

    protected virtual string GetLibPythonPath(string folder, bool freeThreaded = false)
    {
        string suffix = freeThreaded ? "t" : "";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(folder, $"python{Version.Major}{Version.Minor}{suffix}.dll");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(folder, "lib", $"libpython{Version.Major}.{Version.Minor}{suffix}.dylib");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(folder, "lib", $"libpython{Version.Major}.{Version.Minor}{suffix}.so");
        }

        throw new PlatformNotSupportedException($"Unsupported platform: '{RuntimeInformation.OSDescription}'.");
    }

    /// <summary>
    /// Get the standard lib path for Python.
    /// </summary>
    /// <param name="folder">The base folder</param>
    /// <returns></returns>
    protected virtual void AddPythonPaths(EnvironmentPlan plan, string folder, bool freeThreaded = false)
    {
        char sep = Path.PathSeparator;
        string suffix = freeThreaded ? "t" : "";

        // Add standard library to PYTHONPATH
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            plan.AddPath(Path.Combine(folder, "Lib"));
            plan.AddPath(Path.Combine(folder, "DLLs"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            plan.AddPath(Path.Combine(folder, "lib", $"python{Version.Major}.{Version.Minor}{suffix}"));
            plan.AddPath(Path.Combine(folder, "lib", $"python{Version.Major}.{Version.Minor}{suffix}", "lib-dynload"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            plan.AddPath(Path.Combine(folder, "lib", $"python{Version.Major}.{Version.Minor}"));
            plan.AddPath(Path.Combine(folder, "lib", $"python{Version.Major}.{Version.Minor}", "lib-dynload"));
        }
            throw new PlatformNotSupportedException($"Unsupported platform: '{RuntimeInformation.OSDescription}'.");
    }

    /// <summary>
    /// Locates the Python installation internally.
    /// </summary>
    /// <param name="folder">The folder path to search for Python.</param>
    /// <returns>The metadata of the located Python installation.</returns>
    /// <exception cref="DirectoryNotFoundException">Python not found at the specified folder.</exception>
    protected virtual IEnvironmentPlanner LocatePythonInternal(EnvironmentPlan plan, string folder, bool freeThreaded = false)
    {
        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException($"Python not found in '{folder}'.");
        }

        plan.Folder = folder;
        plan.Version = Version;
        plan.LibPythonPath = GetLibPythonPath(folder, freeThreaded);
        plan.PythonBinaryPath = GetPythonExecutablePath(folder, freeThreaded);
        plan.Debug = default;
        plan.FreeThreaded = freeThreaded;

        AddPythonPaths(plan, folder, freeThreaded);
        return this;
    }

    /// <summary>
    /// Specifies whether the Python Locator is supported. Defaults to <see langword="true"/>
    /// </summary>
    /// <remarks>This would be used to do an OS check and ignore a particular <see cref="PythonLocator"/> if the OS does not match the one it supports. See the <see cref="WindowsInstallerLocator"/> as an example.</remarks>
    internal virtual bool IsSupported { get => true; }
}

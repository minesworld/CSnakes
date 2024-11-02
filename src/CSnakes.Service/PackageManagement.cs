using CSnakes.EnvironmentBuilder;
using Microsoft.Extensions.Logging;

namespace CSnakes.Service;
public interface IPythonPackageInstaller : IEnvironmentPlanner { }

internal class PipInstaller : EnvironmentBuilder.PackageManagement.PipInstaller, IPythonPackageInstaller
{
    public PipInstaller(string requirementsFileName, string? environmentPath) : base(requirementsFileName, environmentPath)
    {
    }
}

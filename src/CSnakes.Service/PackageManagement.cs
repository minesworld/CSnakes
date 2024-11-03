using CSnakes.EnvironmentBuilder;

namespace CSnakes.Service;
public interface IPythonPackageInstaller : IEnvironmentPlanner {

    public string EnvironmentPath { get; set; }
}

public class PipInstaller : EnvironmentBuilder.PackageManagement.PipInstaller, IPythonPackageInstaller
{
    public PipInstaller(string requirementsFileName) : base(requirementsFileName) { }
}

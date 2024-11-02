using CSnakes.EnvironmentBuilder;

namespace CSnakes.Service;
public interface IEnvironmentManagement : IEnvironmentPlanner { }

internal class CondaEnvironmentManagement : EnvironmentBuilder.EnvironmentManagement.CondaEnvironmentManagement, IEnvironmentManagement
{
    public CondaEnvironmentManagement(string name, EnvironmentBuilder.Locators.CondaLocator conda, string? environmentSpecPath, bool ensureExists) : base(name, conda, environmentSpecPath, ensureExists)
    {
    }
}

internal class VenvEnvironmentManagement : EnvironmentBuilder.EnvironmentManagement.VenvEnvironmentManagement, IEnvironmentManagement
{
    public VenvEnvironmentManagement(string path, bool ensureExists) : base(path, ensureExists)
    {
    }
}



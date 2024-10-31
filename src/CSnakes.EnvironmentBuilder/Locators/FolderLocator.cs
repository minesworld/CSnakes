namespace CSnakes.EnvironmentBuilder.Locators;
internal class FolderLocator(string folder, Version version) : PythonLocator
{
    protected override Version Version { get; } = version;

    public IEnvironmentPlanner UpdatePlan(EnvironmentPlan plan) => LocatePythonInternal(plan, folder);
}

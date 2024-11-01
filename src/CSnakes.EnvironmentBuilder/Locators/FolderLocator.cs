namespace CSnakes.EnvironmentBuilder.Locators;
internal class FolderLocator(string folder, Version version) : PythonLocator, IEnvironmentPlanner
{
    protected override Version Version { get; } = version;

    public void UpdatePlan(EnvironmentPlan plan) => LocatePythonInternal(plan, folder);
}

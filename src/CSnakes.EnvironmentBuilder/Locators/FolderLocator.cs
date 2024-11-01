namespace CSnakes.EnvironmentBuilder.Locators;
internal class FolderLocator(string folder, Version version) : PythonLocator, IEnvironmentPlanner
{
    protected override Version Version { get; } = version;

    public override Task WorkOnPlanAsync(EnvironmentPlan plan) => ((IEnvironmentPlanner)this).WorkOnPlanAsync(plan);

    public void UpdatePlan(EnvironmentPlan plan) => LocatePythonInternal(plan, folder);
}

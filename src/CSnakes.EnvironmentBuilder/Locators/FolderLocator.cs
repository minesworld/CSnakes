namespace CSnakes.EnvironmentBuilder.Locators;
public class FolderLocator(string folder, Version version) : PythonLocator, IEnvironmentPlanner
{
    protected override Version Version { get; } = version;

    public override Task WorkOnPlanAsync(EnvironmentPlan plan) => IEnvironmentPlanner.WorkOnPlanAsync(this, plan, IsSupported == false | plan.HasPythonLocation);

    public void UpdatePlan(EnvironmentPlan plan) => LocatePythonInternal(plan, folder);
}

namespace CSnakes.EnvironmentBuilder;
public interface IEnvironmentPlanner
{
    static public async Task WorkOnPlanAsync(IEnvironmentPlanner planner, EnvironmentPlan plan)
    {
        try
        {
            if (plan.CanExecute == false) return;
            await planner.PrepareWithPlanAsync(plan);

            if (plan.CanExecute == false) return;
            planner.UpdatePlan(plan);

            if (plan.CanExecute == false) return;
            await planner.ExecutePlanAsync(plan);
        }
        catch (Exception ex)
        {
            plan.ExecutionFailed(ex);
        }
    }

    virtual async Task WorkOnPlanAsync(EnvironmentPlan plan) => WorkOnPlanAsync(this, plan);
 
    abstract Task PrepareWithPlanAsync(EnvironmentPlan plan);
    abstract void UpdatePlan(EnvironmentPlan plan);
    abstract Task ExecutePlanAsync(EnvironmentPlan plan);
}

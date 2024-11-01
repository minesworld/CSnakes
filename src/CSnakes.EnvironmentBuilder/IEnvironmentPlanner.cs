namespace CSnakes.EnvironmentBuilder;
public interface IEnvironmentPlanner
{
    virtual async Task WorkOnPlanAsync(EnvironmentPlan plan)
    {
        try
        {
            if (plan.CanExecute == false) return;
            await PrepareWithPlanAsync(plan);

            if (plan.CanExecute == false) return;
            UpdatePlan(plan);

            if (plan.CanExecute == false) return;
            await ExecutePlanAsync(plan);
        }
        catch (Exception ex)
        {
            plan.ExecutionFailed(ex);
        }
    }

    abstract Task PrepareWithPlanAsync(EnvironmentPlan plan);
    abstract void UpdatePlan(EnvironmentPlan plan);
    abstract Task ExecutePlanAsync(EnvironmentPlan plan);
}

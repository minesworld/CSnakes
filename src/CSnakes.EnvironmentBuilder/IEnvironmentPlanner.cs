namespace CSnakes.EnvironmentBuilder;
public interface IEnvironmentPlanner
{
    async Task<bool> WorkOnPlanAsync(EnvironmentPlan plan)
    {
        if (await PrepareWithPlanAsync(plan) == false) return false;
        return await UpdatePlan(plan).ExecutePlanAsync(plan);
    }

    virtual public Task<bool> PrepareWithPlanAsync(EnvironmentPlan plan) => Task.FromResult(plan.CancellationToken.IsCancellationRequested != true);
    virtual public IEnvironmentPlanner UpdatePlan(EnvironmentPlan plan) => this;
    virtual public Task<bool> ExecutePlanAsync(EnvironmentPlan plan) => Task.FromResult(plan.CancellationToken.IsCancellationRequested != true);
}

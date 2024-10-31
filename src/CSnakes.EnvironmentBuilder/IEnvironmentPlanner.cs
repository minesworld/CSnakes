namespace CSnakes.EnvironmentBuilder;
public interface IEnvironmentPlanner
{
    async Task<bool> WorkOnPlanAsync(EnvironmentPlan plan, CancellationToken cancellationToken)
    {
        if (await PrepareWithPlanAsync(plan, cancellationToken) == false) return false;
        return await UpdatePlan(plan).ExecutePlanAsync(plan, cancellationToken);
    }

    virtual public Task<bool> PrepareWithPlanAsync(EnvironmentPlan plan, CancellationToken cancellationToken) => Task.FromResult(cancellationToken.IsCancellationRequested != true);
    virtual public IEnvironmentPlanner UpdatePlan(EnvironmentPlan plan) => this;
    virtual public Task<bool> ExecutePlanAsync(EnvironmentPlan plan, CancellationToken cancellationToken) => Task.FromResult(cancellationToken.IsCancellationRequested != true);
}

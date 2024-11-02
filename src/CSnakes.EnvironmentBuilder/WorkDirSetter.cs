using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSnakes.EnvironmentBuilder;
public class WorkDirSetter(string path) : IEnvironmentPlanner
{
    static public IEnvironmentPlanner WithPath(string path) => new WorkDirSetter(path);

    public virtual Task PrepareWithPlanAsync(EnvironmentPlan plan) => Task.FromResult(plan.CancellationToken.IsCancellationRequested != true);

    public void UpdatePlan(EnvironmentPlan plan)
    {
        string fullPath = Path.GetFullPath(path);
        if (Path.Exists(fullPath) == false)
            throw new ArgumentException($"workDir does not exists at {path}");

        plan.WorkingDirectory = fullPath;
    }

    public virtual Task ExecutePlanAsync(EnvironmentPlan plan) => Task.FromResult(plan.CancellationToken.IsCancellationRequested != true);
}

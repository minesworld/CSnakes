namespace CSnakes.EnvironmentBuilder.Locators;

internal class EnvironmentVariableLocator(string variable, Version version) : PythonLocator
{
    protected override Version Version { get; } = version;

    public IEnvironmentPlanner UpdatePlan(EnvironmentPlan plan)
    {
        var envValue = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrEmpty(envValue))
        {
            throw new ArgumentNullException($"Environment variable {variable} not found.");
        }

        return LocatePythonInternal(plan, envValue);
    }
}

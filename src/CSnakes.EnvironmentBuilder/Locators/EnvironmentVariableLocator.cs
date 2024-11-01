namespace CSnakes.EnvironmentBuilder.Locators;

internal class EnvironmentVariableLocator(string variable, Version version) : PythonLocator, IEnvironmentPlanner
{
    protected override Version Version { get; } = version;

    public void UpdatePlan(EnvironmentPlan plan)
    {
        var envValue = Environment.GetEnvironmentVariable(variable);
        if (string.IsNullOrEmpty(envValue))
        {
            throw new ArgumentNullException($"Environment variable {variable} not found.");
        }

        LocatePythonInternal(plan, envValue);
    }
}

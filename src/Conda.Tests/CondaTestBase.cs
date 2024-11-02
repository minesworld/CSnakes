using CSnakes.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Conda.Tests;
public class CondaTestBase : IDisposable
{
    protected IPythonEnvironment env { get; private set; }
    private readonly IHost app;

    public CondaTestBase()
    {
        string condaEnv = Environment.GetEnvironmentVariable("CONDA") ?? string.Empty;

        if (string.IsNullOrEmpty(condaEnv))
        {
            if (OperatingSystem.IsWindows())
                condaEnv = Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "";
            condaEnv = Path.Join(condaEnv, "anaconda3");

        }
        var condaBinPath = OperatingSystem.IsWindows() ? Path.Join(condaEnv, "Scripts", "conda.exe") : Path.Join(condaEnv, "bin", "conda");
        var environmentSpecPath = Path.Join(Environment.CurrentDirectory, "python", "environment.yml");
        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Path.Join(Environment.CurrentDirectory, "python"));

                pb.FromConda(condaBinPath)
                  .WithCondaEnvironment("csnakes_test", environmentSpecPath);

                services.AddLogging(builder => builder.AddXUnit());
            })
            .Build();

        AwaitEnv();
    }

    private async void AwaitEnv()
    {
        env = await app.Services.GetRequiredService<Task<IPythonEnvironment>>();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        GC.Collect();
    }

    public IPythonEnvironment Env => env;
}

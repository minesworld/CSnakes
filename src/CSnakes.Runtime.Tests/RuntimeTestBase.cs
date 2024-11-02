﻿using CSnakes.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSnakes.Runtime.Tests;

public class RuntimeTestBase : IDisposable
{
    protected IPythonEnvironment env { get; private set; }
    protected readonly IHost app;

    public RuntimeTestBase()
    {
        string pythonVersionWindows = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12.4";
        string pythonVersionMacOS = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        string pythonVersionLinux = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        bool freeThreaded = Environment.GetEnvironmentVariable("PYTHON_FREETHREADED") == "true";

        app = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var pb = services.WithPython();
                pb.WithHome(Environment.CurrentDirectory);

                pb
                  .FromNuGet(pythonVersionWindows)
                  .FromMacOSInstallerLocator(pythonVersionMacOS, freeThreaded)
                  .FromEnvironmentVariable("Python3_ROOT_DIR", pythonVersionLinux); // This last one is for GitHub Actions

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
}

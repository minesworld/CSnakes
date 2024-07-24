﻿using Python.Generated;
using PythonEnvironments;
using System.Runtime.InteropServices;
using System.Text.Json;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
var userProfile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
var builder = new PythonEnvironment(
    userProfile + Path.Join(".nuget", "packages", "python", "3.12.4", "tools"),
    "3.12.4")
    .WithVirtualEnvironment(Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "ExamplePythonDependency", ".venv"));

using var env = builder.Build(Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "ExamplePythonDependency"));

var quickDemo = env.QuickDemo();
Console.WriteLine(quickDemo.Scream("a", 99));
Console.WriteLine(string.Join(',', quickDemo.ScreamNames(["a", "b", "c"], 3)));

var dict = env.TypeDemos().ReturnDict();

foreach ((string key, long value) in dict)
{
    Console.WriteLine($"{key} : {value}");
}

env.TypeDemos().TakeDict(dict);
// That was boring, how about scikit-learn
var kmeansExample = env.KmeansExample();
List<(long, long)> data = [
    (1, 2), (1, 4), (1, 0),
        (10, 2), (10, 4), (10, 0)
];

var inertiaResult = kmeansExample.CalculateKmeansInertia(data, 4);
var centroids = JsonSerializer.Serialize(inertiaResult.Item1);
var inertia = inertiaResult.Item2;
Console.WriteLine($"KMeans inertia for 4 clusters is {centroids}, inertia is {inertia}");


var phi3demo = env.Phi3Demo();
// var result = phi3demo.Phi3InferenceDemo("What kind of food is Brie?");
// Console.WriteLine(result);
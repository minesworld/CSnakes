using CSnakes;
using CSnakes.Parser;
using CSnakes.Parser.Types;
using CSnakes.Reflection;
using DocoptNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using sourcegenerator;
using System.IO;
using System.Text;

const string usage = @"sourcegenerator

Usage:
  sourcegenerator.exe [--namespace=<n>] [-R] [-B=<basedir>] <pysource>...
  sourcegenerator.exe [--namespace=<n>] -O <pysource>
  sourcegenerator.exe (-h | --help)
  sourcegenerator.exe --version

Options:
  -h --help        Show this screen.
  --version        Show version.
  --namespace=<n>  C# namespace for generated class [default: CSnakes.Service]
  -R               Recursive 
  -B=<basebdir>    Write generated .cs files starting at basedir [default: .]
  -O               Write generated .cs file to stdout
";

var arguments = new Docopt().Apply(usage, args, version: "sourcegenerator 0.1", exit: true)!;
foreach (var (key, value) in arguments)
    Console.WriteLine("{0} = {1}", key, value);


var @namespace = arguments["--namespace"].ToString();
var useStdout = arguments["-O"].IsTrue;

foreach (var s in arguments["<pysource>"].AsList)
{
    var pySourcePath = s.ToString();

    // Read the file
    var text = System.IO.File.ReadAllText(pySourcePath, System.Text.Encoding.UTF8);
    var pyCode = SourceText.From(text);

    string csFileName, csCode;
    GeneratorError[] errors;

    if (Generator.Generate(
            @namespace,
            pySourcePath,
            pyCode,
            out csFileName,
            out csCode,
            out errors) == false)
    {
        continue;
    }

    if (useStdout)
    {
        Console.WriteLine(csCode);
        return;
    }
    else
    {
        var path = Path.Join(Path.GetDirectoryName(pySourcePath), csFileName);
        System.IO.File.WriteAllText(path, csCode, Encoding.UTF8);
        Console.WriteLine($"Generated {path}");
    }
}

/*
            var path = Path.Join(Path.GetDirectoryName(arg), $"{pascalFileName}.py.cs");
            System.IO.File.WriteAllText(path, source, Encoding.UTF8);
            Console.WriteLine($"Generated {path}");
*/

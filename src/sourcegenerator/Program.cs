using CSnakes;
using DocoptNet;
using sourcegenerator;
using System.Text;

const string usage = @"sourcegenerator

Usage:
  sourcegenerator.exe [--namespace=<n>] [--dry] [--csext=<ext>] [-R] [-B=<basedir>] <pysource>...
  sourcegenerator.exe [--namespace=<n>] -O <pysource>
  sourcegenerator.exe (-h | --help)
  sourcegenerator.exe --version

Options:
  -h --help        Show this screen.
  --version        Show version.
  --namespace=<n>  C# namespace for generated class [default: CSnakes.Service]
  --csext=<ext>    Use C# file extension [default: .py.cs] 
  -R               Recurse into directories.
  -B=<basebdir>    Write generated .cs files starting at basedir [default: .]
  --dry            Dry run, does not write generated .cs files.
  -O               Write generated .cs file to stdout.
";

var arguments = new Docopt().Apply(usage, args, version: "sourcegenerator 0.1", exit: true)!;

var @namespace = arguments["--namespace"].ToString();
var csExtension = arguments["--csext"].ToString();
var useStdout = arguments["-O"].IsTrue;
var recursive = arguments["-R"].IsTrue;
var baseDirPath = arguments["-B"].ToString();


List<string> pySourceArgs = new();
foreach (var pySource in arguments["<pysource>"].AsList)
{
    var pySourceArg = pySource.ToString();
    if (String.IsNullOrEmpty(pySourceArg)) continue;
    pySourceArgs.Add(pySourceArg);
}


if (useStdout)
{
    string csFileNameWithoutExtension;
    string csSource;
    GeneratorError[]? errors = null;

    if (Generator.CSharpStringFromPyFile(@namespace, pySourceArgs[0], Encoding.UTF8, out csFileNameWithoutExtension, out csSource, out errors) == false)
    {
        // error
        return 1;
    }

    Console.WriteLine(csSource);
    return 0; // we only process one file if writing to stdout
}

foreach (var pySourceArg in pySourceArgs)
{
    if (recursive && Directory.Exists(pySourceArg))
    {
        throw new NotImplementedException();
    }
    else if (File.Exists(pySourceArg))
    {
        throw new NotImplementedException();
    }
    else
    {
        // error
        return 1;
    }
}

return 0;

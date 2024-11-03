using CSnakes.Parser.Types;
using CSnakes.Parser;
using CSnakes.Reflection;
using CSnakes;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace sourcegenerator;
internal class Generator
{
    static public bool WriteCSharpFileFromPyFile(
        string @namespace,
        string pyPath,
        Encoding pyFileEncoding,
        string csDirectoryPath,
        string csExtension,
        out string csPath,
        Encoding csFileEncoding,
        out GeneratorError[]? errors,
        bool dryRun = false)
    {
        csPath = String.Empty;

        string csFileNameWithoutExtension;
        string csSource;

        if (CSharpStringFromPyFile(@namespace, pyPath, pyFileEncoding, out csFileNameWithoutExtension, out csSource, out errors) == false) return false;

        // calculate and write file to csPath
        csPath = Path.Join(csDirectoryPath, csFileNameWithoutExtension + csExtension);
        if (dryRun == false) File.WriteAllText(csPath, csSource, csFileEncoding);

        return true;
    }


    static public bool CSharpStringFromPyFile(
        string @namespace,
        string pyPath,
        Encoding pyFileEncoding,
        out string csFileNameWithoutExtension,
        out string csSource,
        out GeneratorError[]? errors)
    {
        // read file from pyPath
        var pySource = File.ReadAllText(pyPath, pyFileEncoding);

        return Generate(@namespace, pyPath, pySource, out csFileNameWithoutExtension, out csSource, out errors);
    }


    static public bool Generate(
        string @namespace,
        string pyPath,
        string pySource,
        out string csFileNameWithoutExtension,
        out string csSource,
        out GeneratorError[]? errors)
    {
        csSource = String.Empty;
        errors = null;

        var fileName = Path.GetFileNameWithoutExtension(pyPath);

        // Convert snakecase to pascal case
        csFileNameWithoutExtension = string.Join("", fileName.Split('_').Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)));

        // Parse the Python file
        var result = PythonParser.TryParseFunctionDefinitions(SourceText.From(pySource), out PythonFunctionDefinition[] functions, out errors);

        if (result)
        {
            IEnumerable<MethodDefinition> methods = ModuleReflection.MethodsFromFunctionDefinitions(functions, fileName);
            csSource = PythonStaticGenerator.FormatClassFromMethods(@namespace, csFileNameWithoutExtension, methods, fileName, functions);

            /*
            sourceContext.AddSource($"{pascalFileName}.py.cs", source);
            sourceContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("PSG002", "PythonStaticGenerator", $"Generated {pascalFileName}.py.cs", "PythonStaticGenerator", DiagnosticSeverity.Info, true), Location.None));
            */

            return true;
        }

        foreach (var error in errors)
        {
            // Update text span
            /*
            Location errorLocation = Location.Create(file.Path, TextSpan.FromBounds(0, 1), new LinePositionSpan(new LinePosition(error.StartLine, error.StartColumn), new LinePosition(error.EndLine, error.EndColumn)));
            sourceContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("PSG004", "PythonStaticGenerator", error.Message, "PythonStaticGenerator", DiagnosticSeverity.Error, true), errorLocation));
            */
        }

        return false;
    }
}

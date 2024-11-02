using CSnakes.Parser.Types;
using CSnakes.Parser;
using CSnakes.Reflection;
using CSnakes;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sourcegenerator;
internal class Generator
{
    static public bool Generate(
        string @namespace,
        string pyPath,
        SourceText pyCode,
        out string csFileName,
        out string? csCode,
        out GeneratorError[]? errors)
    {
        var fileName = Path.GetFileNameWithoutExtension(pyPath);

        // Convert snakecase to pascal case
        var pascalFileName = string.Join("", fileName.Split('_').Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)));
        csFileName = $"{pascalFileName}.py.cs";

        // Parse the Python file
        var result = PythonParser.TryParseFunctionDefinitions(pyCode, out PythonFunctionDefinition[] functions, out errors);

        if (result)
        {
            IEnumerable<MethodDefinition> methods = ModuleReflection.MethodsFromFunctionDefinitions(functions, fileName);
            csCode = PythonStaticGenerator.FormatClassFromMethods(@namespace, pascalFileName, methods, fileName, functions);

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

        csCode = null;
        return false;
    }
}

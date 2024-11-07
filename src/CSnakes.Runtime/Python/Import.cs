using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python;

public static class Import
{
    public static PythonObject ImportModule(string module)
    {
        return PythonObject.Create(API.Import(module));
    }

    public static PythonObject CreateAndImportModuleFromSource(string name, string source, string filename = "<string>")
    {
        PythonObject result;
        using (GIL.Acquire())
        {
            result = PythonObject.Steal(API.AddCodeAsModule(source, name, filename));
        }

        return result;
    }
}

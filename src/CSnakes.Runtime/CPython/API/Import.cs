namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

internal unsafe partial class API
{
    /// <summary>
    /// Import a module and return a reference to it.
    /// </summary>
    /// <param name="name">The module name</param>
    /// <returns>A new reference to module `name`</returns>
    internal static ReferenceObject Import(string name)
    {
        pyoPtr pyName = AsPyUnicodeObject(name);
        pyoPtr module = PyImport_Import(pyName);
        Py_DecRef(pyName);
        return ReferenceObject.Steal(module);
    }
}

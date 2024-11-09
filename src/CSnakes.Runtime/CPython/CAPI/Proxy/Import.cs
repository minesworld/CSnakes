namespace CSnakes.Runtime.CPython.CAPI;
using pyoPtr = nint;

public unsafe partial class Proxy
{
    public static pyoPtr GetBuiltin(string name)
    {
        pyoPtr pyName = AsPyUnicodeObject("builtins");
        pyoPtr pyAttrName = AsPyUnicodeObject(name);
        pyoPtr module = PyImport_Import(pyName);
        pyoPtr attr = PyObject_GetAttr(module, pyAttrName);
        if (attr == IntPtr.Zero)
        {
            throw CreateExceptionWrappingPyErr();
        }
        Py_DecRef(pyName);
        Py_DecRef(pyAttrName);
        return attr;
    }

    public static pyoPtr AddCodeAsModule(string code, string name, string filename = "")
    {
        var fileName = string.IsNullOrEmpty(filename) ? "<string>" : filename;

        // Compile the code

        var codePtr = NonFreeUtf8StringMarshaller.ConvertToUnmanaged(code);
        var filenamePtr = NonFreeUtf8StringMarshaller.ConvertToUnmanaged(fileName);

        var pyoPtrCompiledCode = Py_CompileString(codePtr, filenamePtr, (int)Token.File);

        NonFreeUtf8StringMarshaller.Free(codePtr);
        NonFreeUtf8StringMarshaller.Free(filenamePtr);

        if (pyoPtrCompiledCode == IntPtr.Zero)
        {
            throw CreateExceptionWrappingPyErr();
        }

        // Execute the compiled code within the module's dictionary
        var pyoPtrModuleName = AsPyUnicodeObject(name);
        var pyoPtrFilename = AsPyUnicodeObject(fileName);

        var pyoPtrModule = PyImport_ExecCodeModuleObject(pyoPtrModuleName, pyoPtrCompiledCode, pyoPtrFilename, pyoPtrFilename);

        Py_DecRef(pyoPtrModuleName);
        Py_DecRef(pyoPtrFilename);

        if (pyoPtrModule == IntPtr.Zero)
        {
            throw CreateExceptionWrappingPyErr();
        }

        return pyoPtrModule;
    }
}

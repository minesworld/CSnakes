using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython.Unmanaged;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    public void AppendMissingPathsToSysPath(string[] paths)
    {
        var pyoPtrSysName = AsPyUnicodeObject("sys");
        var pyoPtrSysModule = PyImport_Import(pyoPtrSysName);
        Py_DecRef(pyoPtrSysModule);
        if (pyoPtrSysModule == IntPtr.Zero) CreateExceptionWrappingPyErr();

        var pyoPtrPathAttr = AsPyUnicodeObject("path");
        var pyoPtrPathList = PyObject_GetAttr(pyoPtrSysModule, pyoPtrPathAttr);
        Py_DecRef(pyoPtrPathAttr);
        if (pyoPtrSysModule == IntPtr.Zero) CreateExceptionWrappingPyErr();

        foreach (var path in paths)
        {
            var pyoPtrPythonPath = AsPyUnicodeObject(path);

            bool found = false;
            for (int i = 0; i < PyList_Size(pyoPtrPathList); i++)
            {
                var pyoPtrSysPath = PyList_GetItem(pyoPtrPathList, i);
                if (pyoPtrSysPath == IntPtr.Zero)
                    continue;
                found = RichComparePyObjects(pyoPtrPythonPath, pyoPtrSysPath, RichComparisonType.Equal);
                if (found)
                    break;
            }

            if (found == false)
                PyList_Append(pyoPtrPathList, pyoPtrPythonPath);

            Py_DecRef(pyoPtrPythonPath);
        }

        Py_DecRef(pyoPtrPathList);
        Py_DecRef(pyoPtrSysModule);
    }
}

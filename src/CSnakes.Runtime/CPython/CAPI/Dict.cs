using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython.CAPI;
using pyoPtr = nint;

internal unsafe partial class Delegate
{
    internal static nint PackDict(Span<string> kwnames, Span<pyoPtr> kwvalues)
    {
        var dict = PyDict_New();
        for (int i = 0; i < kwnames.Length; i ++)
        {
            var keyObj = AsPyUnicodeObject(kwnames[i]);
            int result = PyDict_SetItem(dict, keyObj, kwvalues[i]);
            if (result == -1)
            {
                throw CreateExceptionWrappingPyErr();
            }
            Py_DecRef(keyObj);
        }
        return dict;
    }
}

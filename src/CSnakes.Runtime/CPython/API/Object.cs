using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

public unsafe partial class API
{
    public static bool IsInstance(ReferenceObject ob, pyoPtr type)
    {
        int result = PyObject_IsInstance(ob, type);
        if (result == -1)
        {
            throw CreateExceptionWrappingPyErr();
        }
        return result == 1;
    }

    public static pyoPtr GetAttr(ReferenceObject ob, string name) => GetAttr(ob.DangerousGetHandle(), name);
    public static pyoPtr GetAttr(ReferenceObject ob, pyoPtr name) => GetAttr(ob.DangerousGetHandle(), name);
    public static pyoPtr GetAttr(ReferenceObject ob, ReferenceObject name) => GetAttr(ob.DangerousGetHandle(), name.DangerousGetHandle());

    public static bool HasAttr(ReferenceObject ob, string name) => HasAttr(ob.DangerousGetHandle(), name);
    public static bool HasAttr(ReferenceObject ob, pyoPtr name) => HasAttr(ob.DangerousGetHandle(), name);
    public static bool HasAttr(ReferenceObject ob, ReferenceObject name) => HasAttr(ob.DangerousGetHandle(), name.DangerousGetHandle());

    public static bool RichComparePyObjects(ReferenceObject ob1, ReferenceObject ob2, RichComparisonType comparisonType)
    {
        int result = PyObject_RichCompareBool(ob1, ob2, comparisonType);
        if (result == -1)
        {
            throw CreateExceptionWrappingPyErr();
        }
        return result == 1;
    }
}

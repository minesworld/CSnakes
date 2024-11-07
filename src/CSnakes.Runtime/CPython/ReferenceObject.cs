using CSnakes.Runtime.CPython.CAPI;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

[DebuggerDisplay("MPyOPtr: {DangerousGetHandle()}")]
public class ReferenceObject : SafeHandle
{
    #region creation
    public static ReferenceObject Steal(pyoPtr pyObjectPtr) => new ReferenceObject(pyObjectPtr);
    public static ReferenceObject ByIncRef(pyoPtr pyObjectPtr) => new ReferenceObject(pyObjectPtr, false, true);
    #endregion

    #region SafeHandle logic
    protected ReferenceObject(pyoPtr pyObjectPtr, bool ownsHandle=true, bool incRef = false) : base(pyObjectPtr, ownsHandle)
    {
        if (pyObjectPtr == IntPtr.Zero)
        {
            // TODO: throw if there is an CPythonException otherwise a normal C# Exception... 
            throw API.CreateExceptionWrappingPyErr();
        }


        if (incRef) API.Py_IncRef(pyObjectPtr);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (IsInvalid == false)
        {
            API.Py_DecRef(handle);
            handle = IntPtr.Zero;
        }

        return true;
    }
    #endregion

    #region CSharp comparision support

    /// <summary>
    /// Are the objects the same instance, equivalent to the `is` operator in Python
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool Is(ReferenceObject other)
    {
        return DangerousGetHandle() == other.DangerousGetHandle();
    }

    public override bool Equals(object? obj)
    {
        if (obj is ReferenceObject pyObj1)
        {
            if (Is(pyObj1))
                return true;
            return Compare(this, pyObj1, API.RichComparisonType.Equal);
        }
        return base.Equals(obj);
    }

    public bool NotEquals(object? obj)
    {
        if (obj is ReferenceObject pyObj1)
        {
            if (Is(pyObj1))
                return false;
            return Compare(this, pyObj1, API.RichComparisonType.NotEqual);
        }
        return !base.Equals(obj);
    }

    public static bool operator ==(ReferenceObject? left, ReferenceObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Equals(right),
        };
    }

    public static bool operator !=(ReferenceObject? left, ReferenceObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => true,
            (null, _) => true,
            (_, _) => left.NotEquals(right),
        };
    }

    public static bool operator <=(ReferenceObject? left, ReferenceObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Is(right) || Compare(left, right, API.RichComparisonType.LessThanEqual),
        };
    }

    public static bool operator >=(ReferenceObject? left, ReferenceObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Is(right) || Compare(left, right, API.RichComparisonType.GreaterThanEqual),
        };
    }

    public static bool operator <(ReferenceObject? left, ReferenceObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => false,
            (null, _) => false,
            (_, _) => Compare(left, right, API.RichComparisonType.LessThan),
        };
    }

    public static bool operator >(ReferenceObject? left, ReferenceObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => false,
            (null, _) => false,
            (_, _) => Compare(left, right, API.RichComparisonType.GreaterThan),
        };
    }

    private static bool Compare(ReferenceObject left, ReferenceObject right, CAPI.Proxy.RichComparisonType type)
    {
        var result = API.PyObject_RichCompareBool(left.DangerousGetHandle(), right.DangerousGetHandle(), type);
        if (result == -1)
        {
            throw API.CreateExceptionWrappingPyErr();
        }
        return result == 1;
    }
    #endregion
}

using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python.Interns;
using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace CSnakes.Runtime.Python;
using pyoPtr = nint;

[DebuggerDisplay("PythonObject: repr={GetRepr()}, type={GetPythonType().ToString()}")]
public class PythonObject : ReferenceObject, ICloneable
{
    protected PythonObject(pyoPtr pyObject, bool ownsHandle = true, bool incRef = false) : base(pyObject, ownsHandle, incRef) { }

    public static PythonObject Create(pyoPtr ptr)
    {
        if (None.DangerousGetHandle() == ptr)
            return None;
        return new PythonObject(ptr, false, true);
    }

    new public static PythonObject Steal(pyoPtr ptr)
    {
        if (None.DangerousGetHandle() == ptr)
            return None;
        return new PythonObject(ptr, false, false);
    }


    public static PythonObject Create(ReferenceObject ob)
    {
        if (None.DangerousGetHandle() == ob.DangerousGetHandle())
            return None;

        var pyoPtr = ob.DangerousGetHandle();
        var result = new PythonObject(pyoPtr, false, true);
        return result;
    }


    protected override bool ReleaseHandle()
    {
        if (IsInvalid)
            return true;
        if (!API.IsInitialized)
        {
            // The Python environment has been disposed, and therefore Python has freed it's memory pools.
            // Don't run decref since the Python process isn't running and this pointer will point somewhere else.
            handle = IntPtr.Zero;
            // TODO: Consider moving this to a logger.
            Debug.WriteLine($"Python object at 0x{handle:X} was released, but Python is no longer running.");
            return true;
        }
        if (GIL.IsAcquired)
        {
            using (GIL.Acquire())
            {
                API.Py_DecRef(handle);
            }
        }
        else
        {
            // Probably in the GC finalizer thread, instead of causing GIL contention, put this on a queue to be processed later.
            GIL.QueueForDisposal(handle);
        }
        handle = IntPtr.Zero;
        return true;
    }

    /// <summary>
    /// Throws a Python exception as a CLR exception.
    /// </summary>
    /// <exception cref="InvalidDataException"></exception>
    /// <exception cref="PythonInvocationException"></exception>
    ///
    internal static Exception CreatePythonExceptionWrappingPyErr(string? message = null)
    {
        using (GIL.Acquire())
        {
            if (API.FetchAndClearPyErr(out nint excType, out nint excValue, out nint excTraceback) == false)
            {
                return new InvalidDataException("An error occurred in Python, but no exception was set.");
            }

            using var pyExceptionType = Create(excType);
            PythonObject? pyExceptionTraceback = excTraceback == IntPtr.Zero ? null : new PythonObject(excTraceback);
            PythonObject? pyException = excValue == IntPtr.Zero ? null : Create(excValue);

            // TODO: Consider adding __qualname__ as well for module exceptions that aren't builtins
            var pyExceptionTypeStr = pyExceptionType.GetAttr("__name__").ToString();

            return string.IsNullOrEmpty(message)
              ? new PythonInvocationException(pyExceptionTypeStr, pyException, pyExceptionTraceback)
              : new PythonInvocationException(pyExceptionTypeStr, pyException, pyExceptionTraceback, message);
        }
    }

    /// <summary>
    /// Throw an invalid operation exception if Python is not running. This is used to prevent
    /// runtime errors when trying to use Python objects after the Python environment has been disposed.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private static void RaiseOnPythonNotInitialized()
    {
        if (!API.IsInitialized)
        {
            throw new InvalidOperationException("Python is not initialized. You cannot call this method outside of a Python Environment context.");
        }
    }

    /// <summary>
    /// Get the type for the object.
    /// </summary>
    /// <returns>A new reference to the type field.</returns>
    public virtual PythonObject GetPythonType()
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return new PythonObject(API.PyObject_Type(this));
        }
    }

    /// <summary>
    /// Get the attribute of the object with name. This is equivalent to obj.name in Python.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>Attribute object (new ref)</returns>
    public virtual PythonObject GetAttr(string name)
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return Create(API.GetAttr(this, name));
        }
    }

    public virtual bool HasAttr(string name)
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return API.HasAttr(this, name);
        }
    }


    internal virtual PythonObject GetIter()
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            return Create(API.PyObject_GetIter(this));
        }
    }

    /// <summary>
    /// Calls iter() on the object and returns an IEnumerable that yields values of type T.
    /// </summary>
    /// <typeparam name="T">The type for each item in the iterator</typeparam>
    /// <returns></returns>
    public IEnumerable<T> AsEnumerable<T>()
    {
        using (GIL.Acquire())
        {
            return new PythonEnumerable<T>(this);
        }
    }

    /// <summary>
    /// Get the results of the repr() function on the object.
    /// </summary>
    /// <returns></returns>
    public virtual string GetRepr()
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            using PythonObject reprStr = new PythonObject(API.PyObject_Repr(this.DangerousGetHandle()));
            return API.StringFromPyUnicodeToUTF8(reprStr);
        }
    }

    /// <summary>
    /// Is the Python object None?
    /// </summary>
    /// <returns>true if None, else false</returns>
    public virtual bool IsNone() => API.IsNone(this);

    /// <summary>
    /// Are the objects the same instance, equivalent to the `is` operator in Python
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool Is(PythonObject other)
    {
        return DangerousGetHandle() == other.DangerousGetHandle();
    }

    public override bool Equals(object? obj)
    {
        if (obj is PythonObject pyObj1)
        {
            if (Is(pyObj1))
                return true;
            return Compare(this, pyObj1, API.RichComparisonType.Equal);
        }
        return base.Equals(obj);
    }

    public bool NotEquals(object? obj)
    {
        if (obj is PythonObject pyObj1)
        {
            if (Is(pyObj1))
                return false;
            return Compare(this, pyObj1, API.RichComparisonType.NotEqual);
        }
        return !base.Equals(obj);
    }

    public static bool operator ==(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Equals(right),
        };
    }

    public static bool operator !=(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => true,
            (null, _) => true,
            (_, _) => left.NotEquals(right),
        };
    }

    public static bool operator <=(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Is(right) || Compare(left, right, API.RichComparisonType.LessThanEqual),
        };
    }

    public static bool operator >=(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => true,
            (_, null) => false,
            (null, _) => false,
            (_, _) => left.Is(right) || Compare(left, right, API.RichComparisonType.GreaterThanEqual),
        };
    }

    public static bool operator <(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => false,
            (null, _) => false,
            (_, _) => Compare(left, right, API.RichComparisonType.LessThan),
        };
    }

    public static bool operator >(PythonObject? left, PythonObject? right)
    {
        return (left, right) switch
        {
            (null, null) => false,
            (_, null) => false,
            (null, _) => false,
            (_, _) => Compare(left, right, API.RichComparisonType.GreaterThan),
        };
    }

    private static bool Compare(PythonObject left, PythonObject right, API.RichComparisonType type)
    {
        using (GIL.Acquire())
        {
            return API.RichComparePyObjects(left, right, type);
        }
    }

    public override int GetHashCode()
    {
        using (GIL.Acquire())
        {
            int hash = API.PyObject_Hash(this);
            if (hash == -1)
            {
                throw CreatePythonExceptionWrappingPyErr();
            }
            return hash;
        }
    }

    public static PythonObject None { get; } = new PythonNoneObject();
    public static PythonObject True { get; } = new PythonTrueObject();
    public static PythonObject False { get; } = new PythonFalseObject();


    /// <summary>
    /// Call the object. Equivalent to (__call__)(args)
    /// All arguments are treated as positional.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>The resulting object, or NULL on error.</returns>
    public PythonObject Call(params PythonObject[] args)
    {
        return CallWithArgs(args);
    }

    public PythonObject CallWithArgs(PythonObject[]? args = null)
    {
        RaiseOnPythonNotInitialized();

        // Don't do any marshalling if there aren't any arguments. 
        if (args is null || args.Length == 0)
        {
            using (GIL.Acquire())
            {
                return Create(API.PyObject_CallNoArgs(this));
            }
        }

        args ??= [];
        var marshallers = new SafeHandleMarshaller<PythonObject>.ManagedToUnmanagedIn[args.Length];
        var argHandles = args.Length < 16
            ? stackalloc IntPtr[args.Length]
            : new IntPtr[args.Length];

        for (int i = 0; i < args.Length; i++)
        {
            ref var m = ref marshallers[i];
            m.FromManaged(args[i]);
            argHandles[i] = m.ToUnmanaged();
        }

        try
        {
            using (GIL.Acquire())
            {
                return Create(API.Call(this, argHandles));
            }
        }
        finally
        {
            foreach (var m in marshallers)
            {
                m.Free();
            }
        }
    }

    public PythonObject CallWithKeywordArguments(PythonObject[]? args = null, string[]? kwnames = null, PythonObject[]? kwvalues = null)
    {
        if (kwnames is null)
            return CallWithArgs(args);
        if (kwvalues is null || kwnames.Length != kwvalues.Length)
            throw new ArgumentException("kwnames and kwvalues must be the same length.");
        RaiseOnPythonNotInitialized();
        args ??= [];

        var argMarshallers = new SafeHandleMarshaller<PythonObject>.ManagedToUnmanagedIn[args.Length];
        var kwargMarshallers = new SafeHandleMarshaller<PythonObject>.ManagedToUnmanagedIn[kwvalues.Length];
        var argHandles = args.Length < 16
            ? stackalloc IntPtr[args.Length]
            : new IntPtr[args.Length];
        var kwargHandles = kwvalues.Length < 16
            ? stackalloc IntPtr[kwvalues.Length]
            : new IntPtr[kwvalues.Length];

        for (int i = 0; i < args.Length; i++)
        {
            ref var m = ref argMarshallers[i];
            m.FromManaged(args[i]);
            argHandles[i] = m.ToUnmanaged();
        }
        for (int i = 0; i < kwvalues.Length; i++)
        {
            ref var m = ref kwargMarshallers[i];
            m.FromManaged(kwvalues[i]);
            kwargHandles[i] = m.ToUnmanaged();
        }

        try
        {
            using (GIL.Acquire())
            {
                return Create(API.Call(this, argHandles, kwnames, kwargHandles));
            }
        }
        finally
        {
            foreach (var m in argMarshallers)
            {
                m.Free();
            }
            foreach (var m in kwargMarshallers)
            {
                m.Free();
            }
        }
    }

    public PythonObject CallWithKeywordArguments(PythonObject[]? args = null, string[]? kwnames = null, PythonObject[]? kwvalues = null, IReadOnlyDictionary<string, PythonObject>? kwargs = null)
    {
        // No keyword parameters supplied
        if (kwnames is null && kwargs is null)
            return CallWithArgs(args);
        // Keyword args are empty and kwargs is empty. 
        if (kwnames is not null && kwnames.Length == 0 && (kwargs is null || kwargs.Count == 0))
            return CallWithArgs(args);

        MergeKeywordArguments(kwnames ?? [], kwvalues ?? [], kwargs, out string[] combinedKwnames, out PythonObject[] combinedKwvalues);
        return CallWithKeywordArguments(args, combinedKwnames, combinedKwvalues);
    }

    /// <summary>
    /// Get a string representation of the object.
    /// </summary>
    /// <returns>The result of `str()` on the object.</returns>
    public override string ToString()
    {
        RaiseOnPythonNotInitialized();
        using (GIL.Acquire())
        {
            using PythonObject pyObjectStr = new(API.PyObject_Str(this));
            return API.StringFromPyUnicodeToUTF8(pyObjectStr);
        }
    }

    public T As<T>() => (T)As(typeof(T));

    /// <summary>
    /// Unpack a tuple of 2 elements into a KeyValuePair
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    /// <returns></returns>
    public KeyValuePair<TKey, TValue> As<TKey, TValue>()
    {
        using (GIL.Acquire())
        {
            return PythonObjectTypeConverter.ConvertToKeyValuePair<TKey, TValue>(this);
        }
    }

    internal object As(Type type)
    {
        using (GIL.Acquire())
        {
            return type switch
            {
                var t when t == typeof(PythonObject) => Clone(),
                var t when t == typeof(bool) => API.IsPyTrue(this),
                var t when t == typeof(int) => API.LongFromPyLong(this),
                var t when t == typeof(long) => API.LongLongFromPyLong(this),
                var t when t == typeof(double) => API.DoubleFromPyFloat(this),
                var t when t == typeof(float) => (float)API.DoubleFromPyFloat(this),
                var t when t == typeof(string) => API.StringFromPyUnicodeToUTF8(this),
                var t when t == typeof(BigInteger) => PythonObjectTypeConverter.ConvertToBigInteger(this, t),
                var t when t == typeof(byte[]) => API.ByteArrayFromPyBytes(this),
                var t when t.IsAssignableTo(typeof(ITuple)) => PythonObjectTypeConverter.ConvertToTuple(this, t),
                var t when t.IsAssignableTo(typeof(IGeneratorIterator)) => PythonObjectTypeConverter.ConvertToGeneratorIterator(this, t),
                var t => PythonObjectTypeConverter.PyObjectToManagedType(this, t),
            };
        }
    }

    public static PythonObject From<T>(T value)
    {
        using (GIL.Acquire())
        {
            if (value is null)
                return None;

            return value switch
            {
                ReferenceObject mpyoPtr => Create(mpyoPtr),
                ICloneable pyObject => pyObject.Clone(),
                bool b => b ? True : False,
                int i => Create(API.PyLong_FromLong(i)),
                long l => Create(API.PyLong_FromLongLong(l)),
                double d => Create(API.PyFloat_FromDouble(d)),
                float f => Create(API.PyFloat_FromDouble((double)f)),
                string s => Create(API.AsPyUnicodeObject(s)),
                byte[] bytes => Create(API.ByteSpanToPyBytes(bytes.AsSpan())),
                IDictionary dictionary => PythonObjectTypeConverter.ConvertFromDictionary(dictionary),
                ITuple t => PythonObjectTypeConverter.ConvertFromTuple(t),
                ICollection l => PythonObjectTypeConverter.ConvertFromList(l),
                IEnumerable e => PythonObjectTypeConverter.ConvertFromList(e),
                BigInteger b => PythonObjectTypeConverter.ConvertFromBigInteger(b),
                _ => throw new InvalidCastException($"Cannot convert {value} to PythonObject"),
            };
        }
    }

    internal virtual PythonObject Clone()
    {
        API.Py_IncRef(handle);
        return new PythonObject(handle);
    }

    private static void MergeKeywordArguments(string[] kwnames, PythonObject[] kwvalues, IReadOnlyDictionary<string, PythonObject>? kwargs, out string[] combinedKwnames, out PythonObject[] combinedKwvalues)
    {
        if (kwnames.Length != kwvalues.Length)
            throw new ArgumentException("kwnames and kwvalues must be the same length.");
        if (kwargs is null)
        {
            combinedKwnames = kwnames;
            combinedKwvalues = kwvalues;
            return;
        }

        var newKwnames = new List<string>(kwnames);
        var newKwvalues = new List<PythonObject>(kwvalues);

        // The order must be the same as we're not submitting these in a mapping, but a parallel array.
        foreach (var (key, value) in kwargs)
        {
            newKwnames.Add(key);
            newKwvalues.Add(value);
        }

        combinedKwnames = [.. newKwnames];
        combinedKwvalues = [.. newKwvalues];
    }

    PythonObject ICloneable.Clone() => Clone();
}

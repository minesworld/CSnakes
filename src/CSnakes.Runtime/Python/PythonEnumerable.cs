using CSnakes.Runtime.CPython;
using System.Collections;

namespace CSnakes.Runtime.Python;

internal class PythonEnumerable<TValue, TImporter> : IEnumerable<TValue>, IEnumerator<TValue>, IDisposable
    where TImporter : IPythonObjectImporter<TValue>
{
    private readonly PythonObject _pyIterator;
    private TValue current = default!;

    internal PythonEnumerable(PythonObject pyObject)
    {
        using (GIL.Acquire())
        {
            _pyIterator = pyObject.GetIter();
        }
    }

    public TValue Current => current;

    object IEnumerator.Current => current!;

    public void Dispose() => _pyIterator.Dispose();

    public IEnumerator<TValue> GetEnumerator() => this;

    public bool MoveNext()
    {
        using (GIL.Acquire())
        {
            nint result = API.PyIter_Next(_pyIterator);
            if (result == IntPtr.Zero && API.IsPyErrOccurred())
            {
                throw PythonObject.CreatePythonExceptionWrappingPyErr();
            }

            if (result == IntPtr.Zero)
            {
                return false;
            }

            using PythonObject pyObject = PythonObject.Create(result);
            current = TImporter.Import(pyObject);
            return true;
        }
    }

    public void Reset() => throw new NotSupportedException("Python iterators cannot be reset");

    IEnumerator IEnumerable.GetEnumerator() => this;
}

internal class PythonEnumerable<TValue> : PythonEnumerable<TValue, PythonObjectImporter<TValue>>
{
    internal PythonEnumerable(PythonObject pyObject) : base(pyObject) { }
}

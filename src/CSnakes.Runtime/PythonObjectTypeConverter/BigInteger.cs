using CSnakes.Runtime.CPython;
using CSnakes.Runtime.Python;
using System.Numerics;

namespace CSnakes.Runtime;
internal partial class PythonObjectTypeConverter
{
    internal static BigInteger ConvertToBigInteger(PythonObject pyObject, Type destinationType) =>
        // There is no practical API for this in CPython. Use str() instead. 
        BigInteger.Parse(pyObject.ToString());

    internal static PythonObject ConvertFromBigInteger(BigInteger integer)
    {
        using PythonObject pyUnicode = PythonObject.Create(CPython.CAPI.Delegate.AsPyUnicodeObject(integer.ToString()));
        return PythonObject.Create(CPython.CAPI.Delegate.PyLong_FromUnicodeObject(pyUnicode.DangerousGetHandle(), 10));
    }
}

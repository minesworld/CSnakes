using CSnakes.Runtime.CPython;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSnakes.Runtime.Python;
public class IsType
{
    public static bool IsBytes(PythonObject p) => CAPI.IsInstance(p, CAPI.PtrToPyBytesType);
}

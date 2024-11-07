using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.CPython;
internal unsafe partial class API
{
    internal static PyBuffer GetBuffer(ReferenceObject p)
    {
        PyBuffer? view = GetBuffer(p.DangerousGetHandle());
        if (view == null)
        {
            throw CreateExceptionWrappingPyErr();
        }
        else
        {
            return (API.PyBuffer)view;
        }
    }

}

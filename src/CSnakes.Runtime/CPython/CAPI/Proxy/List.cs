namespace CSnakes.Runtime.CPython.CAPI;
using pyoPtr = nint;

public unsafe partial class Proxy
{
    /// <summary>
    /// Get a reference to the item at `pos` in the list
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <returns>New reference to the list item</returns>
    public static pyoPtr GetItemOfPyList(pyoPtr obj, nint pos)
    {
        nint item = PyList_GetItem(obj, pos);
        if (item == IntPtr.Zero)
        {
            throw CreateExceptionWrappingPyErr();
        }
        Py_IncRef(item);
        return item;
    }


    public static int SetItemInPyList(pyoPtr ob, nint pos, pyoPtr o)
    {
        int result = PyList_SetItem(ob, pos, o);
        if (result != -1)
        {
            // Add reference to the new item as it belongs to list now.
            Py_IncRef(o);
        }
        return result;
    }
}

namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

public unsafe partial class API
{
    /// <summary>
    /// Get a reference to the item at `pos` in the list
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <returns>New reference to the list item</returns>
    public static pyoPtr GetItemOfPyList(ReferenceObject obj, nint pos) => GetItemOfPyList(obj.DangerousGetHandle(), pos);

    public static int SetItemInPyList(ReferenceObject obj, nint pos, pyoPtr o) => SetItemInPyList(obj.DangerousGetHandle(), pos, o);

    public static int SetItemInPyList(ReferenceObject obj, nint pos, ReferenceObject o) => SetItemInPyList(obj.DangerousGetHandle(), pos, o.DangerousGetHandle());
}

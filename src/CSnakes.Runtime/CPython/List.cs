﻿namespace CSnakes.Runtime.CPython;
using pyoPtr = nint;

internal unsafe partial class CAPI
{
    /// <summary>
    /// Get a reference to the item at `pos` in the list
    /// </summary>
    /// <param name="obj">The list object</param>
    /// <param name="pos">The position as ssize_t</param>
    /// <returns>New reference to the list item</returns>
    internal static pyoPtr GetItemOfPyList(MPyOPtr obj, nint pos) => GetItemOfPyList(obj.DangerousGetHandle(), pos);

    internal static int SetItemInPyList(MPyOPtr obj, nint pos, pyoPtr o) => SetItemInPyList(obj.DangerousGetHandle(), pos, o);

    internal static int SetItemInPyList(MPyOPtr obj, nint pos, MPyOPtr o) => SetItemInPyList(obj.DangerousGetHandle(), pos, o.DangerousGetHandle());
}

﻿using CSnakes.Runtime.CPython;
using System.Collections;

namespace CSnakes.Runtime.Python;

internal class PythonList<TItem>(PythonObject listObject) : IReadOnlyList<TItem>, IDisposable, ICloneable
{
    // If someone fetches the same index multiple times, we cache the result to avoid multiple round trips to Python
    private readonly Dictionary<long, TItem> _convertedItems = [];

    public TItem this[int index]
    {
        get
        {
            if (_convertedItems.TryGetValue(index, out TItem? cachedValue))
            {
                return cachedValue;
            }

            using (GIL.Acquire())
            {
                using PythonObject value = PythonObject.Create(CAPI.PySequence_GetItem(listObject, index));
                TItem result = value.As<TItem>();
                _convertedItems[index] = result;
                return result;
            }
        }
    }

    public int Count
    {
        get
        {
            using (GIL.Acquire())
            {
                return (int)CAPI.PySequence_Size(listObject);
            }
        }
    }

    public void Dispose() => listObject.Dispose();

    public IEnumerator<TItem> GetEnumerator()
    {
        // TODO: If someone fetches the same index multiple times, we cache the result to avoid multiple round trips to Python
        using (GIL.Acquire())
        {
            return new PythonEnumerable<TItem>(listObject);
        }
    }

    PythonObject ICloneable.Clone() => listObject.Clone();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
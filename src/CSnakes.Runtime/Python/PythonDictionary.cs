﻿using CSnakes.Runtime.CPython;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace CSnakes.Runtime.Python;

internal class PythonDictionary<TKey, TValue>(PythonObject dictionary) : IReadOnlyDictionary<TKey, TValue>, IDisposable, ICloneable
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary = [];
    private readonly PythonObject _dictionaryObject = dictionary;

    public TValue this[TKey key]
    {
        get
        {
            if (_dictionary.TryGetValue(key, out TValue? value))
            {
                return value;
            }
            using (GIL.Acquire())
            {
                using PythonObject keyPyObject = PythonObject.From(key);
                using PythonObject pyObjValue = PythonObject.Create(API.GetItemOfPyMapping(_dictionaryObject, keyPyObject));
                TValue managedValue = pyObjValue.As<TValue>();

                _dictionary[key] = managedValue;
                return managedValue;
            }
        }
    }

    public IEnumerable<TKey> Keys
    {
        get
        {
            using (GIL.Acquire())
            {
                return new PythonEnumerable<TKey>(PythonObject.Create(API.PyMapping_Keys(_dictionaryObject)));
            }
        }
    }

    public IEnumerable<TValue> Values
    {
        get
        {
            using (GIL.Acquire())
            {
                return new PythonEnumerable<TValue>(PythonObject.Create(API.PyMapping_Values(_dictionaryObject)));
            }
        }
    }

    public int Count
    {
        get
        {
            using (GIL.Acquire())
            {
                return (int)API.PyMapping_Size(_dictionaryObject);
            }
        }
    }

    public bool ContainsKey(TKey key)
    {
        if (_dictionary.ContainsKey(key))
        {
            return true;
        }

        using (GIL.Acquire())
        {
            using PythonObject keyPyObject = PythonObject.From(key);
            return API.PyMapping_HasKey(_dictionaryObject, keyPyObject) == 1;
        }
    }

    public void Dispose() => _dictionaryObject.Dispose();

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        using (GIL.Acquire())
        {
            using var items = PythonObject.Create(API.PyMapping_Items(_dictionaryObject));
            return new PythonEnumerable<KeyValuePair<TKey, TValue>, PythonObjectImporter<TKey, TValue>>(items).GetEnumerator();
        }
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (ContainsKey(key))
        {
            value = this[key];
            return true;
        }

        value = default;
        return false;
    }

    PythonObject ICloneable.Clone() => _dictionaryObject.Clone();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}

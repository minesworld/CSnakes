using CSnakes.Runtime.Python;

internal interface IPythonObjectImporter<out T>
{
    static abstract T Import(PythonObject pyObj);
}

internal sealed class PythonObjectImporter<T> :
    IPythonObjectImporter<T>
{
    private PythonObjectImporter() { }

    public static T Import(PythonObject pyObj) => pyObj.As<T>();
}

internal sealed class PythonObjectImporter<TKey, TValue> :
    IPythonObjectImporter<KeyValuePair<TKey, TValue>>
{
    private PythonObjectImporter() { }

    public static KeyValuePair<TKey, TValue> Import(PythonObject pyObj) => pyObj.As<TKey, TValue>();
}

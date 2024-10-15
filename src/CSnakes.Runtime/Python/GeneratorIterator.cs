﻿using System.Collections;

namespace CSnakes.Runtime.Python;
public class GeneratorIterator<TYield, TSend, TReturn>(PythonObject generator) : IGeneratorIterator<TYield, TSend, TReturn>
{
    private readonly PythonObject generator = generator;
    private readonly PythonObject nextPyFunction = generator.GetAttr("__next__");
    private readonly PythonObject closePyFunction = generator.GetAttr("close");
    private readonly PythonObject sendPyFunction = generator.GetAttr("send");

    private TYield current = default!;

    public TYield Current => current;

    object IEnumerator.Current => Current!;

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            generator.Dispose();
            nextPyFunction.Dispose();
            closePyFunction.Call().Dispose();
            closePyFunction.Dispose();
            sendPyFunction.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IEnumerator<TYield> GetEnumerator() => this;


    public bool MoveNext()
    {
        try
        {
            using PythonObject result = nextPyFunction.Call();
            current = result.As<TYield>();
            return true;
        }
        catch (PythonInvocationException pyO) when (pyO.PythonExceptionType == "StopIteration")
        {
            return false;
        }
    }

    public void Reset() => throw new NotSupportedException();

    public TYield Send(TSend value)
    {
        try
        {
            using PythonObject sendValue = PythonObject.From(value);
            using PythonObject result = sendPyFunction.Call(sendValue);
            current = result.As<TYield>();
            return current;
        }
        catch (PythonInvocationException pyO) when (pyO.PythonExceptionType == "StopIteration")
        {
            throw new ArgumentOutOfRangeException("Generator is exhausted.");
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this;

}

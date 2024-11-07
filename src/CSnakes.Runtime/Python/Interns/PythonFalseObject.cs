using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PythonFalseObject : ImmortalPythonObject
{
    public PythonFalseObject() : base(API.PyBool_FromLong(0))
    {
    }

    public override bool IsNone() => false;

    public override string GetRepr() => ToString();

    public override string ToString() => "False";

    internal override PythonObject Clone() => this;
}

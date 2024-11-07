using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PythonTrueObject : ImmortalPythonObject
{
    public PythonTrueObject() : base(API.PyBool_FromLong(1))
    {
    }

    public override bool IsNone() => false;

    public override string GetRepr() => ToString();

    public override string ToString() => "True";

    internal override PythonObject Clone() => this;
}

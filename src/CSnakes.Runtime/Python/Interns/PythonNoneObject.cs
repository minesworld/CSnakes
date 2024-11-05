using CSnakes.Runtime.CPython;

namespace CSnakes.Runtime.Python.Interns;

internal sealed class PythonNoneObject : ImmortalPythonObject
{
    public PythonNoneObject() : base(CAPI.GetNone())
    {
    }

    public override bool IsNone() => true;

    public override string GetRepr() => ToString();

    public override string ToString() => "None";

    internal override PythonObject Clone() => this;
}

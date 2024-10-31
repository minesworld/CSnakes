﻿using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Python;
public class AttributeTests : RuntimeTestBase
{
    [Fact]
    public void TestHasAttrFile()
    {
        using (GIL.Acquire())
        {
            using PythonObject sys = Import.ImportModule("os");
            Assert.NotNull(sys);
            Assert.True(sys.HasAttr("__file__"));
        }
    }

    [Fact]
    public void TestHasAttrName()
    {
        using (GIL.Acquire())
        {
            using PythonObject sys = Import.ImportModule("os");
            Assert.NotNull(sys);
            Assert.True(sys.HasAttr("__name__"));
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimdSharp.Test;

[TestClass]
public class SimdTest
{
    [TestMethod]
    public void SimdTest_Empty()
    {
        Simd.Empty();
    }
}

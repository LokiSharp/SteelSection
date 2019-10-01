using NUnit.Framework;
using static SteelSection.SteelSection;

namespace SteelSectionTests
{
    [TestFixture]
    public class SteelSectionTests
    {
        [Test]
        public void CalcSteelSectionTest()
        {
            var resultC = CalcSteelSection("C160*60*20*2.5");
            Assert.AreEqual(0.000775, resultC[0], 0.0000001);
            Assert.AreEqual(0.00608, resultC[1], 0.00001);
            Assert.AreEqual(0.6363, resultC[2], 0.0001);

            var resultH = CalcSteelSection("H300*200*6*8");
            Assert.AreEqual(0.004904, resultH[0], 0.000001);
            Assert.AreEqual(0.03849, resultH[1], 0.00001);
            Assert.AreEqual(1.38, resultH[2], 0.01);

            var resultZ = CalcSteelSection("Z160*60*20*2.5");
            Assert.AreEqual(0.000782, resultZ[0], 0.000001);
            Assert.AreEqual(0.00614, resultZ[1], 0.00001);
            Assert.AreEqual(0.6285, resultZ[2], 0.01);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static SteelSection.Core.SteelSection;

namespace SteelSection.Tests
{
    [TestClass]
    public class SteelSectionCoreTests
    {
        [TestMethod]
        public void CalcHBeamSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("H300*200*6*8", 7.85);
            Assert.AreEqual(0.004904, sectionalArea, 0.000001);
            Assert.AreEqual(0.03849, theoreticalWeight, 0.00001);
            Assert.AreEqual(1.38, surfaceArea, 0.01);
        }

        [TestMethod]
        public void CalcCBeamSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("C160*60*20*2.5", 7.85);
            Assert.AreEqual(0.000775, sectionalArea, 0.0000001);
            Assert.AreEqual(0.00608, theoreticalWeight, 0.00001);
            Assert.AreEqual(0.6257, surfaceArea, 0.0001);
        }

        [TestMethod]
        public void CalcZBeamSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("Z160*60*20*2.5", 7.85);
            Assert.AreEqual(0.000782, sectionalArea, 0.000001);
            Assert.AreEqual(0.00614, theoreticalWeight, 0.00001);
            Assert.AreEqual(0.6285, surfaceArea, 0.001);
        }

        [TestMethod]
        public void CalcIBeamSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("I10", 7.85);
            Assert.AreEqual(0.0014345, sectionalArea, 0.000001);
            Assert.AreEqual(0.011261, theoreticalWeight, 0.00001);
            Assert.AreEqual(0.472, surfaceArea, 0.001);
        }

        [TestMethod]
        public void CalcHxBeamSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("HW100*100", 7.85);
            Assert.AreEqual(0.002158, sectionalArea, 0.000001);
            Assert.AreEqual(0.0169, theoreticalWeight, 0.00001);
            Assert.AreEqual(0.588, surfaceArea, 0.001);
        }

        [TestMethod]
        public void CalcCSteelSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("CS5", 7.85);
            Assert.AreEqual(0.0006928, sectionalArea, 0.000001);
            Assert.AreEqual(0.005438, theoreticalWeight, 0.00001);
            Assert.AreEqual(0.225, surfaceArea, 0.001);
        }

        [TestMethod]
        public void CalcASteelSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("A20*3", 7.85);
            Assert.AreEqual(0.0001132, sectionalArea, 0.000001);
            Assert.AreEqual(0.000889, theoreticalWeight, 0.00001);
            Assert.AreEqual(0.074, surfaceArea, 0.001);
        }

        [TestMethod]
        public void CalcUaSteelSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("UA25*16*3", 7.85);
            Assert.AreEqual(0.0001162, sectionalArea, 0.000001);
            Assert.AreEqual(0.000912, theoreticalWeight, 0.00001);
            Assert.AreEqual(0.076, surfaceArea, 0.001);
        }

        [TestMethod]
        public void CalcTtBeamSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("RT120*80*6", 7.85);
            Assert.AreEqual(0.002256, sectionalArea, 0.000001);
            Assert.AreEqual(0.0177096, theoreticalWeight, 0.00001);
            Assert.AreEqual(0.4, surfaceArea, 0.001);
        }

        [TestMethod]
        public void CalcCtBeamSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("CT89*2.5", 7.85);
            Assert.AreEqual(0.0006793, sectionalArea, 0.000001);
            Assert.AreEqual(0.005332, theoreticalWeight, 0.00001);
            Assert.AreEqual(0.28, surfaceArea, 0.001);
        }

        [TestMethod]
        public void CalcRsSteelSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("RS32", 7.85);
            Assert.AreEqual(0.0008042, sectionalArea, 0.000001);
            Assert.AreEqual(0.006313, theoreticalWeight, 0.00001);
            Assert.AreEqual(0.101, surfaceArea, 0.001);
        }
    }
}
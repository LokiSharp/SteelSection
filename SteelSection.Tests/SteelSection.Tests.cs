using System;
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
            Assert.AreEqual(0.6207, surfaceArea, 0.0001);
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
            // GB/T 706-2016 表 A.1，10 号外表面积 0.432 m²/m。原简式为 0.472。
            Assert.AreEqual(0.432, surfaceArea, 0.0001);
        }

        [TestMethod]
        public void CalcIBeamTableSurfaceFollowsAppendixNotSharpPerimeter()
        {
            var surface20A = CalcSteelSection("I20a", 7.85).Item3;
            var surface50C = CalcSteelSection("I50c", 7.85).Item3;
            Assert.AreEqual(0.742, surface20A, 0.0001);
            Assert.AreEqual(1.547, surface50C, 0.0001);
        }

        [TestMethod]
        public void CalcHxBeamSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("HW100*100", 7.85);
            Assert.AreEqual(0.002158, sectionalArea, 0.000001);
            Assert.AreEqual(0.0169, theoreticalWeight, 0.00001);
            // 直角周长 (2*100+4*100-2*6)/1000。若误用翼缘厚度 8 mm，结果是 0.584。
            Assert.AreEqual(0.588, surfaceArea, 0.0001);
        }

        [TestMethod]
        public void CalcHxBeamSurfaceUsesWebThickness()
        {
            // HW500*500：H=502，B=470，t1=20，t2=25。周长用 t1。
            var surfaceArea = CalcSteelSection("HW500*500", 7.85).Item3;
            Assert.AreEqual((502.0 * 2 + 470.0 * 4 - 20.0 * 2) / 1000.0, surfaceArea, 0.0000001);
        }

        [TestMethod]
        public void CalcCSteelSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("CS5", 7.85);
            Assert.AreEqual(0.0006928, sectionalArea, 0.000001);
            Assert.AreEqual(0.005438, theoreticalWeight, 0.00001);
            // GB/T 706-2016 表 A.2，5 号外表面积 0.226 m²/m。
            Assert.AreEqual(0.226, surfaceArea, 0.0001);
        }

        [TestMethod]
        public void CalcCSteelTableSurfaceAndCorrectedArea()
        {
            var surface25A = CalcSteelSection("[25a", 7.85).Item3;
            Assert.AreEqual(0.722, surface25A, 0.0001);

            // 6.5 号：表4 hd+2t(b-d)+0.339(r²-r₁²)=829.2 mm²；表 A.2 外表面积 0.267 m²/m。
            var (area65, weight65, surface65) = CalcSteelSection("CS6.5", 7.85);
            Assert.AreEqual(0.0008292, area65, 0.0000001);
            Assert.AreEqual(0.006509, weight65, 0.0000001);
            Assert.AreEqual(0.267, surface65, 0.0001);

            var (area10, weight10, surface10) = CalcSteelSection("槽钢10", 7.85);
            Assert.AreEqual(0.0012744, area10, 0.0000001);
            Assert.AreEqual(0.010004, weight10, 0.0000001);
            Assert.AreEqual(0.365, surface10, 0.0001);
        }

        [TestMethod]
        public void CalcASteelSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("A20*3", 7.85);
            Assert.AreEqual(0.0001132, sectionalArea, 0.000001);
            Assert.AreEqual(0.000889, theoreticalWeight, 0.00001);
            // 现式 (4*20-2*3)/1000=0.074。表 A.3 同一行外表面积为 0.078，差值保留为 0.004。
            Assert.AreEqual(0.074, surfaceArea, 0.0001);
            Assert.AreEqual(0.004, 0.078 - surfaceArea, 0.0001);
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
            // t=6 mm，附录 A 取 R=2t=12 mm，r=6 mm。
            // A=[2*6*(120+80-12)-(4-π)(12²-6²)]/1e6。
            const double pi = Math.PI;
            const double areaMm2 = 2.0 * 6.0 * (120.0 + 80.0 - 12.0) - (4.0 - pi) * (144.0 - 36.0);
            Assert.AreEqual(areaMm2 / 1000000.0, sectionalArea, 0.0000000001);
            Assert.AreEqual(sectionalArea * 7.85, theoreticalWeight, 0.0000000001);
            Assert.AreEqual(2.0 * (120.0 + 80.0 - 48.0 + pi * 12.0) / 1000.0, surfaceArea, 0.0000000001);
        }

        [TestMethod]
        public void CalcRectangularTubeNominalRadiusBands()
        {
            // t=8 mm 属于 6<t≤10，R=2.5t=20；边长足够容纳该半径。
            var rect8 = CalcSteelSection("RT200*100*8", 7.85);
            var area = rect8.Item1;
            var surface = rect8.Item3;
            const double pi = Math.PI;
            const double radius = 20.0;
            const double inner = 12.0;
            const double areaMm2 = 2.0 * 8.0 * (200.0 + 100.0 - 16.0) - (4.0 - pi) * (radius * radius - inner * inner);
            Assert.AreEqual(areaMm2 / 1000000.0, area, 0.0000000001);
            Assert.AreEqual(2.0 * (200.0 + 100.0 - 4.0 * radius + pi * radius) / 1000.0, surface, 0.0000000001);

            // t=12 mm，R=3t=36。
            var rect12 = CalcSteelSection("RT300*200*12", 7.85);
            var thickArea = rect12.Item1;
            var thickSurface = rect12.Item3;
            const double thickRadius = 36.0;
            const double thickInner = 24.0;
            const double thickMm2 = 2.0 * 12.0 * (300.0 + 200.0 - 24.0) - (4.0 - pi) * (thickRadius * thickRadius - thickInner * thickInner);
            Assert.AreEqual(thickMm2 / 1000000.0, thickArea, 0.0000000001);
            Assert.AreEqual(2.0 * (300.0 + 200.0 - 4.0 * thickRadius + pi * thickRadius) / 1000.0, thickSurface, 0.0000000001);
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
            Assert.AreEqual(Math.PI * 16.0 * 16.0 / 1000000.0, sectionalArea, 0.0000000001);
            Assert.AreEqual(sectionalArea * 7.85, theoreticalWeight, 0.0000000001);
            Assert.AreEqual(Math.PI * 32.0 / 1000.0, surfaceArea, 0.0000000001);
        }

        [TestMethod]
        public void CalcPlateSectionUsesTwoFacesOnly()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("缀板200*10", 7.85);
            Assert.AreEqual(0.002, sectionalArea, 0.0000000001);
            Assert.AreEqual(0.0157, theoreticalWeight, 0.0000000001);
            // 两大面 2*200/1000。含两条纵边则是 2*(200+10)/1000=0.420。
            Assert.AreEqual(0.4, surfaceArea, 0.0000000001);
        }

        [TestMethod]
        public void CalcZeroDensityKeepsPreviousDensity()
        {
            try
            {
                var standardWeight = CalcSteelSection("RS10", 7.85).Item2;
                var customWeight = CalcSteelSection("RS10", 8.0).Item2;
                var reusedWeight = CalcSteelSection("RS10", 0).Item2;
                Assert.AreEqual(standardWeight * 8.0 / 7.85, customWeight, 0.0000000001);
                Assert.AreEqual(customWeight, reusedWeight, 0.0000000001);
            }
            finally
            {
                CalcSteelSection("RS10", 7.85);
            }
        }
    }
}
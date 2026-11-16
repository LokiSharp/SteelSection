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
            // GB/T 11263-2024 表 A.1：H100×100×6×8，r=8，外表面积 0.574 m²/m。直角周长是 0.588。
            Assert.AreEqual(0.574, surfaceArea, 0.0005);
        }

        [TestMethod]
        public void CalcHxBeamSurfaceMatchesAppendixFilletFormula()
        {
            // HW125*125：H=125，B=125，t1=6.5，r=8。表 A.1 外表面积 0.723 m²/m。
            var small = CalcSteelSection("HW125*125", 7.85).Item3;
            Assert.AreEqual(0.723, small, 0.0005);

            // HW500*500：H=502，B=470，t1=20，r=22。周长用腹板厚度和内圆角，不用翼缘厚度。
            var surfaceArea = CalcSteelSection("HW500*500", 7.85).Item3;
            var expected = (502.0 * 2 + 470.0 * 4 - 20.0 * 2 - (8 - 2 * Math.PI) * 22.0) / 1000.0;
            Assert.AreEqual(expected, surfaceArea, 0.0000001);
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
        public void CalcParallelChannelSectionTest()
        {
            // GB/T 706-2026 表 A.3。PC8：10.35 cm²、8.13 kg/m、0.315 m²/m。
            var (area8, weight8, surface8) = CalcSteelSection("PC8", 7.85);
            Assert.AreEqual(0.001035, area8, 0.0000001);
            Assert.AreEqual(0.00813, weight8, 0.0000001);
            Assert.AreEqual(0.315, surface8, 0.0001);

            // 简记 PC20b 与全称 PC200×75×9×11 是同一行。
            var brief = CalcSteelSection("PC20b", 7.85);
            var full = CalcSteelSection("平行腿槽钢200*75*9*11", 7.85);
            Assert.AreEqual(0.003304, brief.Item1, 0.0000001);
            Assert.AreEqual(0.0259, brief.Item2, 0.0000001);
            Assert.AreEqual(0.673, brief.Item3, 0.0001);
            Assert.AreEqual(brief.Item1, full.Item1, 0.0000001);
            Assert.AreEqual(brief.Item2, full.Item2, 0.0000001);
            Assert.AreEqual(brief.Item3, full.Item3, 0.0001);

            // PC40c：91.61 cm²、71.9 kg/m、1.171 m²/m。
            var (area40, weight40, surface40) = CalcSteelSection("PC40c", 7.85);
            Assert.AreEqual(0.009161, area40, 0.0000001);
            Assert.AreEqual(0.0719, weight40, 0.0000001);
            Assert.AreEqual(1.171, surface40, 0.0001);
        }

        [TestMethod]
        public void CalcGb11263AddedSections()
        {
            // 附录 A。热轧 H 不占用焊接 H 的代号。H95×48×3.2×4.5：7.62 cm²、5.98 kg/m、0.362 m²/m。
            var (areaH, weightH, surfaceH) = CalcSteelSection("热轧H95*48*3.2*4.5", 7.85);
            Assert.AreEqual(0.000762, areaH, 0.0000001);
            Assert.AreEqual(0.00598, weightH, 0.0000001);
            Assert.AreEqual(0.362, surfaceH, 0.0001);

            // 与旧目录 HW100×100 同一组尺寸，表 A.1 外表面积 0.574 m²/m。
            var rolled = CalcSteelSection("热轧H100*100*6*8", 7.85);
            Assert.AreEqual(0.002158, rolled.Item1, 0.0000001);
            Assert.AreEqual(0.0169, rolled.Item2, 0.0000001);
            Assert.AreEqual(0.574, rolled.Item3, 0.0001);

            // 附录 B，2024 版新增的 HK。HK120×64×4.4×6.3。
            var (areaHk, weightHk, surfaceHk) = CalcSteelSection("HK120*64*4.4*6.3", 7.85);
            Assert.AreEqual(0.001321, areaHk, 0.0000001);
            Assert.AreEqual(0.0104, weightHk, 0.0000001);
            Assert.AreEqual(0.475, surfaceHk, 0.0001);

            // 附录 C。T50×50×5×7：5.92 cm²、4.65 kg/m、0.193 m²/m。
            var split = CalcSteelSection("剖分T50*50*5*7", 7.85);
            var tee = CalcSteelSection("T50*50*5*7", 7.85);
            Assert.AreEqual(0.000592, split.Item1, 0.0000001);
            Assert.AreEqual(0.00465, split.Item2, 0.0000001);
            Assert.AreEqual(0.193, split.Item3, 0.0001);
            Assert.AreEqual(split.Item1, tee.Item1, 0.0000001);
            Assert.AreEqual(split.Item3, tee.Item3, 0.0001);
        }

        [TestMethod]
        public void CalcBarSectionsMatchGb702Tables()
        {
            // 表 A.1：方钢边长 10 mm，理论重量 0.785 kg/m。
            var square = CalcSteelSection("方钢10", 7.85);
            Assert.AreEqual(0.0001, square.Item1, 0.0000000001);
            Assert.AreEqual(0.000785, square.Item2, 0.0000000001);
            Assert.AreEqual(0.040, square.Item3, 0.0000001);

            // 表 A.4：六角钢对边 8 mm，面积 0.5543 cm²，重量 0.435 kg/m。
            var hex = CalcSteelSection("六角钢8", 7.85);
            Assert.AreEqual(0.5543 / 10000.0, hex.Item1, 0.00000002);
            Assert.AreEqual(0.000435, hex.Item2, 0.0000002);

            // 表 A.4：八角钢对边 16 mm，面积 2.121 cm²，重量 1.66 kg/m。
            var oct = CalcSteelSection("八角钢16", 7.85);
            Assert.AreEqual(2.121 / 10000.0, oct.Item1, 0.00000005);
            Assert.AreEqual(0.00166, oct.Item2, 0.00001);
        }

        [TestMethod]
        public void CalcASteelSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("A20*3", 7.85);
            Assert.AreEqual(0.0001132, sectionalArea, 0.000001);
            Assert.AreEqual(0.000889, theoreticalWeight, 0.00001);
            // GB/T 706-2026 表 A.4，∠20×3 外表面积 0.078 m²/m。
            Assert.AreEqual(0.078, surfaceArea, 0.0001);

            // ∠100×14 原面积 22.256 cm² 与重量 20.611 kg/m 不相配；表 A.4 为 26.26 cm²，0.785×26.26≈20.614。
            var area100 = CalcSteelSection("A100*14", 7.85).Item1;
            Assert.AreEqual(0.002626, area100, 0.0000001);

            // 表 A.4 新增边宽 280。∠280×20：109.3 cm²、85.8 kg/m、1.103 m²/m。
            var (area280, weight280, surface280) = CalcSteelSection("角钢280*20", 7.85);
            Assert.AreEqual(0.01093, area280, 0.0000001);
            Assert.AreEqual(0.0858, weight280, 0.0000001);
            Assert.AreEqual(1.103, surface280, 0.0001);
        }

        [TestMethod]
        public void CalcUaSteelSectionTest()
        {
            var (sectionalArea, theoreticalWeight, surfaceArea) = CalcSteelSection("UA25*16*3", 7.85);
            Assert.AreEqual(0.0001162, sectionalArea, 0.000001);
            Assert.AreEqual(0.000912, theoreticalWeight, 0.00001);
            // GB/T 706-2026 表 A.5，∠25×16×3 外表面积 0.080 m²/m。
            Assert.AreEqual(0.080, surfaceArea, 0.0001);
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
        public void CalcWeldedBoxAllowsDifferentWallThickness()
        {
            // 200×200×6×9：2×200×9 + 2×(200−18)×6 = 5784 mm²。外皮 2×(200+200)/1000 = 0.8。
            var (area, weight, surface) = CalcSteelSection("箱形200*200*6*9", 7.85);
            Assert.AreEqual(0.005784, area, 0.0000000001);
            Assert.AreEqual(0.005784 * 7.85, weight, 0.0000000001);
            Assert.AreEqual(0.8, surface, 0.0000001);

            // 长边和短边不同，腹板和翼缘厚度也不同。
            var rectangular = CalcSteelSection("焊接矩形管300*200*6*9", 7.85);
            const double rectangularMm2 = 2.0 * 200.0 * 9.0 + 2.0 * (300.0 - 18.0) * 6.0;
            Assert.AreEqual(rectangularMm2 / 1000000.0, rectangular.Item1, 0.0000000001);
            Assert.AreEqual(2.0 * (300.0 + 200.0) / 1000.0, rectangular.Item3, 0.0000001);

            // 三个尺寸不论名称，都是冷弯单壁厚。四个尺寸才是直角、壁厚可以不同。
            var sharp = CalcSteelSection("箱形120*80*6*6", 7.85);
            var coldFormed = CalcSteelSection("RT120*80*6", 7.85);
            Assert.AreEqual(2.0 * 6.0 * (120.0 + 80.0 - 12.0) / 1000000.0, sharp.Item1, 0.0000000001);
            Assert.IsTrue(sharp.Item1 > coldFormed.Item1);
            Assert.AreEqual(coldFormed.Item1, CalcSteelSection("焊接矩形管120*80*6", 7.85).Item1, 0.0000000001);
            Assert.AreEqual(coldFormed.Item1, CalcSteelSection("矩形管120*80*6", 7.85).Item1, 0.0000000001);

            // 四个尺寸时，矩形管和焊接矩形管是同一个直角截面。
            var byRectName = CalcSteelSection("矩形管300*200*6*9", 7.85);
            Assert.AreEqual(rectangular.Item1, byRectName.Item1, 0.0000000001);
            Assert.AreEqual(rectangular.Item2, byRectName.Item2, 0.0000000001);
            Assert.AreEqual(rectangular.Item3, byRectName.Item3, 0.0000001);
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
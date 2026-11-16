using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;

namespace SteelSection.Core
{
    /// <summary>
    /// 型钢截面面积、理论重量和每米侧表面积。
    /// 核实日期 2026-11-13。各计算式的标准依据、单位和几何限制写在对应方法上；
    /// 汇总见仓库 docs/标准核对-2026-11-13.md。标准未给出的表面积按理想化截面周长推导，并标明不是国标公式。
    /// </summary>
    public static class SteelSection
    {
        /// <summary>
        /// 线密度，单位 t/m³，数值上等于 g/cm³。
        /// GB/T 706-2016 第4.4.1条、GB/T 6728-2025 附录A（M=0.785×A，A 以 cm² 计）和
        /// GB/T 709-2019 第9.2条（见代码中钢板注释）均以碳钢 7.85 g/cm³ 计算理论重量。
        /// 截面面积以 m² 返回时，乘以本密度得到 t/m。
        /// density 参数为 0 时不覆盖本字段，因此会沿用上一次非零密度；这是既有接口行为，标准没有规定，故保留。
        /// 该字段为静态可变状态，不同密度的调用会互相影响，也不是线程安全的。
        /// </summary>
        private static double _density = 7.85;

        private static (string type, string[] sectional) ParseSteelSection(string input)
        {
            try
            {
                var inputSplit = Regex.Split(input, @"([A-Za-z\u4e00-\u9fa5\[∟]+)([\d\.A-Za-z\*×~]+)");
                return (type: inputSplit[1], sectional: inputSplit[2].Split('*', 'x', 'X', '×', '~'));
            }
            catch (IndexOutOfRangeException)
            {
                return (type: null, sectional: null);
            }
        }

        /// <summary>
        /// 计算单位长度的截面面积（m²）、理论重量（t/m）和侧表面积（m²/m，不含杆件两端端面）。
        /// </summary>
        public static (double, double, double) CalcSteelSection(string input, double density)
        {
            if (density != 0) _density = density;
            var (type, sectional) = ParseSteelSection(input);

            switch (type)
            {
                case "H":
                case "H型钢":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    var steel = new HBeam(s[0], s[1], s[1], s[2], s[3], s[3]);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "UHH":
                case "变截面H型钢变高":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    var steel = new HBeam((s[0] + s[1]) / 2.0, s[2], s[2], s[3], s[4], s[4]);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "UBH":
                case "变截面H型钢非对称":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    var steel = new HBeam(s[0], s[1], s[2], s[3], s[4], s[5]);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "HW":
                case "HM":
                case "HN":
                case "HT":
                {
                    var steel = new HxBeam($"{type}{sectional[0]}*{sectional[1]}");
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "C":
                case "C型钢":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    var steel = new CBeam(s[0], s[1], s[2], s[3]);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "Z":
                case "Z型钢":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    var steel = new ZBeam(s[0], s[1], s[2], s[3]);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "I":
                case "UB":
                case "工字钢":
                {
                    var steel = new IBeam(sectional[0]);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "[":
                case "CS":
                case "槽钢":
                {
                    var steel = new CSteel(sectional[0]);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "PC":
                case "平行腿槽钢":
                {
                    var steel = new PcSteel(sectional);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "热轧H":
                {
                    var steel = new Gb11263Section("H", sectional);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "HK":
                {
                    var steel = new Gb11263Section("HK", sectional);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "T":
                case "剖分T":
                {
                    var steel = new Gb11263Section("T", sectional);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "∟":
                case "A":
                case "角钢":
                {
                    var steel = new ASteel($"{sectional[0]}*{sectional[1]}");
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "UA":
                {
                    var steel = new UaSteel($"{sectional[0]}*{sectional[1]}*{sectional[2]}");
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "RT":
                case "矩形管":
                case "焊接矩形管":
                case "箱形":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    // 四个数是高度×宽度×腹板厚×翼缘厚，直角箱形，壁厚可以不同。
                    // 三个数是高度×宽度×壁厚。焊接矩形管、箱形仍按直角；RT、矩形管按冷弯圆角。
                    if (s.Length >= 4)
                    {
                        var box = new WeldedBox(s[0], s[1], s[2], s[3]);
                        return (box.CalcSectionalArea(), box.CalcTheoreticalWeight(), box.CalcSurfaceArea());
                    }

                    if (type == "焊接矩形管" || type == "箱形")
                    {
                        var box = new WeldedBox(s[0], s[1], s[2], s[2]);
                        return (box.CalcSectionalArea(), box.CalcTheoreticalWeight(), box.CalcSurfaceArea());
                    }

                    var steel = new RtBeam(s[0], s[1], s[2]);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "CT":
                case "圆管":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    var steel = new CtBeam(s[0], s[1]);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "RS":
                case "圆钢":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    var steel = new RsSteel(s[0]);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "方钢":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    var steel = new StraightBar(s[0], 1.0, 4.0);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "六角钢":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    var steel = new StraightBar(s[0], Math.Sqrt(3) / 2.0, 2.0 * Math.Sqrt(3));
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "八角钢":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    var steel = new StraightBar(s[0], 2.0 * (Math.Sqrt(2) - 1.0), 8.0 * (Math.Sqrt(2) - 1.0));
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                case "PS":
                case "板":
                case "缀板":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    var steel = new PlateSteel(s[0], s[1]);
                    return (steel.CalcSectionalArea(), steel.CalcTheoreticalWeight(), steel.CalcSurfaceArea());
                }
                default:
                    return (0.0, 0.0, 0.0);
            }
        }

        private static CsvReader GetResourceDataReader(string filename)
        {
            var projectName = Assembly.GetExecutingAssembly().GetName().Name;
            var thisAssembly = Assembly.GetExecutingAssembly();
            var fs = thisAssembly.GetManifestResourceStream(projectName + ".Resources.data." + filename);
            var sr = new StreamReader(fs ?? throw new InvalidOperationException(), Encoding.Default);

            return new CsvReader(sr, CultureInfo.InvariantCulture);
        }

        private interface ISteel
        {
            double CalcSectionalArea();
            double CalcTheoreticalWeight();
            double CalcSurfaceArea();
        }

        private class HBeam : ISteel
        {
            public HBeam(double h, double b1, double b2, double tw, double t1, double t2)
            {
                H = h;
                B1 = b1;
                B2 = b2;
                Tw = tw;
                T1 = t1;
                T2 = t2;
            }

            private double B1 { get; }
            private double B2 { get; }
            private double H { get; }
            private double Tw { get; }
            private double T1 { get; }
            private double T2 { get; }

            /// <summary>
            /// 焊接或拼装 H 截面面积，m²。尺寸 mm。
            /// 直角拼接：(腹板净高)×腹板厚 + 两块翼缘。不计焊缝和圆角。
            /// 这不是热轧 H 型钢的国标表值。热轧成品见 <see cref="HxBeam"/> 与 GB/T 11263-2024。
            /// </summary>
            public double CalcSectionalArea()
            {
                return ((H - (T1 + T2)) * Tw + B1 * T1 + B2 * T2) / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            /// <summary>
            /// 每米侧表面积，m²/m。理想化直角 H 的截面外轮廓周长，不是国标公式。
            /// 顶翼缘外表面 B1、底翼缘外表面 B2、两翼缘内侧合计 (B1-Tw)+(B2-Tw)、
            /// 腹板两侧 2(H-T1-T2)、四个翼缘端头 2T1+2T2。翼缘厚度抵消后为
            /// 2H+2B1+2B2-2Tw，再除以 1000。
            /// 忽略圆角、焊缝、翼缘坡口和杆件两端端面。输入不含圆角半径，故不能套用 GB/T 11263-2024 的圆角项。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return (H * 2 + B1 * 2 + B2 * 2 - Tw * 2) / 1000;
            }
        }

        private class CBeam : ISteel
        {
            public CBeam(double h, double b, double c, double t)
            {
                H = h;
                B = b;
                C = c;
                T = t;
            }

            private double H { get; }
            private double B { get; }
            private double C { get; }
            private double T { get; }

            /// <summary>
            /// 冷弯内卷边槽钢截面面积，m²。尺寸 mm，按外侧尺寸、壁厚均匀、四个弯角均为 90° 且圆角半径为 0 的中线长度。
            /// 中线：腹板 H-t，两翼缘各 B-t，两卷边各 C-t/2，合计 H+2B+2C-4t，再乘 t。
            /// GB/T 6728-2025（2025-08-29 发布，2026-03-01 实施，代替 GB/T 6728-2017 与 GB/T 6723-2017）
            /// 第6.1.2.2条把开口型钢面积放在表7～表15，并写明面积仅供参考、表外尺寸由供需双方协商；
            /// 附录 A 只给圆形、方形、矩形闭口型钢的通式，没有内卷边槽钢通式。
            /// 第6.1.2.3条表16规定外圆弧半径是随屈服强度和壁厚变化的区间，不是 0。
            /// 因此本式不是国标公式。本次未能从该标准 PDF 的开口型钢表中解出可核对的数值行（部分页为无法解码的内嵌字体），故保留原式。
            /// 状态页：https://std.samr.gov.cn/gb/search/gbDetailed?id=3DBA2132869D0D16E06397BE0A0A8119
            /// </summary>
            public double CalcSectionalArea()
            {
                return (H + B * 2 + C * 2 - T * 4) * T / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            /// <summary>
            /// 每米侧表面积，m²/m。不是国标公式。整理后为
            /// [2(H+2B+2C-8t)+2πt+2t]/1000。
            /// 其中 2πt 等于 4 段半径为 t 的 90° 外圆弧，2t 对应两条卷边切口。
            /// 这相当于内半径取 0、外半径取壁厚，与 GB/T 6728-2025 表16的半径区间不符，也未计入内圆弧和杆件端面。
            /// 标准附录 A 的「每米长度的表面积」只适用于闭口型钢。开口型钢表的数值本次未能可靠读出，故不改式。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return ((H + B * 2 + C * 2 - T * 2 * 4) * 2 + Math.PI * T * 2 + T * 2) / 1000;
            }
        }

        private class ZBeam : ISteel
        {
            public ZBeam(double h, double b, double c, double t)
            {
                H = h;
                B = b;
                C = c;
                T = t;
            }

            private double H { get; }
            private double B { get; }
            private double C { get; }
            private double T { get; }

            /// <summary>
            /// 冷弯卷边 Z 型钢截面面积，m²。尺寸 mm。
            /// 现式为 (H+2B+2C-2t-t·π·45/180)·t。其中 t·π·45/180 = πt/4，等于半径 t、圆心角 45° 的弧长。
            /// 与 <see cref="CBeam"/> 的直角中线扣减不同。GB/T 6728-2025 图14定义了卷边 Z 型钢（代号 ZJ）的 H、B、C、t、R，
            /// 第6.1.2.5条表19只给出弯曲角度允许偏差，本次读到的正文没有写明卷边公称角度就是 45°。
            /// 附录 A 没有开口型钢面积通式。因此 45° 是原代码自带的几何假设，不是已核对的国标公式，予以保留。
            /// 同样忽略表16的外圆弧半径区间和杆件端面。
            /// </summary>
            public double CalcSectionalArea()
            {
                return (H + B * 2 + C * 2 - T * 2 - T * Math.PI * 45 / 180) * T / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            /// <summary>
            /// 每米侧表面积，m²/m。不是国标公式。整理后为
            /// [2(H+2B+2C-8t)+3πt+2t]/1000。
            /// 3πt 比 C 型钢多出 πt，不能从已读到的 GB/T 6728-2025 条款还原成确定的弯角组合，故保留原式。
            /// 限制与面积相同：未按表16取外圆弧半径，不含杆件端面。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return ((H + B * 2 + C * 2 - T * 2 * 4) * 2 + Math.PI * T * 3 + T * 2) / 1000;
            }
        }

        private class IBeam : ISteel
        {
            public IBeam(string type)
            {
                var csvReader = GetResourceDataReader("IBeam.csv").GetRecords<CsvIBeam>();
                var filtered = csvReader.First(r => r.Type == type);
                Type = type;
                H = filtered.H;
                B = filtered.B;
                D = filtered.D;
                T = filtered.T;
                R = filtered.R;
                R1 = filtered.R1;
                SectionalArea = filtered.SectionalArea;
                TheoreticalWeight = filtered.TheoreticalWeight;
                SurfaceArea = filtered.SurfaceArea;
            }

            private string Type { get; }
            private double H { get; }
            private double B { get; }
            private double D { get; }
            private double T { get; }
            private double R { get; }
            private double R1 { get; }
            private double SectionalArea { get; }
            private double TheoreticalWeight { get; }
            private double SurfaceArea { get; }

            /// <summary>
            /// 截面面积，m²。CSV 中为 mm²。
            /// GB/T 706-2016（2016-12-30 发布，2017-09-01 实施；截至 2026-11-13 仍为现行。
            /// GB/T 706-2026 已于 2026-05-25 发布，2026-12-01 才实施，本次不采用）
            /// 第4.4.3条表4：工字钢面积 = hd+2t(b-d)+0.577(r²-r₁²)，尺寸 mm 时结果为 mm²。
            /// 第4.1.2条要求面积、理论重量符合附录 A 表 A.1。表中 t 的图注是「腿中间厚度」。
            /// 用表4系数核算内嵌尺寸，与 CSV 的相对差不超过约 0.1%（例如 10 号：公式 1433.3 mm²，表 A.1 印 14.33 cm²，CSV 为 1434.5 mm²）。
            /// 差值小于表中重量通常的 0.1 kg/m 修约，故不改面积列。
            /// https://openstd.samr.gov.cn/bzgk/std/newGbInfo?hcno=80EC2383403568E9F0F850B044C0DC5F
            /// https://openstd.samr.gov.cn/bzgk/std/newGbInfo?hcno=284DD62E1E487105E757CC4B771A6C65
            /// </summary>
            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            /// <summary>
            /// 理论重量，t/m。CSV 为按 7.85 g/cm³ 给出的 kg/m，再按调用密度缩放。
            /// GB/T 706-2016 第4.4.1条：理论重量按密度 7.85 g/cm³ 计算。
            /// </summary>
            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            /// <summary>
            /// 每米外表面积，m²/m。直接取 GB/T 706-2016 表 A.1「外表面积/(m²/m)」。
            /// 该列在规范性附录的表头中，标准正文没有给出对应计算式（表4只给截面面积）。
            /// 表下注明 r、r₁ 用于孔型设计、不做交货条件；外表面积没有单独的交货条款。
            /// 原式 (2H+4B)/1000 不含腰厚，10 号得到 0.472，表 A.1 为 0.432。直角全周长 2H+4B-2d 为 0.463，仍不等于表值。
            /// 因此改为表值，不再用简式。表值已含腿内侧斜度、内圆弧和腿端圆弧的结果，但标准未公布拆分方法。
            /// 单位就是 m²/m，不再除以 1000。不含杆件两端端面。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return SurfaceArea;
            }
        }

        private class HxBeam : ISteel
        {
            public HxBeam(string type)
            {
                var csvReader = GetResourceDataReader("HXBeam.csv").GetRecords<CsvHxBeam>();
                var filtered = csvReader.First(r => r.Type == type);
                Type = type;
                H = filtered.H;
                B = filtered.B;
                // GB/T 11263-2024 第6.1.1条图1：t1 为腹板厚度，t2 为翼缘厚度。
                // CSV 的 T1、T2 与之对应。原代码把 T2 写成 filtered.T1，与翼缘厚度列不符。
                // 面积和重量读表，表面积只用腹板厚度，所以改正后现有计算结果不变。
                T1 = filtered.T1;
                T2 = filtered.T2;
                R = filtered.R;
                SectionalArea = filtered.SectionalArea;
                TheoreticalWeight = filtered.TheoreticalWeight;
            }

            private string Type { get; }
            private double H { get; }
            private double B { get; }
            private double T1 { get; }
            private double T2 { get; }
            private double R { get; }
            private double SectionalArea { get; }
            private double TheoreticalWeight { get; }

            /// <summary>
            /// 截面面积，m²。CSV 为 mm²。
            /// 现行标准是 GB/T 11263-2024《热轧H型钢和剖分T型钢》（2024-10-26 发布，2025-05-01 实施，代替并废止 GB/T 11263-2017）。
            /// https://openstd.samr.gov.cn/bzgk/std/newGbInfo?hcno=8894AAFBAE28DF71A29374720910304F
            /// 第5.1条将代号改为 H（通用）和 HK（扩展），不再使用 HW/HM/HN/HT。
            /// 本目录仍保留旧代号，以免改变已有调用字符串。2024 附录 A/B 的逐行尺寸本次没有取到全文，不能声称与现行表逐行一致。
            /// 第6.1.2条写明圆角半径、外表面积及惯性矩等为参考值，面积和理论重量的参考公式在附录 D。
            /// 附录 D 的算式原文本次未取得，因此面积继续用内嵌表，不在这里现算。
            /// </summary>
            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            /// <summary>
            /// 理论重量，t/m。内嵌 kg/m 按 7.85 g/cm³，再随调用密度缩放。
            /// GB/T 11263-2024 第6.3.1条：理论重量按密度 7.85 g/cm³。附录 D 公式 (D.2) 为 M=0.785S，S 以 cm² 计。
            /// </summary>
            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            /// <summary>
            /// 每米外表面积，m²/m。GB/T 11263-2024 第6.1.2条把外表面积列为附录参考值，附录 D 给出计算公式。
            /// 所读文本里公式 (D.3) 的符号被拆乱。与表 A.1 一致、并可从四个内圆角推出的式子是
            /// (2H+4B-2t1-(8-2π)r)/1000。直角轮廓是 2H+4B-2t1；每个内圆角用四分之一圆弧代替两段长 r 的直线，
            /// 四个圆角共缩短 (8-2π)r。H100×100×6×8、r=8 的表值是 0.574 m²/m；H125×125×6.5×9、r=8 的表值是 0.723 m²/m。
            /// 不含杆件两端。面积公式 (D.1) 的圆角项是 0.858r²，0.858=4(1-π/4)，不要和这条周长系数混用。
            /// 表 A.1、附录 B（HK）、附录 C（剖分 T）的整表 OCR 不可靠，没有整表替换现有目录；这里只用 CSV 中已有的 H、B、t1、r。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return (H * 2 + B * 4 - T1 * 2 - (8 - 2 * Math.PI) * R) / 1000;
            }
        }

        private class CSteel : ISteel
        {
            public CSteel(string type)
            {
                var csvReader = GetResourceDataReader("CSteel.csv").GetRecords<CsvCSteel>();
                var filtered = csvReader.First(r => r.Type == type);
                Type = type;
                H = filtered.H;
                B = filtered.B;
                D = filtered.D;
                T = filtered.T;
                R = filtered.R;
                R1 = filtered.R1;
                SectionalArea = filtered.SectionalArea;
                TheoreticalWeight = filtered.TheoreticalWeight;
                SurfaceArea = filtered.SurfaceArea;
            }

            private string Type { get; }
            private double H { get; }
            private double B { get; }
            private double D { get; }
            private double T { get; }
            private double R { get; }
            private double R1 { get; }
            private double SectionalArea { get; }
            private double TheoreticalWeight { get; }
            private double SurfaceArea { get; }

            /// <summary>
            /// 截面面积，m²。CSV 为 mm²。
            /// GB/T 706-2016 第4.4.3条表4：槽钢面积 = hd+2t(b-d)+0.339(r²-r₁²)。
            /// 第4.1.2条要求符合表 A.2。绝大多数型号与该式及表 A.2 一致（修约差约 0.1% 以内）。
            /// 6.5 号原 CSV 为 854.7 mm²、6.709 kg/m，表4算出 829.2 mm²、6.509 kg/m，表 A.2 印 8.292 cm²、6.51 kg/m，已按表改正。
            /// 10 号原 CSV 为 1278.4 mm²，表4为 1274.4 mm²，表 A.2 印 12.74 cm²、10.0 kg/m，已按表4改正。
            /// </summary>
            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            /// <summary>
            /// 理论重量，t/m。GB/T 706-2016 第4.4.1条密度 7.85 g/cm³。CSV 为 kg/m，再随调用密度缩放。
            /// </summary>
            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            /// <summary>
            /// 每米外表面积，m²/m。直接取 GB/T 706-2016 表 A.2「外表面积/(m²/m)」，不再除以 1000。
            /// 正文没有表面积公式。原式 (2H+4B-2d-2t)/1000 对 5 号约为 0.225，表 A.2 为 0.226；
            /// 对 25a（h=250,b=78,d=7,t=12）原式为 0.774，表 A.2 印 0.722。小规格接近、大规格偏离，故改为表值。
            /// 直角全周长 2H+4B-2d 会把腿厚端头与内侧缩短抵消，同样不是表值。表值已包含腿斜度与圆弧，标准未给出分解式。
            /// 不含杆件两端端面。25a 的 0.722 低于相邻 24a 的 0.752，按 PDF 文字层和放大字形均为 0.722，未自行改表。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return SurfaceArea;
            }
        }

        /// <summary>
        /// GB/T 706-2026 表 A.3 平行腿槽钢。代号 PC，示例 PC200×75×9×11，简记 PC20b。
        /// 斜腿槽钢仍走 <see cref="CSteel"/>（“[” / CS / 槽钢）。
        /// </summary>
        private class PcSteel : ISteel
        {
            public PcSteel(string[] sectional)
            {
                var rows = GetResourceDataReader("PCSteel.csv").GetRecords<CsvPcSteel>().ToList();
                CsvPcSteel filtered;
                if (sectional.Length == 1)
                {
                    filtered = rows.First(r => r.Type == sectional[0]);
                }
                else
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    filtered = rows.First(r =>
                        Math.Abs(r.H - s[0]) < 0.05 &&
                        Math.Abs(r.B - s[1]) < 0.05 &&
                        Math.Abs(r.Tw - s[2]) < 0.05 &&
                        Math.Abs(r.Tf - s[3]) < 0.05);
                }

                SectionalArea = filtered.SectionalArea;
                TheoreticalWeight = filtered.TheoreticalWeight;
                SurfaceArea = filtered.SurfaceArea;
            }

            private double SectionalArea { get; }
            private double TheoreticalWeight { get; }
            private double SurfaceArea { get; }

            /// <summary>
            /// 截面面积，m²。CSV 为 mm²，取自 GB/T 706-2026 表 A.3。
            /// 表5印的是 H·tw+2·tf1·(B−tw)+1.57r²。逐行验算时，表列面积等于
            /// H·tw+2·tf1·(B−tw)+2(1−π/4)r²（两个内圆角），与 1.57r² 不一致。
            /// PC8：表 10.35 cm²，圆角式 10.355 cm²，1.57r² 式 11.085 cm²。
            /// 理论重量与 0.785×表列面积一致，故目录用表 A.3，不按 1.57 重算。
            /// </summary>
            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            /// <summary>
            /// 理论重量，t/m。GB/T 706-2026 仍按密度 7.85 g/cm³。CSV 为 kg/m，再随调用密度缩放。
            /// </summary>
            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            /// <summary>
            /// 每米外表面积，m²/m。直接取 GB/T 706-2026 表 A.3，不再除以 1000。
            /// 正文没有外表面积公式。不含杆件两端端面。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return SurfaceArea;
            }
        }

        /// <summary>
        /// GB/T 11263-2024 附录 A 的 H、附录 B 的 HK、附录 C 的剖分 T。
        /// 代号 H 仍是焊接 H，热轧通用 H 用「热轧H」。HK 是 2024 版新增的扩展 H 型钢。
        /// </summary>
        private class Gb11263Section : ISteel
        {
            public Gb11263Section(string sectionClass, string[] sectional)
            {
                var s = Array.ConvertAll(sectional, double.Parse);
                var rows = GetResourceDataReader("GB11263.csv").GetRecords<CsvGb11263>();
                var filtered = rows.First(r =>
                    r.Class == sectionClass &&
                    Math.Abs(r.H - s[0]) < 0.05 &&
                    Math.Abs(r.B - s[1]) < 0.05 &&
                    Math.Abs(r.T1 - s[2]) < 0.05 &&
                    Math.Abs(r.T2 - s[3]) < 0.05);
                SectionalArea = filtered.SectionalArea;
                TheoreticalWeight = filtered.TheoreticalWeight;
                SurfaceArea = filtered.SurfaceArea;
            }

            private double SectionalArea { get; }
            private double TheoreticalWeight { get; }
            private double SurfaceArea { get; }

            /// <summary>
            /// 截面面积，m²。CSV 为 mm²。
            /// 附录 D 公式 (D.1)：H 和 HK 的面积 S=[2Bt2+(H−2t2)t1+0.858r²]×10⁻² cm²。
            /// 剖分 T 没有单独的面积公式；表值与 B·t2+(h−t2)t1+0.429r² 一致，0.429 是两个内圆角。
            /// 写入前逐行用上述式子核对，偏差超过 0.25 cm² 的行没有收入。
            /// </summary>
            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            /// <summary>
            /// 理论重量，t/m。GB/T 11263-2024 第6.3.1条密度 7.85 g/cm³，公式 (D.2) 为 M=0.785S。
            /// CSV 为 kg/m，再随调用密度缩放。
            /// </summary>
            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            /// <summary>
            /// 每米外表面积，m²/m。直接取附录表，不再除以 1000。
            /// 公式 (D.3) 印的是 (4B+2H−2t1−0.858r)×10⁻³，单位写成了 cm²/m。
            /// 表值与 (2H+4B−2t1−(8−2π)r)/1000 一致。例如 H100×100×6×8、r=8 的表值是 0.574 m²/m，
            /// 按印出的 0.858r 会得到约 0.581。因此用表值，不按 0.858r 重算。不含杆件两端。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return SurfaceArea;
            }
        }

        private class ASteel : ISteel
        {
            public ASteel(string type)
            {
                var csvReader = GetResourceDataReader("ASteel.csv").GetRecords<CsvASteel>();
                var filtered = csvReader.First(r => r.Type == type);
                Type = type;
                B = filtered.B;
                D = filtered.D;
                R = filtered.R;
                SectionalArea = filtered.SectionalArea;
                TheoreticalWeight = filtered.TheoreticalWeight;
                SurfaceArea = filtered.SurfaceArea;
            }

            private string Type { get; }
            private double B { get; }
            private double D { get; }
            private double R { get; }
            private double SectionalArea { get; }
            private double TheoreticalWeight { get; }
            private double SurfaceArea { get; }

            /// <summary>
            /// 截面面积，m²。CSV 为 mm²，取自 GB/T 706-2026 表 A.4。
            /// 表5：等边角钢面积 = t(2B−t)+0.215(r1²−2r2²)。
            /// 原目录有两格面积与表列不符，但原重量正好等于 0.785×表列面积：
            /// ∠100×14 面积由 22.256 cm² 改为表列 26.26 cm²（重量仍为 20.611 kg/m）；
            /// ∠140×12 面积由 35.512 cm² 改为表列 32.51 cm²（重量仍为 25.522 kg/m）。
            /// 其余型号与表列面积的差在修约范围内，面积和重量未改。
            /// 表 A.4 比原目录多出 ∠250×22 以及边宽 280～360 的型号，已按表补入。
            /// </summary>
            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            /// <summary>
            /// 理论重量，t/m。GB/T 706-2026 密度 7.85 g/cm³。CSV 为 kg/m，再随调用密度缩放。
            /// </summary>
            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            /// <summary>
            /// 每米外表面积，m²/m。直接取 GB/T 706-2026 表 A.4，不再除以 1000。正文没有外表面积公式。
            /// ∠20×3 为 0.078。原式 (4B−2t)/1000 得 0.074；把两条边端加回后的直角全周长 4B/1000 为 0.080。
            /// 表值介于二者之间，含圆角，不含杆件两端端面。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return SurfaceArea;
            }
        }

        private class UaSteel : ISteel
        {
            public UaSteel(string type)
            {
                var csvReader = GetResourceDataReader("UASteel.csv")
                    .GetRecords<CsvUaSteel>();
                var filtered = csvReader.First(r => r.Type == type);
                Type = type;
                B1 = filtered.B1;
                B2 = filtered.B2;
                D = filtered.D;
                R = filtered.R;
                SectionalArea = filtered.SectionalArea;
                TheoreticalWeight = filtered.TheoreticalWeight;
                SurfaceArea = filtered.SurfaceArea;
            }

            private string Type { get; }
            private double B1 { get; }
            private double B2 { get; }
            private double D { get; }
            private double R { get; }
            private double SectionalArea { get; }
            private double TheoreticalWeight { get; }
            private double SurfaceArea { get; }

            /// <summary>
            /// 截面面积，m²。CSV 为 mm²。
            /// GB/T 706-2026 表5：不等边角钢面积 = t(BL+BS−t)+0.215(r1²−2r2²)。
            /// 表 A.5 的面积与原目录一致（修约差在 0.08 cm² 以内），面积和重量未改。
            /// </summary>
            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            /// <summary>
            /// 理论重量，t/m。GB/T 706-2026 密度 7.85 g/cm³。CSV 为 kg/m，再随调用密度缩放。
            /// </summary>
            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            /// <summary>
            /// 每米外表面积，m²/m。直接取 GB/T 706-2026 表 A.5，不再除以 1000。正文没有外表面积公式。
            /// ∠25×16×3 为 0.080。原式 (2BL+2BS−2t)/1000 得 0.076；直角全周长 2(BL+BS)/1000 为 0.082。
            /// 表值含圆角，不含杆件两端端面。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return SurfaceArea;
            }
        }

        private class RtBeam : ISteel
        {
            public RtBeam(double b1, double b2, double t)
            {
                B1 = b1;
                B2 = b2;
                T = t;
            }

            private double B1 { get; }
            private double B2 { get; }
            private double T { get; }

            /// <summary>
            /// 冷弯矩形闭口型钢截面面积，m²。边长与壁厚为 mm，四边共用一个壁厚。
            /// 钢板焊接、腹板厚和翼缘厚可以不同的箱形截面用 <see cref="WeldedBox"/>。
            /// GB/T 6728-2025 附录 A（资料性）表 A.2，适用于表1、表2以外的尺寸：
            /// A = [2t(B+H-2t)-(4-π)(R²-r²)]/100 ，A 为 cm²；r=R-t。
            /// 公称外圆弧半径：t≤6 mm 时 R=2.0t；6 mm&lt;t≤10 mm 时 R=2.5t；t&gt;10 mm 时 R=3.0t。
            /// 第6.1.1.6条表4规定产品实际外圆弧半径是区间（碳素钢 t≤3 mm 为 (1.0～2.5)t，其余档另有上下限），
            /// 附录 A 的单一 R 落在碳素钢区间内，只作为公称参考，不是唯一成品面积。
            /// 原式 2t(B+H-2t) 是 R=0 的直角箱形，不在表4允许的半径范围内。
            /// 当 2R 大于较短边时，公称半径与边长不能同时成立，本式仍按附录 A 的代数式计算。
            /// 圆周率用 Math.PI；镀锌层附录 C.5 另写 π=3.1416，不用于本条。
            /// </summary>
            public double CalcSectionalArea()
            {
                var outerRadius = NominalOuterCornerRadius(T);
                var innerRadius = outerRadius - T;
                var sharpCornerArea = 2.0 * T * (B1 + B2 - 2.0 * T);
                var cornerAdjustment = (4.0 - Math.PI) * (outerRadius * outerRadius - innerRadius * innerRadius);
                return (sharpCornerArea - cornerAdjustment) / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                // 表 A.2：M = 0.785×A（A 为 cm²，得 kg/m），即面积 m² × 7.85 t/m³。调用密度不是 7.85 时按比例缩放。
                return CalcSectionalArea() * _density;
            }

            /// <summary>
            /// 每米外表面积，m²/m。GB/T 6728-2025 表 A.2：
            /// As = 2(H+B-4R+πR)×10³ ，单位 mm²/m，故再除以 10⁶ 得到 m²/m，等价于 2(H+B-4R+πR)/1000。
            /// 只含外侧：四条直线边加一整圈外圆角，不含内表面和杆件端面。R 的取法与面积相同。
            /// </summary>
            public double CalcSurfaceArea()
            {
                var outerRadius = NominalOuterCornerRadius(T);
                return 2.0 * (B1 + B2 - 4.0 * outerRadius + Math.PI * outerRadius) / 1000.0;
            }

            /// <summary>
            /// GB/T 6728-2025 表 A.2 的公称外圆弧半径，mm。
            /// </summary>
            private static double NominalOuterCornerRadius(double thickness)
            {
                if (thickness <= 6.0)
                {
                    return 2.0 * thickness;
                }

                if (thickness <= 10.0)
                {
                    return 2.5 * thickness;
                }

                return 3.0 * thickness;
            }
        }

        /// <summary>
        /// 钢板焊成的箱形截面。外轮廓高 H、宽 B，两块腹板厚度同为 tw，两块翼缘厚度同为 tf。
        /// </summary>
        private class WeldedBox : ISteel
        {
            public WeldedBox(double h, double b, double tw, double tf)
            {
                H = h;
                B = b;
                Tw = tw;
                Tf = tf;
            }

            private double H { get; }
            private double B { get; }
            private double Tw { get; }
            private double Tf { get; }

            /// <summary>
            /// 截面面积，m²。尺寸 mm。
            /// 成品焊接方矩管只有一个公称壁厚：GB/T 6728-2025 附录 A，以及 T/CSCS《建筑结构用方矩管》
            /// 附录 A 表 A.1，都是单一 t 加冷弯外圆弧。那种截面走 <see cref="RtBeam"/>。
            /// GB 50017-2017 表 3.5.1 把箱形截面的腹板厚度和壁板间翼缘厚度分开，但没有截面面积公式。
            /// 这里按四块直角板相加：两块翼缘各为 B×tf，两块腹板各为 (H−2tf)×tw。
            /// 翼缘盖住全宽、腹板夹在翼缘之间，与腹板通高、翼缘夹在腹板之间的面积相同。
            /// 不计焊缝和圆角。200×200×6×9 得 57.84 cm²。
            /// </summary>
            public double CalcSectionalArea()
            {
                return (2.0 * B * Tf + 2.0 * (H - 2.0 * Tf) * Tw) / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            /// <summary>
            /// 每米外表面积，m²/m。外轮廓周长 2(H+B)/1000。
            /// 成品管标准的外表面积含冷弯圆角，不用于这块钢板拼成的直角外皮。
            /// 不含内表面、焊缝和杆件两端。壁厚不改变外轮廓。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return 2.0 * (H + B) / 1000.0;
            }
        }

        private class CtBeam : ISteel
        {
            public CtBeam(double d, double t)
            {
                D = d;
                T = t;
            }

            private double D { get; }
            private double T { get; }

            /// <summary>
            /// 圆形闭口型钢截面面积，m²。外径、壁厚为 mm。
            /// GB/T 6728-2025 附录 A 表 A.1：d=D-2t，A=π(D²-d²)/(4×10²) cm²。
            /// 该式与圆环精确面积 π(D-t)t 相同。M=0.785×A（kg/m）对应密度 7.85 g/cm³。
            /// GB/T 17395-2024《钢管尺寸、外形、重量及允许偏差》已于 2025-04-01 实施，并代替 GB/T 17395-2008 与 GB/T 21835-2008，
            /// 但其重量公式原文本次未取得，圆管面积按已读到的 GB/T 6728-2025 表 A.1。
            /// https://std.samr.gov.cn/gb/search/gbDetailed?id=D6TruQsr%2Fxg%3D
            /// </summary>
            public double CalcSectionalArea()
            {
                return (D - T) * Math.PI * T / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            /// <summary>
            /// 每米外表面积，m²/m。GB/T 6728-2025 表 A.1：As=πD×10³ mm²/m，即 πD/1000。
            /// 只用公称外径，不含内表面和两端。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return D * Math.PI / 1000;
            }
        }

        private class RsSteel : ISteel
        {
            public RsSteel(double d)
            {
                D = d;
            }

            private double D { get; }

            /// <summary>
            /// 圆钢截面面积，m²。直径 mm，按实心圆 π(d/2)²。
            /// 现行标准 GB/T 702-2026《钢棒尺寸、外形、重量及允许偏差》2026-04-30 发布、2026-11-01 实施，
            /// 代替 GB/T 702-2017、GB/T 908-2019、GB/T 905-1994。
            /// 第6.1.1.1条规定热轧圆钢和方钢的理论重量见附录 A 表 A.1。
            /// 表 A.1 中 d=10 mm 的圆钢为 0.617 kg/m，等于 πd²/4×7.85 修约到 0.001 kg/m，不是另一套系数。
            /// 因此继续用精确圆面积，不把表里的修约值逐档抄入。
            /// https://openstd.samr.gov.cn/bzgk/std/newGbInfo?hcno=462E3915DC655BAC6815B86128854E89
            /// </summary>
            public double CalcSectionalArea()
            {
                return Math.Pow(D / 2, 2) * Math.PI / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            /// <summary>
            /// 每米侧表面积，m²/m。圆柱侧面 πd/1000。表 A.1 只列理论重量，没有外表面积。
            /// 不含两端圆面。这是几何周长，不是国标公式。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return D * Math.PI / 1000;
            }
        }

        /// <summary>
        /// 实心正多边形钢棒。尺寸是对边距离，mm。
        /// </summary>
        private class StraightBar : ISteel
        {
            public StraightBar(double size, double areaFactor, double perimeterFactor)
            {
                Size = size;
                AreaFactor = areaFactor;
                PerimeterFactor = perimeterFactor;
            }

            private double Size { get; }
            private double AreaFactor { get; }
            private double PerimeterFactor { get; }

            /// <summary>
            /// 截面面积，m²。GB/T 702-2026 表 A.1、表 A.4 的理论重量按锐角正多边形，没有扣圆角。
            /// 方钢 a=10 mm 为 0.785 kg/m，等于 a²×7.85。
            /// 六角钢对边 8 mm 为 0.5543 cm²、0.435 kg/m，等于 (√3/2)S²。
            /// 八角钢对边 16 mm 为 2.121 cm²、1.66 kg/m，等于 2(√2−1)S²。
            /// </summary>
            public double CalcSectionalArea()
            {
                return AreaFactor * Size * Size / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            /// <summary>
            /// 每米侧表面积，m²/m。表 A.1 和表 A.4 没有这一列。
            /// 按对边距离对应的外侧棱长：方钢 4a，六角钢 2√3 S，八角钢 8(√2−1)S，再除以 1000。
            /// 不含两端，也不扣截面图里的圆角。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return PerimeterFactor * Size / 1000.0;
            }
        }

        private class PlateSteel : ISteel
        {
            public PlateSteel(double b,double d)
            {
                B = b;
                T = d;
            }

            private double B { get; }
            private double T { get; }

            /// <summary>
            /// 钢板或缀板单位长度的截面面积，m²。宽度、厚度为 mm，按矩形 B×t。
            /// GB/T 709-2019《热轧钢板和钢带的尺寸、外形、重量及允许偏差》仍为现行
            /// （2019-03-25 发布，2020-02-01 实施，代替 GB/T 709-2006）。
            /// https://std.samr.gov.cn/gb/search/gbDetailed?id=8531C702DB296551E05397BE0A0ACD16
            /// 官方全文本次未打开。公开转载的第9.2条写明按公称尺寸和碳钢密度 7.85 kg/dm³ 计算理论重量，第9.4条指向表14；
            /// 表14原式未见到。本式与「公称宽度×公称厚度」一致，冷轧板 GB/T 708-2026 要到 2026-12-01 才实施，本次不改用。
            /// </summary>
            public double CalcSectionalArea()
            {
                return B * T / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            /// <summary>
            /// 每米表面积，m²/m。只算上下两大面 2B/1000，不含两条厚度为 t 的纵边，也不含两端。
            /// GB/T 709-2019 未见表面积条款。这是涂装常用的两大面几何量，不是国标公式。
            /// </summary>
            public double CalcSurfaceArea()
            {
                return B * 2 / 1000;
            }
        }


        private struct CsvIBeam
        {
            public string Type { get; set; }
            public double H { get; set; }
            public double B { get; set; }
            public double D { get; set; }
            public double T { get; set; }
            public double R { get; set; }
            public double R1 { get; set; }
            public double SectionalArea { get; set; }
            public double TheoreticalWeight { get; set; }
            public double SurfaceArea { get; set; }
        }

        private struct CsvHxBeam
        {
            public string Type { get; set; }
            public double H { get; set; }
            public double B { get; set; }
            public double T1 { get; set; }
            public double T2 { get; set; }
            public double R { get; set; }
            public double SectionalArea { get; set; }
            public double TheoreticalWeight { get; set; }
        }

        private struct CsvGb11263
        {
            public string Class { get; set; }
            public double H { get; set; }
            public double B { get; set; }
            public double T1 { get; set; }
            public double T2 { get; set; }
            public double R { get; set; }
            public double SectionalArea { get; set; }
            public double TheoreticalWeight { get; set; }
            public double SurfaceArea { get; set; }
        }

        private struct CsvPcSteel
        {
            public string Type { get; set; }
            public double H { get; set; }
            public double B { get; set; }
            public double Tw { get; set; }
            public double Tf { get; set; }
            public double R { get; set; }
            public double SectionalArea { get; set; }
            public double TheoreticalWeight { get; set; }
            public double SurfaceArea { get; set; }
        }

        private struct CsvCSteel
        {
            public string Type { get; set; }
            public double H { get; set; }
            public double B { get; set; }
            public double D { get; set; }
            public double T { get; set; }
            public double R { get; set; }
            public double R1 { get; set; }
            public double SectionalArea { get; set; }
            public double TheoreticalWeight { get; set; }
            public double SurfaceArea { get; set; }
        }

        private struct CsvASteel
        {
            public string Type { get; set; }
            public double B { get; set; }
            public double D { get; set; }
            public double R { get; set; }
            public double SectionalArea { get; set; }
            public double TheoreticalWeight { get; set; }
            public double SurfaceArea { get; set; }
        }

        private struct CsvUaSteel
        {
            public string Type { get; set; }
            public double B1 { get; set; }
            public double B2 { get; set; }
            public double D { get; set; }
            public double R { get; set; }
            public double SectionalArea { get; set; }
            public double TheoreticalWeight { get; set; }
            public double SurfaceArea { get; set; }
        }
    }
}
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
    public static class SteelSection
    {
        private static double _density = 7.85;

        private static (string type, string[] sectional) ParseSteelSection(string input)
        {
            try
            {
                var inputSplit = Regex.Split(input, @"([A-Za-z\u4e00-\u9fa5\[∟]+)([\d\.A-Za-z\*×]+)");
                return (type: inputSplit[1], sectional: inputSplit[2].Split('*', 'x', 'X', '×'));
            }
            catch (IndexOutOfRangeException)
            {
                return (type: null, sectional: null);
            }
        }

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
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
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

            public double CalcSectionalArea()
            {
                return ((H - (T1 + T2)) * Tw + B1 * T1 + B2 * T2) / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            public double CalcSurfaceArea()
            {
                return (H * 2 + B1 * 2 + B2 * 2 - T1 - T2) / 1000;
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

            public double CalcSectionalArea()
            {
                return (H + B * 2 + C * 2 - T * 4) * T / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            public double CalcSurfaceArea()
            {
                return ((H + B * 2 + C * 2 - T * 2 * 4) * 2 + 1 / 2d * Math.PI * T * 4 + T * 4) / 1000;
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

            public double CalcSectionalArea()
            {
                return (H + B * 2 + C * 2 - T * 2 - T * Math.PI * 45 / 180) * T / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            public double CalcSurfaceArea()
            {
                return ((H + B * 2 + C * 2 - T * 2 * 4 + 1 / 2d * Math.PI * T * 2 + T * Math.PI * 45 / 180 * 2) *
                    2 + T * 2) / 1000;
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

            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            public double CalcSurfaceArea()
            {
                return (H * 2 + B * 4) / 1000;
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
                T1 = filtered.T1;
                T2 = filtered.T1;
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

            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            public double CalcSurfaceArea()
            {
                return (H * 2 + B * 4 - T1 * 2) / 1000;
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

            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            public double CalcSurfaceArea()
            {
                return (H * 2 + B * 4 - D * 2 - T * 2) / 1000;
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
            }

            private string Type { get; }
            private double B { get; }
            private double D { get; }
            private double R { get; }
            private double SectionalArea { get; }
            private double TheoreticalWeight { get; }

            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            public double CalcSurfaceArea()
            {
                return (B * 4 - D * 2) / 1000;
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
            }

            private string Type { get; }
            private double B1 { get; }
            private double B2 { get; }
            private double D { get; }
            private double R { get; }
            private double SectionalArea { get; }
            private double TheoreticalWeight { get; }

            public double CalcSectionalArea()
            {
                return SectionalArea / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return TheoreticalWeight / 1000 / 7.85 * _density;
            }

            public double CalcSurfaceArea()
            {
                return (B1 * 2 + B2 * 2 - D * 2) / 1000;
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

            public double CalcSectionalArea()
            {
                return (B1 + B2 - T * 2) * 2 * T / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            public double CalcSurfaceArea()
            {
                return (B1 + B2) * 2 / 1000;
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

            public double CalcSectionalArea()
            {
                return (D - T) * Math.PI * T / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

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

            public double CalcSectionalArea()
            {
                return Math.Pow(D / 2, 2) * Math.PI / Math.Pow(1000, 2);
            }

            public double CalcTheoreticalWeight()
            {
                return CalcSectionalArea() * _density;
            }

            public double CalcSurfaceArea()
            {
                return D * Math.PI / 1000;
            }
        }


        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class CsvIBeam
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
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class CsvHxBeam
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

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class CsvCSteel
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
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class CsvASteel
        {
            public string Type { get; set; }
            public double B { get; set; }
            public double D { get; set; }
            public double R { get; set; }
            public double SectionalArea { get; set; }
            public double TheoreticalWeight { get; set; }
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class CsvUaSteel
        {
            public string Type { get; set; }
            public double B1 { get; set; }
            public double B2 { get; set; }
            public double D { get; set; }
            public double R { get; set; }
            public double SectionalArea { get; set; }
            public double TheoreticalWeight { get; set; }
        }
    }
}
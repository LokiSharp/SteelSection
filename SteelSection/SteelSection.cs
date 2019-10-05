using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;

namespace SteelSection
{
    public static class SteelSection
    {
        private const double Density = 7.85;


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

        public static double[] CalcSteelSection(string input, double density = Density)
        {
            var (type, sectional) = ParseSteelSection(input);

            switch (type)
            {
                case "H":
                case "H型钢":
                    return CalcHBeam(sectional, density);
                case "HW":
                case "HM":
                case "HN":
                case "HT":
                    return CalcHxBeam(type, sectional, density);
                case "C":
                case "C型钢":
                    return CalcCBeam(sectional, density);
                case "Z":
                case "Z型钢":
                    return CalcZBeam(sectional, density);
                case "I":
                case "UB":
                case "工字钢":
                    return CalcIBeam(sectional, density);
                case "[":
                case "CS":
                case "槽钢":
                    return CalcCSteel(sectional, density);
                case "∟":
                case "A":
                case "UA":
                case "角钢":
                    return CalcASteel(sectional, density);
                default:
                    return null;
            }
        }

        private static double[] CalcHBeam(string[] sectional, double density)
        {
            if (sectional.Length != 4) throw new Exception("Not Match ");
            var s = Array.ConvertAll(sectional, double.Parse);

            var h = s[0];
            var b = s[1];
            var tw = s[2];
            var t = s[3];

            var sectionalArea = ((h - t * 2) * tw + b * t * 2) / Math.Pow(1000, 2);
            var theoreticalWeight = sectionalArea * density;
            var surfaceArea = (h * 2 + b * 4 - t * 2) / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};

            return results;
        }

        private static double[] CalcCBeam(string[] sectional, double density)
        {
            if (sectional.Length != 4) throw new Exception("Not Match ");
            var s = Array.ConvertAll(sectional, double.Parse);

            var h = s[0];
            var b = s[1];
            var c = s[2];
            var t = s[3];

            var sectionalArea = (h + b * 2 + c * 2 - t * 4) * t / Math.Pow(1000, 2);
            var theoreticalWeight = sectionalArea * density;
            var surfaceArea = ((h + b * 2 + c * 2 - t * 2 * 4 + 1 / 2d * 3.14 * t * 4) * 2 + t * 2) / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};

            return results;
        }

        private static double[] CalcZBeam(string[] sectional, double density)
        {
            if (sectional.Length != 4) throw new Exception("Not Match ");
            var s = Array.ConvertAll(sectional, double.Parse);

            var h = s[0];
            var b = s[1];
            var c = s[2];
            var t = s[3];

            var sectionalArea = (h + b * 2 + c * 2 - t * 2 - t * 3.14 * 45 / 180) * t / Math.Pow(1000, 2);
            var theoreticalWeight = sectionalArea * density;
            var surfaceArea = ((h + b * 2 + c * 2 - t * 2 * 4 + 1 / 2d * 3.14 * t * 2 + t * 3.14 * 45 / 180 * 2) * 2 +
                               t * 2) / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};

            return results;
        }

        private static double[] CalcIBeam(IReadOnlyList<string> type, double density)
        {
            if (type.Count != 1) throw new Exception("Not Match ");
            var csvReader = GetResourceDataReader("SteelSection.Resources.data.IBeam.csv").GetRecords<CsvIBeam>();
            var filtered = csvReader.First(r => r.Type == type[0]);
            var sectionalArea = filtered.SectionalArea / 1000 / 1000;
            var theoreticalWeight = filtered.TheoreticalWeight / 1000 / 7.85 * density;
            double[] results = {sectionalArea, theoreticalWeight, 0.0};
            return results;
        }

        private static double[] CalcHxBeam(string type, IReadOnlyList<string> sectional, double density)
        {
            if (sectional.Count != 2) throw new Exception("Not Match ");
            var csvReader = GetResourceDataReader("SteelSection.Resources.data.HXBeam.csv").GetRecords<CsvHxBeam>();
            var filtered = csvReader.First(r => r.Type == $"{type}{sectional[0]}*{sectional[1]}");
            var sectionalArea = filtered.SectionalArea / 1000 / 1000;
            var theoreticalWeight = filtered.TheoreticalWeight / 1000 / 7.85 * density;
            var surfaceArea = (filtered.H * 2 + filtered.B * 4 - filtered.T1 * 2) / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};
            return results;
        }

        private static double[] CalcCSteel(IReadOnlyList<string> type, double density)
        {
            if (type.Count != 1) throw new Exception("Not Match ");
            var csvReader = GetResourceDataReader("SteelSection.Resources.data.CSteel.csv").GetRecords<CsvCSteel>();
            var filtered = csvReader.First(r => r.Type == type[0]);
            var sectionalArea = filtered.SectionalArea / 1000 / 1000;
            var theoreticalWeight = filtered.TheoreticalWeight / 1000 / 7.85 * density;
            double[] results = {sectionalArea, theoreticalWeight, 0.0};
            return results;
        }

        private static double[] CalcASteel(IReadOnlyList<string> sectional, double density)
        {
            switch (sectional.Count)
            {
                case 2:
                {
                    var csvReader = GetResourceDataReader("SteelSection.Resources.data.ASteel.csv")
                        .GetRecords<CsvASteel>();
                    var filtered = csvReader.First(r => r.Type == $"{sectional[0]}*{sectional[1]}");
                    var sectionalArea = filtered.SectionalArea / 1000 / 1000;
                    var theoreticalWeight = filtered.TheoreticalWeight / 1000 / 7.85 * density;
                    double[] results = {sectionalArea, theoreticalWeight, 0.0};
                    return results;
                }
                case 3:
                {
                    var csvReader =
                        GetResourceDataReader("SteelSection.Resources.data.UASteel.csv").GetRecords<CsvUaSteel>();
                    var filtered = csvReader.First(r => r.Type == $"{sectional[0]}*{sectional[1]}*{sectional[2]}");
                    var sectionalArea = filtered.SectionalArea / 1000 / 1000;
                    var theoreticalWeight = filtered.TheoreticalWeight / 1000 / 7.85 * density;
                    double[] results = {sectionalArea, theoreticalWeight, 0.0};
                    return results;
                }
                default:
                    throw new Exception("Not Match ");
            }
        }

        private static CsvReader GetResourceDataReader(string filename)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var fs = thisAssembly.GetManifestResourceStream(filename);
            var sr = new StreamReader(fs ?? throw new InvalidOperationException(), Encoding.Default);

            return new CsvReader(sr);
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
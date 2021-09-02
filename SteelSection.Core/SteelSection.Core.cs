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
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    return CalcHBeam(s[0], s[1], s[1], s[2], s[3], s[3]);
                }
                case "HW":
                case "HM":
                case "HN":
                case "HT":
                    return CalcHxBeam(type);
                case "C":
                case "C型钢":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    return CalcCBeam(s[0], s[1], s[2], s[3]);
                }
                case "Z":
                case "Z型钢":
                {
                    var s = Array.ConvertAll(sectional, double.Parse);
                    return CalcZBeam(s[0], s[1], s[2], s[3]);
                }
                case "I":
                case "UB":
                case "工字钢":
                    return CalcIBeam(type);
                case "[":
                case "CS":
                case "槽钢":
                    return CalcCSteel(type);
                case "∟":
                case "A":
                case "UA":
                case "角钢":
                    return CalcASteel(type);
                default:
                    return null;
            }
        }

        private static double[] CalcHBeam(double h, double b1, double b2, double tw, double t1, double t2)
        {
            var sectionalArea = ((h - (t1 + t2)) * tw + b1 * t1 + b2 * t2) / Math.Pow(1000, 2);
            var theoreticalWeight = sectionalArea * Density;
            var surfaceArea = (h * 2 + b1 * 2 + b2 * 2 - t1 - t2) / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};

            return results;
        }

        private static double[] CalcCBeam(double h, double b, double c, double t)
        {
            var sectionalArea = (h + b * 2 + c * 2 - t * 4) * t / Math.Pow(1000, 2);
            var theoreticalWeight = sectionalArea * Density;
            var surfaceArea = ((h + b * 2 + c * 2 - t * 2 * 4) * 2 + 1 / 2d * 3.14 * t * 4 + t * 4) / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};

            return results;
        }

        private static double[] CalcZBeam(double h, double b, double c, double t)
        {
            var sectionalArea = (h + b * 2 + c * 2 - t * 2 - t * 3.14 * 45 / 180) * t / Math.Pow(1000, 2);
            var theoreticalWeight = sectionalArea * Density;
            var surfaceArea = ((h + b * 2 + c * 2 - t * 2 * 4 + 1 / 2d * 3.14 * t * 2 + t * 3.14 * 45 / 180 * 2) * 2 +
                               t * 2) / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};

            return results;
        }

        private static double[] CalcIBeam(string type)
        {
            var csvReader = GetResourceDataReader("SteelSection.Resources.data.IBeam.csv").GetRecords<CsvIBeam>();
            var filtered = csvReader.First(r => r.Type == type);
            var sectionalArea = filtered.SectionalArea / 1000 / 1000;
            var theoreticalWeight = filtered.TheoreticalWeight / 1000 / 7.85 * Density;
            var surfaceArea = (filtered.H * 2 + filtered.B * 4) / 1000 / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};
            return results;
        }

        private static double[] CalcHxBeam(string type)
        {
            var csvReader = GetResourceDataReader("SteelSection.Resources.data.HXBeam.csv").GetRecords<CsvHxBeam>();
            var filtered = csvReader.First(r => r.Type == type);
            var sectionalArea = filtered.SectionalArea / 1000 / 1000;
            var theoreticalWeight = filtered.TheoreticalWeight / 1000 / 7.85 * Density;
            var surfaceArea = (filtered.H * 2 + filtered.B * 4 - filtered.T1 * 2) / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};
            return results;
        }

        private static double[] CalcCSteel(string type)
        {
            var csvReader = GetResourceDataReader("SteelSection.Resources.data.CSteel.csv").GetRecords<CsvCSteel>();
            var filtered = csvReader.First(r => r.Type == type);
            var sectionalArea = filtered.SectionalArea / 1000 / 1000;
            var theoreticalWeight = filtered.TheoreticalWeight / 1000 / 7.85 * Density;
            var surfaceArea = (filtered.H * 2 + filtered.B * 4 - filtered.D * 2 - filtered.T * 2) / 1000 / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea };
            return results;
        }

        private static double[] CalcASteel(string type)
        {
          
            var csvReader = GetResourceDataReader("SteelSection.Resources.data.ASteel.csv")
                .GetRecords<CsvASteel>();
            var filtered = csvReader.First(r => r.Type == type);
            var sectionalArea = filtered.SectionalArea / 1000 / 1000;
            var theoreticalWeight = filtered.TheoreticalWeight / 1000 / 7.85 * Density;
            var surfaceArea = (filtered.B * 4 - filtered.D * 2) / 1000 / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};
            return results;
          
        }

        private static CsvReader GetResourceDataReader(string filename)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var fs = thisAssembly.GetManifestResourceStream(filename);
            var sr = new StreamReader(fs ?? throw new InvalidOperationException(), Encoding.Default);

            return new CsvReader(sr, CultureInfo.InvariantCulture);
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
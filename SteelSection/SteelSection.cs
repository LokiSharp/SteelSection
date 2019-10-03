using System;
using System.Collections.Generic;
using System.Data;
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
        private static readonly IEnumerable<CsvIBeam> csvIBeam = ReadResourceDataFileStream("SteelSection.data.IBeam.csv").GetRecords<CsvIBeam>();

        private static (string type, string[] sectional) ParseSteelSection(string input)
        {
            var inputSplit = Regex.Split(input, @"([A-Za-z\u4e00-\u9fa5\[]+)([\d\.A-Za-z\*]+)");

            return (type: inputSplit[1], sectional: inputSplit[2].Split('*', 'x', 'X'));
        }

        public static double[] CalcSteelSection(string input, double density = Density)
        {
            double[] results = {0, 0, 0};
            if (input.Length == 0) return results;

            var (type, sectional) = ParseSteelSection(input);

            switch (type)
            {
                case "H":
                case "H型钢":
                    results = CalcHBeam(sectional, density);
                    break;
                case "C":
                case "C型钢":
                    results = CalcCBeam(sectional, density);
                    break;
                case "Z":
                case "Z型钢":
                    results = CalcZBeam(sectional, density);
                    break;
                case "I":
                case "工字钢":
                    results = CalcIBeam(sectional, density);
                    break;
            }

            return results;
        }

        private static double[] CalcHBeam(string[] sectional, double density)
        {
            if (sectional.Length != 4) throw new Exception("Not Match != 4 ");
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
            if (sectional.Length != 4) throw new Exception("Not Match != 4 ");
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
            if (sectional.Length != 4) throw new Exception("Not Match != 4 ");
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
            if (type.Count != 1) throw new Exception("Not Match != 1 ");
            var filtered = csvIBeam.First(r => r.Type == type[0]);
            var sectionalArea = filtered.SectionalArea / 7.85 * density;
            var theoreticalWeight = filtered.TheoreticalWeight;
            double[] results = {sectionalArea, theoreticalWeight, 0.0};
            return results;
        }

        private static CsvReader ReadResourceDataFileStream(string filename)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var fs = thisAssembly.GetManifestResourceStream(filename);
            var sr = new StreamReader(fs ?? throw new InvalidOperationException(), Encoding.Default);
            
            return new CsvReader(sr);
            
        }

        private class CsvIBeam
        {
            public string Type { get; set;}
            public double h { get; set;}
            public double b { get; set;}
            public double d { get; set;}
            public double t { get; set;}
            public double r { get; set;}
            public double r1 { get; set;}
            public double SectionalArea { get; set;}
            public double TheoreticalWeight { get; set;}
            public double Ix { get; set;}
            public double Iy { get; set;}
            public double ix { get; set;}
            public double iy { get; set;}
            public double Wx { get; set;}
            public double Wy { get; set;}
        }

        private static void SaveStreamToFile(string fileFullPath, Stream stream)
        {
            if (stream.Length == 0) return;

            // Create a FileStream object to write a stream to a file
            using (var fileStream = File.Create(fileFullPath, (int) stream.Length))
            {
                // Fill the bytes[] array with the stream data
                var bytesInStream = new byte[stream.Length];
                stream.Read(bytesInStream, 0, bytesInStream.Length);

                // Use FileStream object to write to the specified file
                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
            }
        }
    }
}
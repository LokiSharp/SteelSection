using System;

namespace SteelSection
{
    public static class SteelSection
    {
        private const double Density = 7.85;

        private static string[] ReadSteelSection(string input)
        {
            var s = input.Split('H', 'C', 'Z', 'I', '[')[1].Split('*', 'x', 'X');
            return s;
        }

        public static double[] CalcSteelSection(string input)
        {
            var results = new double[3];
            if (input[0] == 'H') results = CalcHBeam(input);
            else if (input[0] == 'C') results = CalcCBeam(input);

            return results;
        }

        private static double[] CalcHBeam(string input)
        {
            var hBeam = new HBeam(input);

            var h = hBeam.H;
            var b = hBeam.B;
            var tw = hBeam.Tw;
            var t = hBeam.T;

            var sectionalArea = ((h - t * 2) * tw + b * t * 2) / Math.Pow(1000, 2);
            var theoreticalWeight = sectionalArea * Density;
            var surfaceArea = (h * 2 + b * 4 - t * 2) / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};

            return results;
        }

        private static double[] CalcCBeam(string input)
        {
            var cBeam = new CBeam(input);

            var h = cBeam.H;
            var b = cBeam.B;
            var c = cBeam.C;
            var t = cBeam.T;

            var sectionalArea = (h + b * 2 + c * 2 - t * 4) * t / Math.Pow(1000, 2);
            var theoreticalWeight = sectionalArea * Density;
            var surfaceArea = ((h + b * 2 + c * 2 + 1 / 2d * 3.14 * t * 4) * 2 + t * 2) / 1000;
            double[] results = {sectionalArea, theoreticalWeight, surfaceArea};

            return results;
        }

        private struct HBeam
        {
            public readonly double H, B, Tw, T;

            public HBeam(string input)
            {
                var r = ReadSteelSection(input);
                if (r.Length != 4) throw new Exception("Not Match != 4 " + input);
                var s = Array.ConvertAll(r, double.Parse);
                H = s[0];
                B = s[1];
                Tw = s[2];
                T = s[3];
            }
        }

        private struct CBeam
        {
            public readonly double H, B, C, T;

            public CBeam(string input)
            {
                var r = ReadSteelSection(input);
                if (r.Length != 4) throw new Exception("Not Match != 4 " + input);
                var s = Array.ConvertAll(r, double.Parse);
                H = s[0];
                B = s[1];
                C = s[2];
                T = s[3];
            }
        }
    }
}
using System.Linq;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Tools.Ribbon;
using static SteelSection.Core.SteelSection;

namespace SteelSection
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class SteelSectionRibbon
    {
        private void SteelSectionRibbon_Load(object sender, RibbonUIEventArgs e)
        {
        }

        private void CalcSteelSectionButton_Click(object sender, RibbonControlEventArgs e)
        {
            const int nullLimit = 100;
            Range range = Globals.ThisAddIn.Application.Selection;
            var globalLimit = int.Parse(LimitEditBox.Text);

            var globalCounter = 0;
            var nullCounter = 0;
            foreach (var cell in range.Cast<Range>()
                .TakeWhile(_ => globalCounter < globalLimit && nullCounter < nullLimit))
            {
                var density = double.Parse(DensityEditBox.Text);
                var offset = int.Parse(CalcSteelSectionOffsetEditBox.Text);
                (double, double, double) result = CalcSteelSection(cell.Text, density);
                var (sectionalArea, theoreticalWeight, surfaceArea) = result;
                globalCounter++;
                if (Equals(sectionalArea, 0.0))
                {
                    nullCounter++;
                    continue;
                }

                var offsetCounter = 0;
                if (SectionalAreaCheckBox.Checked)
                {
                    offsetCounter++;
                    cell.Offset[0, offset + offsetCounter].Value = sectionalArea;
                }

                if (TheoreticalWeightCheckBox.Checked)
                {
                    offsetCounter++;
                    cell.Offset[0, offset + offsetCounter].Value = theoreticalWeight;
                }

                if (SurfaceAreaCheckBox.Checked)
                {
                    offsetCounter++;
                    cell.Offset[0, offset + offsetCounter].Value = surfaceArea;
                }
            }
        }
    }
}
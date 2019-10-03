using System.Linq;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Tools.Ribbon;
using static SteelSection.SteelSection;

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
            var i = 0;
            Range range = Globals.ThisAddIn.Application.Selection;
            var limit = int.Parse(LimitEditBox.Text);
            foreach (var cell in range.Cast<Range>().TakeWhile(cell => i < limit))
            {
                var density = double.Parse(DensityEditBox.Text);
                var offset = int.Parse(CalcSteelSectionOffsetEditBox.Text);
                var result = CalcSteelSection(cell.Text, density);
                var j = 0;
                if (SectionalAreaCheckBox.Checked)
                {
                    j++;
                    cell.Offset[0, offset + j].Value = result[0];
                }

                if (TheoreticalWeightCheckBox.Checked)
                {
                    j++;
                    cell.Offset[0, offset + j].Value = result[1];
                }

                if (SurfaceAreaCheckBox.Checked)
                {
                    j++;
                    cell.Offset[0, offset + j].Value = result[2];
                }

                i++;
            }
        }
    }
}
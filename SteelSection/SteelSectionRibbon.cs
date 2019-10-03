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
            int i = 0;
            Range range = Globals.ThisAddIn.Application.Selection;
            foreach (Range cell in range)
            {
                if (i >= 5000) break;
                var density = double.Parse(DensityEditBox.Text);
                var offset = int.Parse(CalcSteelSectionOffsetEditBox.Text);
                var result = CalcSteelSection(cell.Text, density);
                var j = 0;
                bool[] checks =
                    {SectionalAreaCheckBox.Checked, TheoreticalWeightCheckBox.Checked, SurfaceAreaCheckBox.Checked};
                foreach (var check in checks)
                    if (check)
                    {
                        cell.Offset[0, offset + j + 1].Value2 = result[j];
                        j += 1;
                    }
                i += 1;
            }
        }
    }
}
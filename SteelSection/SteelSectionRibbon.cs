using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Tools.Ribbon;
using static SteelSection.SteelSection;

namespace SteelSection
{
    public partial class SteelSectionRibbon
    {
        private void SteelSectionRibbon_Load(object sender, RibbonUIEventArgs e)
        {
        }

        private void CalcSteelSectionButton_Click(object sender, RibbonControlEventArgs e)
        {
            var i = 0;
            Range range = Globals.ThisAddIn.Application.Selection;
            foreach (Range cell in range)
            {
                var density = double.Parse(DensityEditBox.Text);
                var offset = int.Parse(CalcSteelSectionOffsetEditBox.Text);
                var result = CalcSteelSection(cell.Text, density);
                i += 1;

                cell.Offset[0, offset + 1].Value2 = result[0];
                cell.Offset[0, offset + 2].Value2 = result[1];
                cell.Offset[0, offset + 3].Value2 = result[2];
            }
        }
    }
}
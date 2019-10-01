using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using static SteelSection.SteelSection;

// TODO:  Follow these steps to enable the Ribbon (XML) item:

// 1: Copy the following code block into the ThisAddin, ThisWorkbook, or ThisDocument class.

//  protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
//  {
//      return new MyRibbon();
//  }

// 2. Create callback methods in the "Ribbon Callbacks" region of this class to handle user
//    actions, such as clicking a button. Note: if you have exported this Ribbon from the Ribbon designer,
//    move your code from the event handlers to the callback methods and modify the code to work with the
//    Ribbon extensibility (RibbonX) programming model.

// 3. Assign attributes to the control tags in the Ribbon XML file to identify the appropriate callback methods in your code.  

// For more information, see the Ribbon XML documentation in the Visual Studio Tools for Office Help.


namespace SteelSection
{
    [ComVisible(true)]
    public class MyRibbon : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI ribbon;

        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("SteelSection.MyRibbon.xml");
        }

        #endregion


        public void OnCalcSteelSectionButton(Office.IRibbonControl control)
        {
            var i = 0;
            Range range = Globals.ThisAddIn.Application.Selection;
            foreach (Range cell in range)
            {
                var offset = 0;
                var result = CalcSteelSection(cell.Text);
                i += 1;

                cell.Offset[0, offset + 1].Value2 = result[0];
                cell.Offset[0, offset + 2].Value2 = result[1];
                cell.Offset[0, offset + 3].Value2 = result[2];
            }
        }

        #region Ribbon Callbacks

        //Create callback methods here. For more information about adding callback methods, visit https://go.microsoft.com/fwlink/?LinkID=271226

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            ribbon = ribbonUI;
        }

        #endregion

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            var resourceNames = asm.GetManifestResourceNames();
            for (var i = 0; i < resourceNames.Length; ++i)
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                    using (var resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null) return resourceReader.ReadToEnd();
                    }

            return null;
        }

        #endregion
    }
}
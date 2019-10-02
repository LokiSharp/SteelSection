namespace SteelSection
{
    partial class SteelSectionRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public SteelSectionRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SteelSectionTab = this.Factory.CreateRibbonTab();
            this.SteelSectionGroup = this.Factory.CreateRibbonGroup();
            this.CalcSteelSectionOffsetEditBox = this.Factory.CreateRibbonEditBox();
            this.DensityEditBox = this.Factory.CreateRibbonEditBox();
            this.CalcSteelSectionButton = this.Factory.CreateRibbonButton();
            this.SteelSectionTab.SuspendLayout();
            this.SteelSectionGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // SteelSectionTab
            // 
            this.SteelSectionTab.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.SteelSectionTab.Groups.Add(this.SteelSectionGroup);
            this.SteelSectionTab.Label = "Steel Section";
            this.SteelSectionTab.Name = "SteelSectionTab";
            // 
            // SteelSectionGroup
            // 
            this.SteelSectionGroup.Items.Add(this.CalcSteelSectionOffsetEditBox);
            this.SteelSectionGroup.Items.Add(this.DensityEditBox);
            this.SteelSectionGroup.Items.Add(this.CalcSteelSectionButton);
            this.SteelSectionGroup.Label = "Steel Section";
            this.SteelSectionGroup.Name = "SteelSectionGroup";
            // 
            // CalcSteelSectionOffsetEditBox
            // 
            this.CalcSteelSectionOffsetEditBox.Label = "Offset";
            this.CalcSteelSectionOffsetEditBox.MaxLength = 1;
            this.CalcSteelSectionOffsetEditBox.Name = "CalcSteelSectionOffsetEditBox";
            this.CalcSteelSectionOffsetEditBox.Text = "0";
            // 
            // DensityEditBox
            // 
            this.DensityEditBox.Label = "Density";
            this.DensityEditBox.Name = "DensityEditBox";
            this.DensityEditBox.Text = "7.85";
            // 
            // CalcSteelSectionButton
            // 
            this.CalcSteelSectionButton.Label = "Calc Steel Section";
            this.CalcSteelSectionButton.Name = "CalcSteelSectionButton";
            this.CalcSteelSectionButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.CalcSteelSectionButton_Click);
            // 
            // SteelSectionRibbon
            // 
            this.Name = "SteelSectionRibbon";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.SteelSectionTab);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.SteelSectionRibbon_Load);
            this.SteelSectionTab.ResumeLayout(false);
            this.SteelSectionTab.PerformLayout();
            this.SteelSectionGroup.ResumeLayout(false);
            this.SteelSectionGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab SteelSectionTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup SteelSectionGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox CalcSteelSectionOffsetEditBox;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton CalcSteelSectionButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox DensityEditBox;
    }

    partial class ThisRibbonCollection
    {
        internal SteelSectionRibbon SteelSectionRibbon
        {
            get { return this.GetRibbon<SteelSectionRibbon>(); }
        }
    }
}

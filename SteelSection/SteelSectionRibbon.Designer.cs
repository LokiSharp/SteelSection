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
            this.LimitEditBox = this.Factory.CreateRibbonEditBox();
            this.SectionalAreaCheckBox = this.Factory.CreateRibbonCheckBox();
            this.TheoreticalWeightCheckBox = this.Factory.CreateRibbonCheckBox();
            this.SurfaceAreaCheckBox = this.Factory.CreateRibbonCheckBox();
            this.CalcSteelSectionButton = this.Factory.CreateRibbonButton();
            this.SteelSectionTab.SuspendLayout();
            this.SteelSectionGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // SteelSectionTab
            // 
            this.SteelSectionTab.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.SteelSectionTab.Groups.Add(this.SteelSectionGroup);
            this.SteelSectionTab.Label = global::SteelSection.Properties.Resources.Steel_Section;
            this.SteelSectionTab.Name = "SteelSectionTab";
            // 
            // SteelSectionGroup
            // 
            this.SteelSectionGroup.Items.Add(this.CalcSteelSectionOffsetEditBox);
            this.SteelSectionGroup.Items.Add(this.DensityEditBox);
            this.SteelSectionGroup.Items.Add(this.LimitEditBox);
            this.SteelSectionGroup.Items.Add(this.SectionalAreaCheckBox);
            this.SteelSectionGroup.Items.Add(this.TheoreticalWeightCheckBox);
            this.SteelSectionGroup.Items.Add(this.SurfaceAreaCheckBox);
            this.SteelSectionGroup.Items.Add(this.CalcSteelSectionButton);
            this.SteelSectionGroup.Label = global::SteelSection.Properties.Resources.Steel_Section;
            this.SteelSectionGroup.Name = "SteelSectionGroup";
            // 
            // CalcSteelSectionOffsetEditBox
            // 
            this.CalcSteelSectionOffsetEditBox.Label = global::SteelSection.Properties.Resources.Offset;
            this.CalcSteelSectionOffsetEditBox.MaxLength = 1;
            this.CalcSteelSectionOffsetEditBox.Name = "CalcSteelSectionOffsetEditBox";
            this.CalcSteelSectionOffsetEditBox.SizeString = "1.00000";
            this.CalcSteelSectionOffsetEditBox.Text = "0";
            // 
            // DensityEditBox
            // 
            this.DensityEditBox.Label = global::SteelSection.Properties.Resources.Density;
            this.DensityEditBox.MaxLength = 7;
            this.DensityEditBox.Name = "DensityEditBox";
            this.DensityEditBox.SizeString = "1.00000";
            this.DensityEditBox.Text = "7.85";
            // 
            // LimitEditBox
            // 
            this.LimitEditBox.Label = global::SteelSection.Properties.Resources.Limit;
            this.LimitEditBox.MaxLength = 7;
            this.LimitEditBox.Name = "LimitEditBox";
            this.LimitEditBox.SizeString = "1.00000";
            this.LimitEditBox.Text = "100";
            // 
            // SectionalAreaCheckBox
            // 
            this.SectionalAreaCheckBox.Checked = true;
            this.SectionalAreaCheckBox.Label = global::SteelSection.Properties.Resources.Sectional_Area;
            this.SectionalAreaCheckBox.Name = "SectionalAreaCheckBox";
            // 
            // TheoreticalWeightCheckBox
            // 
            this.TheoreticalWeightCheckBox.Checked = true;
            this.TheoreticalWeightCheckBox.Label = global::SteelSection.Properties.Resources.Theoretical_Weight;
            this.TheoreticalWeightCheckBox.Name = "TheoreticalWeightCheckBox";
            // 
            // SurfaceAreaCheckBox
            // 
            this.SurfaceAreaCheckBox.Checked = true;
            this.SurfaceAreaCheckBox.Label = global::SteelSection.Properties.Resources.Surface_Area;
            this.SurfaceAreaCheckBox.Name = "SurfaceAreaCheckBox";
            // 
            // CalcSteelSectionButton
            // 
            this.CalcSteelSectionButton.Label = global::SteelSection.Properties.Resources.Calc_Steel_Section;
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
        internal Microsoft.Office.Tools.Ribbon.RibbonCheckBox SectionalAreaCheckBox;
        internal Microsoft.Office.Tools.Ribbon.RibbonCheckBox TheoreticalWeightCheckBox;
        internal Microsoft.Office.Tools.Ribbon.RibbonCheckBox SurfaceAreaCheckBox;
        internal Microsoft.Office.Tools.Ribbon.RibbonEditBox LimitEditBox;
    }

    partial class ThisRibbonCollection
    {
        internal SteelSectionRibbon SteelSectionRibbon
        {
            get { return this.GetRibbon<SteelSectionRibbon>(); }
        }
    }
}

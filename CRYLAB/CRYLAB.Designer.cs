namespace CRYLAB
{
    partial class CRYLAB
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CRYLAB));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.baseStructureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.childStructuresToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.structureMatFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calculateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.childListBox = new System.Windows.Forms.ListBox();
            this.baseStructureTextBox = new System.Windows.Forms.TextBox();
            this.childStructuresLabel = new System.Windows.Forms.Label();
            this.parentStructureLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.removeSelectedButton = new System.Windows.Forms.Button();
            this.browseChildrenButton = new System.Windows.Forms.Button();
            this.browseParentButton = new System.Windows.Forms.Button();
            this.plotSelectedButton = new System.Windows.Forms.Button();
            this.plotParentButton = new System.Windows.Forms.Button();
            this.plotNextButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.plotStyleGroup = new System.Windows.Forms.GroupBox();
            this.curlsRadio = new System.Windows.Forms.RadioButton();
            this.moleculesRadio = new System.Windows.Forms.RadioButton();
            this.vectorsRadio = new System.Windows.Forms.RadioButton();
            this.centroidsRadio = new System.Windows.Forms.RadioButton();
            this.cloneButton = new System.Windows.Forms.Button();
            this.menuStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.plotStyleGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.calculateToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(584, 24);
            this.menuStrip.TabIndex = 2;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.baseStructureToolStripMenuItem,
            this.childStructuresToolStripMenuItem,
            this.toolStripSeparator1,
            this.structureMatFileToolStripMenuItem});
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.loadToolStripMenuItem.Text = "Load";
            // 
            // baseStructureToolStripMenuItem
            // 
            this.baseStructureToolStripMenuItem.Name = "baseStructureToolStripMenuItem";
            this.baseStructureToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.baseStructureToolStripMenuItem.Text = "Base Structure";
            this.baseStructureToolStripMenuItem.Click += new System.EventHandler(this.baseStructureToolStripMenuItem_Click);
            // 
            // childStructuresToolStripMenuItem
            // 
            this.childStructuresToolStripMenuItem.Name = "childStructuresToolStripMenuItem";
            this.childStructuresToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.childStructuresToolStripMenuItem.Text = "Child Structures";
            this.childStructuresToolStripMenuItem.Click += new System.EventHandler(this.childStructuresToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(162, 6);
            // 
            // structureMatFileToolStripMenuItem
            // 
            this.structureMatFileToolStripMenuItem.Name = "structureMatFileToolStripMenuItem";
            this.structureMatFileToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.structureMatFileToolStripMenuItem.Text = "Structure mat file";
            this.structureMatFileToolStripMenuItem.Click += new System.EventHandler(this.structureMatFileToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorsToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // colorsToolStripMenuItem
            // 
            this.colorsToolStripMenuItem.Name = "colorsToolStripMenuItem";
            this.colorsToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.colorsToolStripMenuItem.Text = "Colors";
            // 
            // calculateToolStripMenuItem
            // 
            this.calculateToolStripMenuItem.Name = "calculateToolStripMenuItem";
            this.calculateToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.calculateToolStripMenuItem.Text = "Calculate";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Mol2 Files|*.mol2";
            // 
            // childListBox
            // 
            this.childListBox.FormattingEnabled = true;
            this.childListBox.Location = new System.Drawing.Point(3, 55);
            this.childListBox.Name = "childListBox";
            this.childListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.childListBox.Size = new System.Drawing.Size(160, 95);
            this.childListBox.TabIndex = 3;
            // 
            // baseStructureTextBox
            // 
            this.baseStructureTextBox.Location = new System.Drawing.Point(3, 16);
            this.baseStructureTextBox.Name = "baseStructureTextBox";
            this.baseStructureTextBox.ReadOnly = true;
            this.baseStructureTextBox.Size = new System.Drawing.Size(160, 20);
            this.baseStructureTextBox.TabIndex = 4;
            // 
            // childStructuresLabel
            // 
            this.childStructuresLabel.AutoSize = true;
            this.childStructuresLabel.Location = new System.Drawing.Point(0, 39);
            this.childStructuresLabel.Name = "childStructuresLabel";
            this.childStructuresLabel.Size = new System.Drawing.Size(84, 13);
            this.childStructuresLabel.TabIndex = 5;
            this.childStructuresLabel.Text = "Child Structures:";
            // 
            // parentStructureLabel
            // 
            this.parentStructureLabel.AutoSize = true;
            this.parentStructureLabel.Location = new System.Drawing.Point(0, 0);
            this.parentStructureLabel.Name = "parentStructureLabel";
            this.parentStructureLabel.Size = new System.Drawing.Size(80, 13);
            this.parentStructureLabel.TabIndex = 6;
            this.parentStructureLabel.Text = "Base Structure:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cloneButton);
            this.panel1.Controls.Add(this.removeSelectedButton);
            this.panel1.Controls.Add(this.browseChildrenButton);
            this.panel1.Controls.Add(this.browseParentButton);
            this.panel1.Controls.Add(this.childListBox);
            this.panel1.Controls.Add(this.parentStructureLabel);
            this.panel1.Controls.Add(this.baseStructureTextBox);
            this.panel1.Controls.Add(this.childStructuresLabel);
            this.panel1.Location = new System.Drawing.Point(12, 394);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(252, 155);
            this.panel1.TabIndex = 7;
            // 
            // removeSelectedButton
            // 
            this.removeSelectedButton.Location = new System.Drawing.Point(169, 125);
            this.removeSelectedButton.Name = "removeSelectedButton";
            this.removeSelectedButton.Size = new System.Drawing.Size(79, 23);
            this.removeSelectedButton.TabIndex = 11;
            this.removeSelectedButton.Text = "Remove";
            this.removeSelectedButton.UseVisualStyleBackColor = true;
            this.removeSelectedButton.Click += new System.EventHandler(this.removeSelectedButton_Click);
            // 
            // browseChildrenButton
            // 
            this.browseChildrenButton.Location = new System.Drawing.Point(169, 55);
            this.browseChildrenButton.Name = "browseChildrenButton";
            this.browseChildrenButton.Size = new System.Drawing.Size(79, 23);
            this.browseChildrenButton.TabIndex = 10;
            this.browseChildrenButton.Text = "Browse";
            this.browseChildrenButton.UseVisualStyleBackColor = true;
            this.browseChildrenButton.Click += new System.EventHandler(this.browseChildrenButton_Click);
            // 
            // browseParentButton
            // 
            this.browseParentButton.Location = new System.Drawing.Point(169, 14);
            this.browseParentButton.Name = "browseParentButton";
            this.browseParentButton.Size = new System.Drawing.Size(79, 23);
            this.browseParentButton.TabIndex = 7;
            this.browseParentButton.Text = "Browse";
            this.browseParentButton.UseVisualStyleBackColor = true;
            this.browseParentButton.Click += new System.EventHandler(this.browseParentButton_Click);
            // 
            // plotSelectedButton
            // 
            this.plotSelectedButton.Location = new System.Drawing.Point(114, 55);
            this.plotSelectedButton.Name = "plotSelectedButton";
            this.plotSelectedButton.Size = new System.Drawing.Size(79, 23);
            this.plotSelectedButton.TabIndex = 9;
            this.plotSelectedButton.Text = "Plot Selected";
            this.plotSelectedButton.UseVisualStyleBackColor = true;
            this.plotSelectedButton.Click += new System.EventHandler(this.plotSelectedButton_Click);
            // 
            // plotParentButton
            // 
            this.plotParentButton.Location = new System.Drawing.Point(114, 26);
            this.plotParentButton.Name = "plotParentButton";
            this.plotParentButton.Size = new System.Drawing.Size(79, 23);
            this.plotParentButton.TabIndex = 8;
            this.plotParentButton.Text = "Plot Parent";
            this.plotParentButton.UseVisualStyleBackColor = true;
            this.plotParentButton.Click += new System.EventHandler(this.plotParentButton_Click);
            // 
            // plotNextButton
            // 
            this.plotNextButton.Location = new System.Drawing.Point(114, 84);
            this.plotNextButton.Name = "plotNextButton";
            this.plotNextButton.Size = new System.Drawing.Size(79, 23);
            this.plotNextButton.TabIndex = 12;
            this.plotNextButton.Text = "Plot Next";
            this.plotNextButton.UseVisualStyleBackColor = true;
            this.plotNextButton.Click += new System.EventHandler(this.plotNextButton_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.plotStyleGroup);
            this.panel2.Controls.Add(this.plotSelectedButton);
            this.panel2.Controls.Add(this.plotNextButton);
            this.panel2.Controls.Add(this.plotParentButton);
            this.panel2.Location = new System.Drawing.Point(270, 435);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(203, 114);
            this.panel2.TabIndex = 13;
            // 
            // plotStyleGroup
            // 
            this.plotStyleGroup.Controls.Add(this.curlsRadio);
            this.plotStyleGroup.Controls.Add(this.moleculesRadio);
            this.plotStyleGroup.Controls.Add(this.vectorsRadio);
            this.plotStyleGroup.Controls.Add(this.centroidsRadio);
            this.plotStyleGroup.Location = new System.Drawing.Point(3, 3);
            this.plotStyleGroup.Name = "plotStyleGroup";
            this.plotStyleGroup.Size = new System.Drawing.Size(105, 107);
            this.plotStyleGroup.TabIndex = 13;
            this.plotStyleGroup.TabStop = false;
            this.plotStyleGroup.Text = "Plot Style";
            // 
            // curlsRadio
            // 
            this.curlsRadio.AutoSize = true;
            this.curlsRadio.Location = new System.Drawing.Point(6, 88);
            this.curlsRadio.Name = "curlsRadio";
            this.curlsRadio.Size = new System.Drawing.Size(48, 17);
            this.curlsRadio.TabIndex = 3;
            this.curlsRadio.Text = "Curls";
            this.curlsRadio.UseVisualStyleBackColor = true;
            // 
            // moleculesRadio
            // 
            this.moleculesRadio.AutoSize = true;
            this.moleculesRadio.Location = new System.Drawing.Point(6, 65);
            this.moleculesRadio.Name = "moleculesRadio";
            this.moleculesRadio.Size = new System.Drawing.Size(92, 17);
            this.moleculesRadio.TabIndex = 2;
            this.moleculesRadio.Text = "Full Molecules";
            this.moleculesRadio.UseVisualStyleBackColor = true;
            // 
            // vectorsRadio
            // 
            this.vectorsRadio.AutoSize = true;
            this.vectorsRadio.Location = new System.Drawing.Point(6, 42);
            this.vectorsRadio.Name = "vectorsRadio";
            this.vectorsRadio.Size = new System.Drawing.Size(61, 17);
            this.vectorsRadio.TabIndex = 1;
            this.vectorsRadio.Text = "Vectors";
            this.vectorsRadio.UseVisualStyleBackColor = true;
            // 
            // centroidsRadio
            // 
            this.centroidsRadio.AutoSize = true;
            this.centroidsRadio.Checked = true;
            this.centroidsRadio.Location = new System.Drawing.Point(6, 19);
            this.centroidsRadio.Name = "centroidsRadio";
            this.centroidsRadio.Size = new System.Drawing.Size(93, 17);
            this.centroidsRadio.TabIndex = 0;
            this.centroidsRadio.TabStop = true;
            this.centroidsRadio.Text = "Centroids Only";
            this.centroidsRadio.UseVisualStyleBackColor = true;
            // 
            // cloneButton
            // 
            this.cloneButton.Location = new System.Drawing.Point(170, 96);
            this.cloneButton.Name = "cloneButton";
            this.cloneButton.Size = new System.Drawing.Size(78, 23);
            this.cloneButton.TabIndex = 12;
            this.cloneButton.Text = "Clone";
            this.cloneButton.UseVisualStyleBackColor = true;
            this.cloneButton.Click += new System.EventHandler(this.cloneButton_Click);
            // 
            // CRYLAB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 561);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Name = "CRYLAB";
            this.Text = "CRYLAB";
            this.Load += new System.EventHandler(this.CRYLAB_Load);
            this.Click += new System.EventHandler(this.CRYLAB_Click);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.plotStyleGroup.ResumeLayout(false);
            this.plotStyleGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calculateToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem baseStructureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem childStructuresToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem structureMatFileToolStripMenuItem;
        private System.Windows.Forms.ListBox childListBox;
        private System.Windows.Forms.TextBox baseStructureTextBox;
        private System.Windows.Forms.Label childStructuresLabel;
        private System.Windows.Forms.Label parentStructureLabel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button browseParentButton;
        private System.Windows.Forms.Button plotSelectedButton;
        private System.Windows.Forms.Button plotParentButton;
        private System.Windows.Forms.Button browseChildrenButton;
        private System.Windows.Forms.Button removeSelectedButton;
        private System.Windows.Forms.Button plotNextButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox plotStyleGroup;
        private System.Windows.Forms.RadioButton curlsRadio;
        private System.Windows.Forms.RadioButton moleculesRadio;
        private System.Windows.Forms.RadioButton vectorsRadio;
        private System.Windows.Forms.RadioButton centroidsRadio;
        private System.Windows.Forms.ToolStripMenuItem colorsToolStripMenuItem;
        private System.Windows.Forms.Button cloneButton;
    }
}


namespace csPrepareRelease
{
    partial class FormMain
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.textBoxSolution = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxMajor = new System.Windows.Forms.TextBox();
			this.textBoxMinor = new System.Windows.Forms.TextBox();
			this.textBoxPatch = new System.Windows.Forms.TextBox();
			this.comboBoxMeta = new System.Windows.Forms.ComboBox();
			this.textBoxPre = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.buttonPre = new System.Windows.Forms.Button();
			this.buttonPatch = new System.Windows.Forms.Button();
			this.buttonMinor = new System.Windows.Forms.Button();
			this.buttonMajor = new System.Windows.Forms.Button();
			this.buttonReset = new System.Windows.Forms.Button();
			this.buttonUpdate = new System.Windows.Forms.Button();
			this.listBoxProjects = new System.Windows.Forms.ListBox();
			this.textBoxVersion = new System.Windows.Forms.TextBox();
			this.buttonSolution = new System.Windows.Forms.Button();
			this.checkBoxAddConfig = new System.Windows.Forms.CheckBox();
			this.comboBoxSemVer = new System.Windows.Forms.ComboBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// textBoxSolution
			// 
			this.textBoxSolution.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxSolution.Location = new System.Drawing.Point(76, 12);
			this.textBoxSolution.Name = "textBoxSolution";
			this.textBoxSolution.ReadOnly = true;
			this.textBoxSolution.Size = new System.Drawing.Size(262, 20);
			this.textBoxSolution.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(45, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Solution";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 69);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(45, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Version:";
			// 
			// textBoxMajor
			// 
			this.textBoxMajor.Location = new System.Drawing.Point(76, 66);
			this.textBoxMajor.Name = "textBoxMajor";
			this.textBoxMajor.Size = new System.Drawing.Size(37, 20);
			this.textBoxMajor.TabIndex = 3;
			this.textBoxMajor.Text = "1";
			this.textBoxMajor.TextChanged += new System.EventHandler(this.textBox_TextChanged);
			// 
			// textBoxMinor
			// 
			this.textBoxMinor.Location = new System.Drawing.Point(119, 66);
			this.textBoxMinor.Name = "textBoxMinor";
			this.textBoxMinor.Size = new System.Drawing.Size(37, 20);
			this.textBoxMinor.TabIndex = 4;
			this.textBoxMinor.Text = "0";
			this.textBoxMinor.TextChanged += new System.EventHandler(this.textBox_TextChanged);
			// 
			// textBoxPatch
			// 
			this.textBoxPatch.Location = new System.Drawing.Point(162, 66);
			this.textBoxPatch.Name = "textBoxPatch";
			this.textBoxPatch.Size = new System.Drawing.Size(61, 20);
			this.textBoxPatch.TabIndex = 5;
			this.textBoxPatch.Text = "0";
			this.textBoxPatch.TextChanged += new System.EventHandler(this.textBox_TextChanged);
			// 
			// comboBoxMeta
			// 
			this.comboBoxMeta.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxMeta.FormattingEnabled = true;
			this.comboBoxMeta.Items.AddRange(new object[] {
            "alpha",
            "beta",
            "rc"});
			this.comboBoxMeta.Location = new System.Drawing.Point(229, 66);
			this.comboBoxMeta.Name = "comboBoxMeta";
			this.comboBoxMeta.Size = new System.Drawing.Size(77, 21);
			this.comboBoxMeta.TabIndex = 6;
			this.comboBoxMeta.TextChanged += new System.EventHandler(this.comboBoxMeta_TextChanged);
			// 
			// textBoxPre
			// 
			this.textBoxPre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxPre.Location = new System.Drawing.Point(312, 66);
			this.textBoxPre.Name = "textBoxPre";
			this.textBoxPre.Size = new System.Drawing.Size(61, 20);
			this.textBoxPre.TabIndex = 7;
			this.textBoxPre.TextChanged += new System.EventHandler(this.textBox_TextChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(80, 50);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(33, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Major";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(123, 50);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(33, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Minor";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(188, 50);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(35, 13);
			this.label5.TabIndex = 10;
			this.label5.Text = "Patch";
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(275, 50);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(31, 13);
			this.label6.TabIndex = 11;
			this.label6.Text = "Meta";
			// 
			// buttonPre
			// 
			this.buttonPre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPre.Location = new System.Drawing.Point(344, 93);
			this.buttonPre.Name = "buttonPre";
			this.buttonPre.Size = new System.Drawing.Size(29, 23);
			this.buttonPre.TabIndex = 13;
			this.buttonPre.Text = "+";
			this.buttonPre.UseVisualStyleBackColor = true;
			this.buttonPre.Click += new System.EventHandler(this.buttonPre_Click);
			// 
			// buttonPatch
			// 
			this.buttonPatch.Location = new System.Drawing.Point(194, 93);
			this.buttonPatch.Name = "buttonPatch";
			this.buttonPatch.Size = new System.Drawing.Size(29, 23);
			this.buttonPatch.TabIndex = 14;
			this.buttonPatch.Text = "+";
			this.buttonPatch.UseVisualStyleBackColor = true;
			this.buttonPatch.Click += new System.EventHandler(this.buttonPatch_Click);
			// 
			// buttonMinor
			// 
			this.buttonMinor.Location = new System.Drawing.Point(127, 93);
			this.buttonMinor.Name = "buttonMinor";
			this.buttonMinor.Size = new System.Drawing.Size(29, 23);
			this.buttonMinor.TabIndex = 15;
			this.buttonMinor.Text = "+";
			this.buttonMinor.UseVisualStyleBackColor = true;
			this.buttonMinor.Click += new System.EventHandler(this.buttonMinor_Click);
			// 
			// buttonMajor
			// 
			this.buttonMajor.Location = new System.Drawing.Point(84, 93);
			this.buttonMajor.Name = "buttonMajor";
			this.buttonMajor.Size = new System.Drawing.Size(29, 23);
			this.buttonMajor.TabIndex = 16;
			this.buttonMajor.Text = "+";
			this.buttonMajor.UseVisualStyleBackColor = true;
			this.buttonMajor.Click += new System.EventHandler(this.buttonMajor_Click);
			// 
			// buttonReset
			// 
			this.buttonReset.Location = new System.Drawing.Point(12, 177);
			this.buttonReset.Name = "buttonReset";
			this.buttonReset.Size = new System.Drawing.Size(75, 23);
			this.buttonReset.TabIndex = 17;
			this.buttonReset.Text = "Reset";
			this.buttonReset.UseVisualStyleBackColor = true;
			this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
			// 
			// buttonUpdate
			// 
			this.buttonUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonUpdate.Location = new System.Drawing.Point(274, 177);
			this.buttonUpdate.Name = "buttonUpdate";
			this.buttonUpdate.Size = new System.Drawing.Size(104, 23);
			this.buttonUpdate.TabIndex = 18;
			this.buttonUpdate.Text = "Update Solution";
			this.buttonUpdate.UseVisualStyleBackColor = true;
			this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
			// 
			// listBoxProjects
			// 
			this.listBoxProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listBoxProjects.FormattingEnabled = true;
			this.listBoxProjects.IntegralHeight = false;
			this.listBoxProjects.Location = new System.Drawing.Point(0, 206);
			this.listBoxProjects.Name = "listBoxProjects";
			this.listBoxProjects.Size = new System.Drawing.Size(390, 230);
			this.listBoxProjects.TabIndex = 19;
			// 
			// textBoxVersion
			// 
			this.textBoxVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxVersion.Location = new System.Drawing.Point(96, 179);
			this.textBoxVersion.Name = "textBoxVersion";
			this.textBoxVersion.ReadOnly = true;
			this.textBoxVersion.Size = new System.Drawing.Size(172, 20);
			this.textBoxVersion.TabIndex = 20;
			// 
			// buttonSolution
			// 
			this.buttonSolution.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSolution.Location = new System.Drawing.Point(344, 10);
			this.buttonSolution.Name = "buttonSolution";
			this.buttonSolution.Size = new System.Drawing.Size(29, 23);
			this.buttonSolution.TabIndex = 21;
			this.buttonSolution.Text = "...";
			this.buttonSolution.UseVisualStyleBackColor = true;
			this.buttonSolution.Click += new System.EventHandler(this.buttonSolution_Click);
			// 
			// checkBoxAddConfig
			// 
			this.checkBoxAddConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxAddConfig.AutoSize = true;
			this.checkBoxAddConfig.Checked = true;
			this.checkBoxAddConfig.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAddConfig.Location = new System.Drawing.Point(209, 131);
			this.checkBoxAddConfig.Name = "checkBoxAddConfig";
			this.checkBoxAddConfig.Size = new System.Drawing.Size(164, 17);
			this.checkBoxAddConfig.TabIndex = 22;
			this.checkBoxAddConfig.Text = "add configuration tag to meta";
			this.checkBoxAddConfig.UseVisualStyleBackColor = true;
			this.checkBoxAddConfig.CheckedChanged += new System.EventHandler(this.checkBoxAddConfig_CheckedChanged);
			// 
			// comboBoxSemVer
			// 
			this.comboBoxSemVer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxSemVer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSemVer.FormattingEnabled = true;
			this.comboBoxSemVer.Items.AddRange(new object[] {
            "SemVer1",
            "SemVer2"});
			this.comboBoxSemVer.Location = new System.Drawing.Point(96, 129);
			this.comboBoxSemVer.Name = "comboBoxSemVer";
			this.comboBoxSemVer.Size = new System.Drawing.Size(95, 21);
			this.comboBoxSemVer.TabIndex = 23;
			this.comboBoxSemVer.SelectedIndexChanged += new System.EventHandler(this.comboBoxSemVer_SelectedIndexChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(12, 132);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(72, 13);
			this.label8.TabIndex = 24;
			this.label8.Text = "Configuration:";
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(350, 50);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(23, 13);
			this.label7.TabIndex = 25;
			this.label7.Text = "Pre";
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(390, 436);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.comboBoxSemVer);
			this.Controls.Add(this.checkBoxAddConfig);
			this.Controls.Add(this.buttonSolution);
			this.Controls.Add(this.textBoxVersion);
			this.Controls.Add(this.listBoxProjects);
			this.Controls.Add(this.buttonUpdate);
			this.Controls.Add(this.buttonReset);
			this.Controls.Add(this.buttonMajor);
			this.Controls.Add(this.buttonMinor);
			this.Controls.Add(this.buttonPatch);
			this.Controls.Add(this.buttonPre);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textBoxPre);
			this.Controls.Add(this.comboBoxMeta);
			this.Controls.Add(this.textBoxPatch);
			this.Controls.Add(this.textBoxMinor);
			this.Controls.Add(this.textBoxMajor);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxSolution);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(406, 388);
			this.Name = "FormMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Prepare Release";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSolution;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxMajor;
        private System.Windows.Forms.TextBox textBoxMinor;
        private System.Windows.Forms.TextBox textBoxPatch;
        private System.Windows.Forms.ComboBox comboBoxMeta;
        private System.Windows.Forms.TextBox textBoxPre;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonPre;
        private System.Windows.Forms.Button buttonPatch;
        private System.Windows.Forms.Button buttonMinor;
        private System.Windows.Forms.Button buttonMajor;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.ListBox listBoxProjects;
        private System.Windows.Forms.TextBox textBoxVersion;
        private System.Windows.Forms.Button buttonSolution;
        private System.Windows.Forms.CheckBox checkBoxAddConfig;
        private System.Windows.Forms.ComboBox comboBoxSemVer;
        private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
	}
}


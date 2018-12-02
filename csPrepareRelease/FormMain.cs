using Cave;
using Cave.Console;
using Cave.FileSystem;
using Cave.IO;
using Cave.Text;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace csPrepareRelease
{
    partial class FormMain : Form
    {
        csPrepareRelease program;
        List<Project> Projects;
        Ini ini;
        bool updating;

        public FormMain(csPrepareRelease program, List<Project> projects)
        {
            InitializeComponent();
            this.program = program;
            textBoxSolution.Text = program.Solution;
            Projects = projects;
            Init();
        }

        private void Init()
        {
            var fileName = Path.GetFullPath(textBoxSolution.Text + ".ver");
            FileSystem.TouchFile(fileName);
            ini = new Ini(fileName);
            LoadVersonFile();
            foreach (var p in Projects) listBoxProjects.Items.Add(p.DirectoryPath);
        }

        string GetMeta(bool withCheckBox)
        {
            var s = new string(comboBoxMeta.Text.Trim().ToLower().Where(c => (c >= 'a' && c <= 'z') || (c == '+') || (c == '-')).ToArray());
            if (withCheckBox && checkBoxAddConfig.Checked)
            {
                if (s.Length > 0) s += (s.LastIndexOf('+') > s.LastIndexOf('-')) ? '+' : '-';
                s += "$CONF$";
            }
            s = s.Replace("--", "-").Replace("++", "+");
            return s;
        }

        enum VFlags
        {
            WithConfig = 1,
            UpdateIni = 2,
            Version2 = 4,
        }

        string BuildVersionString(VFlags flags)
        {
            bool withConfig = flags.HasFlag(VFlags.WithConfig);
            bool updateIni = flags.HasFlag(VFlags.UpdateIni);
            bool version2 = flags.HasFlag(VFlags.Version2);

			if (withConfig && updateIni)
			{
				//ini should never contain configuration, so update without config
				BuildVersionString(flags & ~VFlags.WithConfig);
				updateIni = false;
			}

            int major, minor, patch, pre;
            if (!int.TryParse(textBoxMajor.Text, out major)) major = 1;
            if (!int.TryParse(textBoxMinor.Text, out minor)) minor = 0;
            if (string.IsNullOrWhiteSpace(textBoxPatch.Text)) patch = -1; else if (!int.TryParse(textBoxPatch.Text, out patch)) patch = 0;
			if (string.IsNullOrWhiteSpace(textBoxPre.Text)) pre = -1; else if (!int.TryParse(textBoxPre.Text, out pre)) pre = -1;
            StringBuilder sb = new StringBuilder();
            sb.Append(major); sb.Append('.'); sb.Append(minor);
            if (patch > -1) { sb.Append('.'); sb.Append(patch); }
            string meta = GetMeta(withConfig);
            if (!version2)
            {
                if (pre > 0 || !string.IsNullOrWhiteSpace(meta))
                {
                    string s = $"{meta}";
                    if (pre > 0) s += "-" + pre.ToString();
                    s = s.Split(".-+".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Join("-");
                    sb.Append('-');
                    sb.Append(s);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(meta))
                {
                    if (!meta.StartsWith("-") && !meta.StartsWith("+")) sb.Append('-');
                    sb.Append(meta);
                }
				if (pre > -1)
                {
                    sb.Append('.');
                    sb.Append(pre);
                }
            }
            if (updateIni)
            {
                if (ini.HasSection("Version")) ini.RemoveSection("Version");
                ini.WriteSetting("Version", "AddConfig", checkBoxAddConfig.Checked.ToString());
                ini.WriteSetting("Version", "Major", major.ToString());
                ini.WriteSetting("Version", "Minor", minor.ToString());
                ini.WriteSetting("Version", "Patch", patch.ToString());
                ini.WriteSetting("Version", "Pre", pre.ToString());
                ini.WriteSetting("Version", "Meta", meta);
                ini.WriteSetting("Version", "Type", comboBoxSemVer.SelectedIndex.ToString());
				ini.WriteSetting("Version", "Full", sb.ToString());
			}
            return sb.ToString();
        }

		void UpdateVersionString(bool updateIni)
		{
			if (updating) return;
			VFlags flags = 0;
			if (comboBoxSemVer.SelectedIndex == 1) { flags |= VFlags.Version2; }
			if (updateIni) { flags |= VFlags.UpdateIni; }
			if (checkBoxAddConfig.Checked) { flags |= VFlags.WithConfig; }
			textBoxVersion.Text = BuildVersionString(flags);
		}

        private void LoadVersonFile()
        {
            updating = true;
            textBoxMajor.Text = ini.ReadSetting("Version", "Major");
            textBoxMinor.Text = ini.ReadSetting("Version", "Minor");
            textBoxPatch.Text = ini.ReadSetting("Version", "Patch");
            textBoxPre.Text = ini.ReadSetting("Version", "Pre");
            comboBoxMeta.Text = ini.ReadSetting("Version", "Meta");
            checkBoxAddConfig.Checked = ini.ReadBool("Version", "AddConfig", true);
            try { comboBoxSemVer.SelectedIndex = ini.ReadInt32("Version", "Type", 0); } catch { comboBoxSemVer.SelectedIndex = 0; }
            updating = false;
            UpdateVersionString(false);
        }

        private void buttonMajor_Click(object sender, EventArgs e)
        {
            int i;
            int.TryParse(textBoxMajor.Text, out i);
            textBoxMajor.Text = (++i).ToString();
            textBoxMinor.Text = "0";
            textBoxPatch.Text = "0";
            textBoxPre.Text = "";
            comboBoxMeta.Text = "";
        }

        private void buttonMinor_Click(object sender, EventArgs e)
        {
            int i;
            int.TryParse(textBoxMinor.Text, out i);
            textBoxMinor.Text = (++i).ToString();
            textBoxPatch.Text = "0";
            textBoxPre.Text = "";
            comboBoxMeta.Text = "";
        }

        private void buttonPatch_Click(object sender, EventArgs e)
        {
            int i;
            int.TryParse(textBoxPatch.Text, out i);
            textBoxPatch.Text = (++i).ToString();
            textBoxPre.Text = "";
            comboBoxMeta.Text = "";
        }

        private void buttonPre_Click(object sender, EventArgs e)
        {
            int i;
            int.TryParse(textBoxPre.Text, out i);
            textBoxPre.Text = (++i).ToString();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            LoadVersonFile();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            UpdateVersionString(true);
            Enabled = false;
            try
            {
                foreach (var p in Projects)
                {
                    listBoxProjects.SelectedItem = p.DirectoryPath;
                    Application.DoEvents();
                    ProjectPatcher.Patch(p, textBoxVersion.Text);
                }
                ini.Save();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                Enabled = true;
            }
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            TextBox t = sender as TextBox;
            if (t == null) return;
            int i;
            if (string.IsNullOrWhiteSpace(t.Text) || int.TryParse(t.Text, out i))
            {
                t.BackColor = SystemColors.Window;
                t.ForeColor = SystemColors.WindowText;
            }
            else
            {
                t.BackColor = Color.LightCoral;
                t.ForeColor = Color.Black;
            }
            UpdateVersionString(false);
        }

        private void comboBoxMeta_TextChanged(object sender, EventArgs e)
        {
            ComboBox c = sender as ComboBox;
            if (c == null) return;
            if (GetMeta(false) == comboBoxMeta.Text)
            {
                c.BackColor = SystemColors.Window;
                c.ForeColor = SystemColors.WindowText;
            }
            else
            {
                c.BackColor = Color.LightCoral;
                c.ForeColor = Color.Black;
            }
            UpdateVersionString(false);
        }

        private void buttonSolution_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                DefaultExt = ".sln",
                FileName = textBoxSolution.Text,
                Filter = "Solution files|*.sln|All files (*.*)|*.*",
                FilterIndex = 0,
                Multiselect = false,
                Title = "Select solution"
            })
            {
                if (DialogResult.OK == dialog.ShowDialog())
                {
                    textBoxSolution.Text = dialog.FileName;
                    Projects = program.LoadSolution(Arguments.FromArray(dialog.FileName));
                    Init();
                }
            }
        }

        private void checkBoxAddConfig_CheckedChanged(object sender, EventArgs e)
        {
            UpdateVersionString(false);
        }

        private void comboBoxSemVer_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateVersionString(false);
        }
    }
}

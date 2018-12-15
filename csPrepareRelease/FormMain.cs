using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cave;
using Cave.Console;
using Cave.FileSystem;
using Microsoft.Build.Evaluation;

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
            string fileName = Path.GetFullPath(textBoxSolution.Text + ".ver");
            FileSystem.TouchFile(fileName);
            ini = new Ini(fileName);
            LoadVersonFile();
            foreach (Project p in Projects)
            {
                listBoxProjects.Items.Add(p.DirectoryPath);
            }
        }

        string GetMeta(bool withConfig, bool withDateTime, bool version2)
        {
            string s = new string(comboBoxMeta.Text.Trim().ToLower().Where(c => (c >= 'a' && c <= 'z') || (c == '+') || (c == '-')).ToArray());
            if (withConfig && checkBoxAddConfig.Checked)
            {
                if (s.Length > 0)
                {
                    s += (s.LastIndexOf('+') > s.LastIndexOf('-')) ? '+' : '-';
                }

                s += "$CONF$";
            }
            if (withDateTime && checkBoxAddDateTime.Checked)
            {
                if (version2)
                {
                    if (s.Length > 0)
                    {
                        s += ".";
                    }
                }
                else
                {
                    if (s.Length > 0)
                    {
                        s += (s.LastIndexOf('+') > s.LastIndexOf('-')) ? '+' : '-';
                    }
                }
                s += "$DATETIME$";
            }
            s = s.Replace("--", "-").Replace("++", "+");
            return s;
        }

        [Flags]
        enum VFlags
        {
            MetaWithConfig = 1,
            UpdateIni = 2,
            Version2 = 4,
            MetaWithDateTime = 8,
        }

        string BuildVersionString(VFlags flags)
        {
            bool metaWithConfig = flags.HasFlag(VFlags.MetaWithConfig);
            bool metaWithDateTime = flags.HasFlag(VFlags.MetaWithDateTime);
            bool updateIni = flags.HasFlag(VFlags.UpdateIni);
            bool version2 = flags.HasFlag(VFlags.Version2);

            if ((metaWithConfig || metaWithDateTime) && updateIni)
            {
                //ini should never contain additional meta, so update without it
                BuildVersionString(flags & ~VFlags.MetaWithConfig & ~VFlags.MetaWithDateTime);
                updateIni = false;
            }

            int patch;
            if (!int.TryParse(textBoxMajor.Text, out int major))
            {
                major = 1;
            }

            if (!int.TryParse(textBoxMinor.Text, out int minor))
            {
                minor = 0;
            }

            if (string.IsNullOrWhiteSpace(textBoxPatch.Text))
            {
                patch = -1;
            }
            else if (!int.TryParse(textBoxPatch.Text, out patch))
            {
                patch = 0;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(major); sb.Append('.'); sb.Append(minor);
            if (patch > -1) { sb.Append('.'); sb.Append(patch); }
            string meta = GetMeta(metaWithConfig, metaWithDateTime, version2);
            if (!version2)
            {
                if (!string.IsNullOrWhiteSpace(meta))
                {
                    string s = $"{meta}";
                    s = s.Split(".-+".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Join("-");
                    sb.Append('-');
                    sb.Append(s);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(meta))
                {
                    if (!meta.StartsWith("-") && !meta.StartsWith("+"))
                    {
                        sb.Append('-');
                    }

                    sb.Append(meta);
                }
            }
            if (updateIni)
            {
                if (ini.HasSection("Version"))
                {
                    ini.RemoveSection("Version");
                }

                ini.WriteSetting("Version", "AddConfig", checkBoxAddConfig.Checked.ToString());
                ini.WriteSetting("Version", "AddDateTime", checkBoxAddDateTime.Checked.ToString());
                ini.WriteSetting("Version", "Major", major.ToString());
                ini.WriteSetting("Version", "Minor", minor.ToString());
                ini.WriteSetting("Version", "Patch", patch.ToString());
                ini.WriteSetting("Version", "Meta", meta);
                ini.WriteSetting("Version", "Type", comboBoxSemVer.SelectedIndex.ToString());
                ini.WriteSetting("Version", "Full", sb.ToString());
            }
            return sb.ToString();
        }

        void UpdateVersionString(bool updateIni)
        {
            if (updating)
            {
                return;
            }

            VFlags flags = 0;
            if (comboBoxSemVer.SelectedIndex == 1) { flags |= VFlags.Version2; }
            if (updateIni) { flags |= VFlags.UpdateIni; }
            if (checkBoxAddConfig.Checked) { flags |= VFlags.MetaWithConfig; }
            if (checkBoxAddDateTime.Checked) { flags |= VFlags.MetaWithDateTime; }
            textBoxVersion.Text = BuildVersionString(flags);
        }

        private void LoadVersonFile()
        {
            updating = true;
            textBoxMajor.Text = ini.ReadString("Version", "Major", "").Trim();
            textBoxMinor.Text = ini.ReadString("Version", "Minor", "").Trim();
            textBoxPatch.Text = ini.ReadString("Version", "Patch", "").Trim();
            comboBoxMeta.Text = ini.ReadString("Version", "Meta", "").Trim();
            checkBoxAddConfig.Checked = ini.ReadBool("Version", "AddConfig", true);
            checkBoxAddDateTime.Checked = ini.ReadBool("Version", "AddDateTime", true);
            try { comboBoxSemVer.SelectedIndex = ini.ReadInt32("Version", "Type", 0); } catch { comboBoxSemVer.SelectedIndex = 0; }
            updating = false;
            UpdateVersionString(false);
        }

        private void buttonMajor_Click(object sender, EventArgs e)
        {
            int.TryParse(textBoxMajor.Text, out int i);
            textBoxMajor.Text = (++i).ToString();
            textBoxMinor.Text = "0";
            textBoxPatch.Text = "0";
            comboBoxMeta.Text = "";
        }

        private void buttonMinor_Click(object sender, EventArgs e)
        {
            int.TryParse(textBoxMinor.Text, out int i);
            textBoxMinor.Text = (++i).ToString();
            textBoxPatch.Text = "0";
            comboBoxMeta.Text = "";
        }

        private void buttonPatch_Click(object sender, EventArgs e)
        {
            int.TryParse(textBoxPatch.Text, out int i);
            textBoxPatch.Text = (++i).ToString();
            comboBoxMeta.Text = "";
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
                foreach (Project p in Projects)
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
            if (t == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(t.Text) || int.TryParse(t.Text, out int i))
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
            if (c == null)
            {
                return;
            }

            if (GetMeta(false, false, comboBoxSemVer.SelectedIndex == 1) == comboBoxMeta.Text)
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
            using (OpenFileDialog dialog = new OpenFileDialog()
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

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateVersionString(false);
        }

        private void comboBoxSemVer_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateVersionString(false);
        }
    }
}

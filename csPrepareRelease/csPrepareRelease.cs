﻿using Cave;
using Cave.Console;
using Cave.FileSystem;
using Cave.IO;
using Cave.Text;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace csPrepareRelease
{
    class csPrepareRelease 
    {
        const string Banner = "csPrepareRelease - Release Patcher - (c) 2018 CaveSystems GmbH, Andreas Rohleder";

        public string Solution { get; private set; }

        public string LogSourceName => "csPrepareRelease";

        public bool IsConsoleMode { get; private set; }

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            new csPrepareRelease().Run(Arguments.FromArray(args));
        }

        private void Run(Arguments arguments)
        {
            try
            {
                if (arguments.IsHelpOptionFound()) { Help(); return; }
				if (arguments.IsOptionPresent("increment") || arguments.IsOptionPresent("pre") || arguments.IsOptionPresent("stable"))
				{
					AutoUpdate(arguments);
					return;
				}
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                List<Project> projects = LoadSolution(arguments);
                Application.Run(new FormMain(this, projects));
            }
            catch (Exception ex)
            {
                if (!IsConsoleMode)
                {
                    MessageBox.Show(ex.ToString());
                }
                if (Debugger.IsAttached) throw;
            }
            finally
            {
                if (IsConsoleMode && (Debugger.IsAttached || arguments.IsOptionPresent("wait")))
                {
                    SystemConsole.WriteLine("--- press <yellow>enter<default> to exit ---");
                    SystemConsole.ReadLine();
                }
            }
        }

        void Help()
        {
            SystemConsole.WriteLine(
                Banner +
                "\n" +
                "csPrepareRelease [<cyan>--increment<default>] <magenta>solution.sln" +
                "Prepares a visual studio solution for release patching csproj, AssemblyInfo, nuspec and setup files." +
                "\n" +
				"  <cyan>--stable[=patch]<default>: clear meta and pre and use only major.minor.patch." +
				"  <cyan>--increment<default>:      automatically patch the solution to the next pre/patch version." +
				"  <cyan>--pre=<value><default>:    set pre to the specified value (integer) but you can you use 'time' too." +
				"  <cyan>--debug<default>:          set loglevel to debug." +
                "  <cyan>--verbose<default>:        set loglevel to verbose." +
                "  <cyan>--wait<default>:           wait for console key after execution." +
                "\n");
        }

		void AutoUpdate(Arguments arguments)
		{
			SystemConsole.WriteLine(Banner);
			SystemConsole.WriteLine();

			List<Project> projects = LoadProjects(arguments);
            SystemConsole.WriteLine("Solution {0} with {1} projects loaded.", Solution, projects.Count);

			var fileName = Path.GetFullPath(Solution + ".ver");
			FileSystem.TouchFile(fileName);
			var ini = new Ini(fileName);

			var major = ini.ReadInt32("Version", "Major", 0);
			var minor = ini.ReadInt32("Version", "Minor", 0);
			var patch = ini.ReadInt32("Version", "Patch", 0);
			var pre = ini.ReadInt32("Version", "Pre", 0);
			var meta = ini.ReadSetting("Version", "Meta");
			if (meta == null) meta = "";
			var addConfig = ini.ReadBool("Version", "AddConfig", true);
			var semVerType = ini.ReadInt32("Version", "Type", 0);

			if (arguments.IsOptionPresent("pre"))
			{
				var value = arguments.Options["pre"].Value;
				if (!int.TryParse(value, out pre))
				{
					switch(value.ToLower())
					{
						case "time": pre = (int)(DateTime.Now - new DateTime(2000, 1, 1)).TotalMinutes; break;
						default: throw new Exception("Unknown pre value " + value);
					}
				}
			}
			if (arguments.IsOptionPresent("stable"))
			{
				meta = null;
				pre = -1;
				if (int.TryParse(arguments.Options["stable"].Value, out int stablePatch)) patch = stablePatch;
				if (arguments.IsOptionPresent("increment")) patch++;
			}
			else
			{
				if (arguments.IsOptionPresent("increment")) pre++;
				meta = new string(meta.Where(c => (c >= 'a' && c <= 'z') || (c == '+') || (c == '-')).ToArray());
				if (addConfig)
				{
					if (meta.Length > 0) meta += (meta.LastIndexOf('+') > meta.LastIndexOf('-')) ? '+' : '-';
					meta += "$CONF$";
				}
			}

			string semVer;
			if (string.IsNullOrWhiteSpace(meta) && pre <= 0)
			{
				semVer = $"{major}.{minor}.{patch}";
			}
			else
			{
				switch (semVerType)
				{
					//version 1
					default:
						meta = meta.Split(".-+".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Join("-");
						if (pre >= 0)
						{
							semVer = $"{major}.{minor}.{patch}-{meta}-{pre}";
						}
						else
						{
							semVer = $"{major}.{minor}.{patch}-{meta}";
						}
						break;
					//version 2
					case 1:
						if (pre >= 0)
						{
							semVer = $"{major}.{minor}.{patch}-{meta}.{pre}";
						}
						else
						{
							semVer = $"{major}.{minor}.{patch}-{meta}";
						}
						break;
				}
			}
            SystemConsole.WriteLine("Patching to version <cyan>{0}", semVer);

			foreach (var project in projects)
			{
                SystemConsole.WriteLine("Patching project <cyan>{0}<default> with version <cyan>{1}", Path.GetFileName(project.FullPath), semVer);
				ProjectPatcher.Patch(project, semVer);
			}
            SystemConsole.WriteLine("{0} projects patched.", projects.Count);
		}

        public List<Project> LoadSolution(Arguments arguments)
        {
			using (var splash = new FormSplash())
			{
				splash.Show(); Application.DoEvents();
				splash.BringToFront(); Application.DoEvents();

				List<Project> projects = new List<Project>();
				var t = Task.Factory.StartNew(() => { projects = LoadProjects(arguments); });
				while (!t.IsCompleted)
				{
					t.Wait(10);
					Application.DoEvents();
				}
				if (t.IsFaulted) MessageBox.Show(t.Exception.ToString());
				return projects;
			}
        }

        private List<Project> LoadProjects(Arguments args)
        {
            List<Project> projects = new List<Project>();
            foreach (string p in args.Parameters)
            {
                var file = Path.GetFullPath(FileSystem.Combine(".", p));
                SystemConsole.WriteLine("Loading {0}", file);
                if (File.Exists(file))
                {
                    string folder = Path.GetDirectoryName(file);
                    int projectNumber = 0;
                    foreach (string s in File.ReadAllLines(file).Where(s => s.StartsWith("Project(")))
                    {
                        projectNumber++;
                        string fileName = null;
                        try
                        {
                            string name = s.Substring(s.IndexOf('='));
                            name = name.Split(new string[] { "\", \"" }, StringSplitOptions.None)[1];
                            fileName = Path.Combine(folder, name);
                            SystemConsole.WriteLine("Loading project {0} {1}", projectNumber, fileName);
                            if (File.Exists(fileName))
                            {
                                Project project = new Project(fileName, null, null, new ProjectCollection());
                                projects.Add(project);
                                Solution = file;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (IsConsoleMode)
                            {
                                if (fileName != null)
                                {
                                    SystemConsole.WriteLine("Could not load project {0}", fileName);
                                    SystemConsole.WriteLine(ex.ToXT());
                                }
                                else
                                {
                                    SystemConsole.WriteLine("Could not load project {0}", projectNumber);
                                    SystemConsole.WriteLine(ex.ToXT());
                                }
                            }
                            MessageBox.Show(ex.ToString());
                        }
                    }
                }
            }
            return projects;
        }
    }
}
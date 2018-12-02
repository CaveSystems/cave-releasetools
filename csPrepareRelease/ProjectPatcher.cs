using System;
using System.IO;
using Cave;
using Cave.Console;
using Cave.Text;
using Microsoft.Build.Evaluation;

namespace csPrepareRelease
{
    internal class ProjectPatcher
    {
        readonly Project project;
        readonly string fullVersion;
		readonly Version shortVersion;
		readonly Version fileVersion;

		public ProjectPatcher(Project project, string fullVersion)
        {
            this.project = project;
            this.fullVersion = fullVersion;
			var ver = SemanticVersion.Parse(fullVersion.Replace("$CONF$", ""));
			int.TryParse(fullVersion.Substring(fullVersion.LastIndexOfAny(".-".ToCharArray()) + 1).GetValidChars(ASCII.Strings.Digits), out int pre);
			shortVersion = (pre > 0) ? Version.Parse(ver.GetNormalizedVersion() + "." + pre) : ver.GetNormalizedVersion();

			int y = DateTime.UtcNow.Year;
			int md = DateTime.UtcNow.Month * 100 + DateTime.UtcNow.Day;
			int hm = DateTime.UtcNow.Hour * 100 + DateTime.UtcNow.Minute;
			int c = y ^ md ^ hm;
			fileVersion = new Version(y, md, hm, c);
		}

        public string LogSourceName => "ProjectPatcher";

        internal static void Patch(Project project, string fullVersion)
        {
            var patcher = new ProjectPatcher(project, fullVersion);
            patcher.Run();
        }

        private void Run()
        {
            string csProjVersion = fullVersion.Replace("$CONF$", "$(Configuration.ToLower())");
            SystemConsole.WriteLine("{0} Project.Version = <cyan>{1}", Path.GetFileName(project.FullPath), csProjVersion);
            project.SetProperty("Version", csProjVersion);

			project.SetProperty("AssemblyVersion", shortVersion.ToString());
			project.SetProperty("FileVersion", fileVersion.ToString());

			if (!string.IsNullOrEmpty(project.GetPropertyValue("ApplicationVersion")))
            {
                SystemConsole.WriteLine("{0} Project.ApplicationVersion = <cyan>{1}", Path.GetFileName(project.FullPath), csProjVersion);
                project.SetProperty("ApplicationVersion", csProjVersion);
            }
			project.Save();

            PatchAssemblyInfo();
            PatchInnoSetup();
            PatchNuspec();
        }

        private string GetVersionByFileName(string patchFile)
        {
            if (patchFile.ToLower().Contains("debug")) return fullVersion.Replace("$CONF$", "debug");
            if (patchFile.ToLower().Contains("release")) return fullVersion.Replace("$CONF$", "release");
            return fullVersion;
        }

        private void PatchNuspec()
        {
            foreach (var patchFile in Directory.GetFiles(project.DirectoryPath, "*.nuspec", SearchOption.AllDirectories))
            {
                var text = File.ReadAllText(patchFile);
                int start = text.IndexOf("<version>");
                int end = text.IndexOf("</version>", start);
                if (start < 0 || end < 0) continue;
                start += 9;
                string ver = GetVersionByFileName(patchFile);
                text = text.Remove(start, end - start).Insert(start, ver);
                File.WriteAllText(patchFile, text);
                SystemConsole.WriteLine("{0} Version = <cyan>{1}", Path.GetFileName(patchFile), ver);
            }
        }       

        private void PatchInnoSetup()
        {
            foreach (var patchFile in Directory.GetFiles(project.DirectoryPath, "*.iss", SearchOption.AllDirectories))
            {
                var lines = File.ReadAllLines(patchFile);
                string ver = GetVersionByFileName(patchFile);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!lines[i].Contains("#define")) continue;
                    switch (lines[i].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1])
                    {
                        case "VERSION": lines[i] = $"#define VERSION \"{ver}\""; break;
                    }
                }
                File.WriteAllLines(patchFile, lines);
                SystemConsole.WriteLine("{0} Version = <cyan>{1}", Path.GetFileName(patchFile), ver);
            }
        }

        private void PatchAssemblyInfo()
        {
            foreach (var patchFile in Directory.GetFiles(project.DirectoryPath, "AssemblyInfo.cs", SearchOption.AllDirectories))
            {
                var lines = File.ReadAllLines(patchFile);
                string ver = GetVersionByFileName(patchFile);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("[assembly: AssemblyVersion("))
                    {
						lines[i] = "[assembly: AssemblyVersion(\"" + shortVersion + "\")]";
                        SystemConsole.WriteLine("{0} AssemblyVersion = <cyan>{1}", Path.GetFileName(patchFile), shortVersion);
                    }
                    else if (lines[i].Contains("[assembly: AssemblyInformationalVersion("))
                    {
                        lines[i] = "[assembly: AssemblyInformationalVersion(\"" + ver + "\")]";
                        SystemConsole.WriteLine("{0} AssemblyInformationalVersion = <cyan>{1}", Path.GetFileName(patchFile), ver);
                    }
                    else if (lines[i].Contains("[assembly: AssemblyFileVersion("))
                    {
                        lines[i] = "[assembly: AssemblyFileVersion(\"" + fileVersion + "\")]";
                        SystemConsole.WriteLine("{0} AssemblyFileVersion = <cyan>{1}", Path.GetFileName(patchFile), fileVersion);
                    }
                }
                File.WriteAllLines(patchFile, lines);
            }
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Cave;
using Cave.Console;
using Cave.Net;

namespace csPublish
{
    class csPublish
    {
        static int Main(string[] args)
        {
            try
            {
                new csPublish().Run(Arguments.FromArray(args));
                return 0;
            }
            catch (Exception ex)
            {
                SystemConsole.WriteLine(ex.ToXT(true));
                return 1;
            }
        }

        private void Run(Arguments arguments)
        {
            if (arguments.IsHelpOptionFound() || arguments.Parameters.Count > 1)
            {
                Help();
                return;
            }
            string configFileName = arguments.Parameters.Count == 0 ? "publish.ini" : arguments.Parameters[0];
            if (!File.Exists(configFileName))
            {
                throw new FileNotFoundException(string.Format("File {0} cannot be found!", configFileName), configFileName);
            }

            Ini config = new Ini(configFileName);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(config.Name)));
            if (arguments.IsOptionPresent("root"))
            {
                Directory.SetCurrentDirectory(arguments.Options["root"].Value);
            }
            Publish(config);
        }

        private void Help()
        {
            SystemConsole.WriteLine("csPublish [<publish.{0}>] [--create] [-root=<dir>]", Ini.PlatformExtension);
            SystemConsole.WriteLine();
            SystemConsole.WriteLine("--create: creates a default configuration.");
            SystemConsole.WriteLine();
        }

        private void Publish(Ini config)
        {
            LoadIncludes(config);
            string[] files = Directory.GetFiles(".", "*", SearchOption.AllDirectories);
            foreach (string section in config.GetSectionNames())
            {
                if (!section.ToLower().Trim().StartsWith("publish:"))
                {
                    continue;
                }

                Publish(config, section, files);
            }
        }

        private void LoadIncludes(Ini config)
        {
            { // check version
                Version ver = new Version(config.ReadSetting("csPublish", "Version"));
                if (LatestVersion.VersionIsNewer(ver, AssemblyVersionInfo.Program.AssemblyVersion))
                {
                    throw new Exception("Newer csPublish version required!");
                }
            }

            string[] includes = config.ReadSection("include", true);
            if (includes.Length > 0)
            {
                config.RemoveSection("include");
                foreach (string inc in includes)
                {
                    Ini sub = new Ini(inc);
                    LoadIncludes(sub);
                    foreach (string section in sub.GetSectionNames())
                    {
                        if (config.HasSection(section))
                        {
                            throw new NotSupportedException(string.Format("Main configuration and included {0} contains a section with the name {1}.", Path.GetFileName(sub.Name), section));
                        }

                        config.WriteSection(section, sub.ReadSection(section));
                    }
                }
            }
        }

        private void Publish(Ini config, string sectionName, string[] files)
        {
            string source = config.ReadSetting(sectionName, "Source");
            Regex regex = CreateRegex(source);
            SystemConsole.WriteLine("<yellow>{0}", sectionName);
            foreach (string file in files)
            {
                string fileName = file.Replace('\\', '/');
                if (regex.IsMatch(fileName))
                {
                    PublishFile(config, sectionName, file);
                }
            }
        }

        private void PublishFile(Ini config, string sectionName, string file)
        {
            string target = config.ReadSetting(sectionName, "target");
            switch (target?.ToLower().BeforeFirst(':'))
            {
                case "globalfolder": PublishFileToFolder(config, sectionName, file, true); break;
                case "projectfolder": PublishFileToFolder(config, sectionName, file, false); break;
                case "ftp": PublishFileToFtp(config, sectionName, file, target); break;
                default: throw new NotSupportedException(string.Format("Unknown target {0}", target));
            }
        }

        private void PublishFileToFtp(Ini config, string sectionName, string file, string target)
        {
            FtpConnection con = new FtpConnection
            {
                EnableSSL = config.ReadBool(target, "ssl", true)
            };
            ConnectionString connectionString = config.ReadSetting(target, "Target");

            string user = config.ReadSetting(target, "Username");
            if (!string.IsNullOrEmpty(user))
            {
                connectionString.UserName = user;
            }

            string pass = config.ReadSetting(target, "Password");
            if (!string.IsNullOrEmpty(pass))
            {
                connectionString.Password = pass;
            }

            connectionString.Location = FileSystem.Combine('/', config.ReadSetting(target, "Path"), Path.GetFileName(file));
            SystemConsole.Write("<cyan>Upload<default>: {0} -> <cyan>{1}<default> ..", file, connectionString);
            con.Upload(connectionString, File.ReadAllBytes(file));
            SystemConsole.WriteLine(" <green>ok");

            bool move = config.ReadBool(sectionName, "move", false) | config.ReadBool(sectionName, "delete", false);
            SystemConsole.Write("<red>Delete<default>: {0} ..", file);
            File.Delete(file);
            SystemConsole.WriteLine(" <green>ok");
        }

        private void PublishFileToFolder(Ini config, string sectionName, string file, bool global)
        {
            string folder = config.ReadSetting(sectionName, "Folder");
            if (!global)
            {
                folder = FileSystem.Combine(".", folder.TrimStart('/', '\\'));
            }

            Directory.CreateDirectory(folder);
            bool move = config.ReadBool(sectionName, "move", false) | config.ReadBool(sectionName, "delete", false);
            string target = FileSystem.Combine(folder, Path.GetFileName(file));
            if (move)
            {
                if (File.Exists(target))
                {
                    SystemConsole.WriteLine("<red>Delete<default>: {0} ..", file);
                    File.Delete(file);
                    SystemConsole.WriteLine(" <green>ok");
                }
                else
                {
                    SystemConsole.WriteLine("<cyan>Move<default>: {0} -> <cyan>{1}<default> ..", file, target);
                    File.Copy(file, target);
                    File.Delete(file);
                    SystemConsole.WriteLine(" <green>ok");
                }
            }
            else
            {
                if (File.Exists(target))
                {
                    SystemConsole.WriteLine("<magenta>Skipped<default>: {0}", file);
                }
                else
                {
                    SystemConsole.WriteLine("<cyan>Copy<default>: {0} -> <cyan>{1}<default> ..", file, target);
                    File.Copy(file, target);
                    SystemConsole.WriteLine(" <green>ok");
                }
            }
        }

        private Regex CreateRegex(string mask)
        {
            string valueString = mask;
            bool lastWasWildcard = false;

            StringBuilder sb = new StringBuilder();
            sb.Append('^');
            foreach (char c in valueString)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append("\\/");
                        lastWasWildcard = false;
                        continue;
                    case '*':
                        if (lastWasWildcard)
                        {
                            continue;
                        }

                        lastWasWildcard = true;
                        sb.Append(".*");
                        continue;
                    case '?':
                        sb.Append(".");
                        lastWasWildcard = false;
                        continue;
                    case ' ':
                    case '%':
                    case '+':
                    case '_':
                    case '|':
                    case '/':
                    case '{':
                    case '[':
                    case '(':
                    case ')':
                    case '^':
                    case '$':
                    case '.':
                    case '#':
                        sb.Append('\\');
                        break;
                }
                sb.Append(c);
                lastWasWildcard = false;
            }
            sb.Append('$');
            string s = sb.ToString();
            return new Regex(s, RegexOptions.IgnoreCase);
        }
    }
}

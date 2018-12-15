using System;
using System.Collections.Generic;
using System.IO;
using Cave;
using Cave.Collections;
using Cave.Console;
using Cave.FileSystem;
using Microsoft.Win32;

namespace csObfuscate
{
    class Shell
    {
        static Arguments m_Arguments = Arguments.FromEnvironment();
        static Dictionary<string, string> m_Cache = new Dictionary<string, string>();
        static List<string> files = new List<string>();

        static string ShowSelector(List<string> files)
        {
            SystemConsole.WriteLine("Please select the program to use:");
            string selectors = ASCII.Strings.Digits + ASCII.Strings.UppercaseLetters;
            int i = 0;
            foreach (string file in files)
            {
                string selector = "";
                //get selector
                {
                    int n = ++i;
                    while (n > 0)
                    {
                        selector += selectors[n % selectors.Length];
                        n /= selectors.Length;
                    }
                }
                SystemConsole.WriteLine("  <cyan>{0}<default> {1}", selector, file);
            }
            while (true)
            {
                string selector = SystemConsole.ReadLine();
                SystemConsole.WriteLine();
                i = 0;
                //get number
                foreach (char c in selector)
                {
                    int x = selectors.IndexOf(c);
                    if (x < 0) { i = -1; break; }
                    i = i * selectors.Length + x;
                }
                if (i < 1 || i > files.Count)
                {
                    SystemConsole.WriteLine(string.Format("Selection <red>{0}<default> is invalid!", selector));
                    continue;
                }
                return files[i - 1];
            }
        }

        static bool FindProgramUnix(string name, string description, out string command)
        {
            if (m_Cache.TryGetValue(name, out command))
            {
                return true;
            }

            IniReader reader = IniReader.FromLocation(RootLocation.LocalUserConfig);
            command = reader.ReadSetting("Software", name);
            if (command != null && File.Exists(command))
            {
                if (!m_Arguments.IsOptionPresent("reset-toolchain"))
                {
                    m_Cache[name] = command;
                    return true;
                }
            }

            SystemConsole.WriteLine("\n<yellow>Warning:<default> Build tool <yellow>{0}<default> not set, searching...", description);

            ProcessResult result = ProcessRunner.Run("whereis", name);
            if (result.ExitCode != 0)
            {
                SystemConsole.WriteLine("<red>Cannot find program {0} with whereis!", name);
                SystemConsole.WriteLine(result.Combined);
                return false;
            }
            command = result.StdOut.GetString(-1, ": ", " ");
            SystemConsole.Write("Selected <cyan>{0}<default> for <cyan>{1}<default>...", command, description);

            result = ProcessRunner.Run(command, "");
            if (result.StartException != null)
            {
                SystemConsole.WriteLine(" <red>error");
                SystemConsole.WriteLine(result.Combined);
                return false;
            }
            SystemConsole.WriteLine(" <green>ok");
            m_Cache[name] = command;
            IniWriter writer = new IniWriter(reader);
            writer.WriteSetting("Software", name, command);
            writer.Save();
            return true;
        }

        static bool FindProgramWindows(string name, string description, out string command)
        {
            try
            {
                command = FileSystem.Combine(FileSystem.ProgramDirectory, name + ".exe");
                if (File.Exists(command))
                {
                    return true;
                }
            }
            catch { }

            if (m_Cache.TryGetValue(name, out command))
            {
                return true;
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\CaveSystems\" + name + "\\" + Environment.MachineName))
            {
                command = key.GetValue("Command") as string;
                if (command != null && File.Exists(command))
                {
                    if (!m_Arguments.IsOptionPresent("reset-toolchain"))
                    {
                        m_Cache[name] = command;
                        return true;
                    }
                }

                SystemConsole.WriteLine("\n<yellow>Warning:<default> Build tool <yellow>{0}<default> not set, searching...", description);

                StringComparison comparison = Platform.IsMicrosoft ? StringComparison.InvariantCultureIgnoreCase : StringComparison.Ordinal;
                string path = Environment.GetEnvironmentVariable("PATH");
                files.Clear();

                foreach (string dir in path.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!Directory.Exists(dir))
                    {
                        SystemConsole.WriteLine("<yellow>Warning:<default> Directory <red>{0}<default> (set at PATH variable) does not exist!", dir);
                        continue;
                    }
                    foreach (string prog in Directory.GetFiles(dir, "*.exe"))
                    {
                        if (string.Equals(Path.GetFileName(prog), name, comparison))
                        {
                            ProcessResult result = ProcessRunner.Run(prog, "");
                            if (result.StartException == null)
                            {
                                files.Add(prog);
                            }
                        }
                    }
                }

                if (files.Count == 0)
                {
                    List<FileFinder> finders = new List<FileFinder>
                    {
                        new FileFinder(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "*", name + ".*")
                    };
                    try { finders.Add(new FileFinder(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "*", name + ".*")); } catch { }

                    foreach (FileFinder finder in finders)
                    {
                        finder.FoundFile += Finder_FoundFile;
                        finder.Start();
                    }
                    foreach (FileFinder finder in finders)
                    {
                        finder.Wait();
                        finder.Dispose();
                    }
                    if (files.Count == 0)
                    {
                        SystemConsole.WriteLine("Could not find any working <red>{0}<default> for <red>{1}", name, description);
                        return false;
                    }
                }

                command = files.Count == 1 ? files[0] : ShowSelector(files);
                SystemConsole.WriteLine("Selected <cyan>{0}<default> for <cyan>{1}", command, description);
                m_Cache[name] = command;
                key.SetValue("Command", command);
                return true;
            }
        }

        static void Finder_FoundFile(object sender, FileItemEventArgs e)
        {
            if (e.File == null)
            {
                return;
            }

            ProcessResult result = ProcessRunner.Run(e.File, "");
            if (result.StartException == null)
            {
                files.Add(e.File);
            }
        }

        /// <summary>Searches for a program with the specified filename and runs it with the specified arguments.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="description">The description.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="environmentVariables">The environment variables.</param>
        /// <returns>Returns null if the program cannot be found, a result otherwise.</returns>
        public static ProcessResult FindAndRun(string filename, string description, string arguments, params Option[] environmentVariables)
        {
            //try to find command first, if not possible, just run the command and return the error
            string command;
            switch (Platform.Type)
            {
                case PlatformType.Windows:
                    if (!FindProgramWindows(filename, description, out command))
                    {
                        command = filename;
                    }

                    break;
                case PlatformType.Linux:
                    if (!FindProgramUnix(filename, description, out command))
                    {
                        command = filename;
                    }

                    break;
                default:
                    throw new NotImplementedException(string.Format("Platform {0} not implemented!", Platform.Type));
            }
            return ProcessRunner.Run(command, arguments, environmentVariables);
        }
    }
}

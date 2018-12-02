using Cave;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace csInnoSetup
{
    class csInnoSetup
    {
        static string iscc;

        static bool FindProgramWindows()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\CaveSystems\InnoSetup\" + Environment.MachineName))
            {
                iscc = key.GetValue("Command") as string;
                if (iscc != null && File.Exists(iscc)) return true;

                List<string> paths = new List<string>();
                paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Inno Setup 5"));
                try { paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Inno Setup 5")); } catch { }
                foreach (string path in paths)
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(path, "iscc.exe", SearchOption.AllDirectories))
                        {
                            try
                            {
                                ProcessStartInfo si = new ProcessStartInfo(file);
                                si.UseShellExecute = false;
                                si.RedirectStandardError = true;
                                si.RedirectStandardOutput = true;
                                Process.Start(si).WaitForExit();
                                iscc = file;
                                key.SetValue("Command", iscc);
                                return true;
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine("Cannot use {0}", file);
                                Console.Error.WriteLine(ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Cannot search at path {0}", path);
                        Console.Error.WriteLine(ex.Message);
                    }
                }
                return false;
            }
        }

        static bool FindProgram()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return FindProgramWindows();

                default: return false;
            }
        }

        static int Main(string[] args)
        {
            if (!FindProgram())
            {
                Console.Error.WriteLine("Cannot find Inno Setup Compiler!");
                return 1;
            }
            try
            {
				//check if --debug flag is set -> skip this setup
				if (args.Any(a => a.ToLower() == "--debug")) return 0;
				//remove all flags
				args = args.Where(a => !a.StartsWith("--")).ToArray();
				//call inno
				ProcessStartInfo si = new ProcessStartInfo(iscc, '"' + string.Join('"' + " " + '"', args) + '"');
                si.UseShellExecute = false;
				si.RedirectStandardInput = true;
				si.RedirectStandardOutput = true;
				var result = ProcessRunner.Run(si);
				string str = result.Combined;
				while (str.Contains("\r\n\r\n")) str = str.Replace("\r\n\r\n", "\r\n");
				Console.WriteLine(str);
                return result.ExitCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 2;
            }
        }
    }
}
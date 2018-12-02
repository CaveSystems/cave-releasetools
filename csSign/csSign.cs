using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace csSign
{
    class csSign
    {
        static string SignTool;

        static bool FindSignToolWindows()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\CaveSystems\SignTool\" + Environment.MachineName))
            {
                SignTool = key.GetValue("Command") as string;
                if (SignTool != null && File.Exists(SignTool)) return true;

                List<string> paths = new List<string>();
                paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Kits"));
                try { paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits")); } catch { }
                foreach (string path in paths)
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(path, "signtool.exe", SearchOption.AllDirectories))
                        {
                            try
                            {
                                ProcessStartInfo si = new ProcessStartInfo(file);
                                si.UseShellExecute = false;
                                si.RedirectStandardError = true;
                                si.RedirectStandardOutput = true;
								using (var p = Process.Start(si))
								{
									p.BeginErrorReadLine();
									p.BeginOutputReadLine();
									p.WaitForExit();
								}
								SignTool = file;
                                key.SetValue("Command", SignTool);
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

        static bool FindSignTool()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return FindSignToolWindows();

                default: return false;
            }
        }

        static int Main(string[] args)
        {
            if (!FindSignTool())
            {
                Console.Error.WriteLine("Cannot find signtool!");
                return 1;
            }
            try
            {
                ProcessStartInfo si = new ProcessStartInfo(SignTool, '"' + string.Join('"' + " " + '"', args) + '"');
                si.UseShellExecute = false;
                Process p = Process.Start(si);
                p.WaitForExit();
                if (p.ExitCode == 0) return 0;
                return p.ExitCode + 10;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 2;
            }
        }
    }
}
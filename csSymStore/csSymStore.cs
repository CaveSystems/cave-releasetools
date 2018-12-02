using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace csSymStore
{
	class csSymStore
	{
		static string SymStoreTool;

		static bool FindSignToolWindows()
		{
			using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\CaveSystems\SymStore\" + Environment.MachineName))
			{
				SymStoreTool = key.GetValue("Command") as string;
				if (SymStoreTool != null && File.Exists(SymStoreTool)) return true;

				Console.Error.WriteLine("Search for symstore.exe...");
				List<string> paths = new List<string>();
				try { paths.Add(TestPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Debugging Tools for Windows"))); } catch { }
				try { paths.Add(TestPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Debugging Tools for Windows"))); } catch { }
				try { paths.Add(TestPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Debugging Tools for Windows (x64)"))); } catch { }
				try { paths.Add(TestPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Debugging Tools for Windows (x64)"))); } catch { }
				try { paths.Add(TestPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Kits"))); } catch { }
				try { paths.Add(TestPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits"))); } catch { }
				foreach (string path in paths)
				{
					try
					{
						Console.Error.WriteLine("Check {0}", path);
						foreach (string file in Directory.GetFiles(path, "symstore.exe", SearchOption.AllDirectories))
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
								SymStoreTool = file;
								key.SetValue("Command", SymStoreTool);
								Console.Error.WriteLine("Using {0}", file);
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

		private static string TestPath(string path)
		{
			path = Path.GetFullPath(path);
			if (!Directory.Exists(path)) throw new DirectoryNotFoundException();
			return path;
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
				Console.Error.WriteLine("Cannot find symstore!");
				return 1;
			}

			try
			{
				ProcessStartInfo si = new ProcessStartInfo(SymStoreTool, '"' + string.Join('"' + " " + '"', args) + '"');
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

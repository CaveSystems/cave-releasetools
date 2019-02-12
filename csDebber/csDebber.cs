using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cave;
using Cave.Compression;
using Cave.Compression.Tar;
using Cave.Console;
using Cave.IO;

namespace csDebber
{
    class csDebber
    {
        static void Main(string[] args)
        {
            SystemConsole.WriteLine(
                "CaveSystems Debian Package Builder\n" +
                "(c) 2003-2018 Andreas Rohleder. All rights reserved.\n");

            Arguments arguments = Arguments.FromArray(args);

            if (arguments.Parameters.Count != 1 || arguments.IsHelpOptionFound())
            {
                Help();
                return;
            }

            foreach (string file in arguments.Parameters)
            {
                new csDebber(file).Build();
            }
        }

        private static void Help()
        {
            SystemConsole.WriteLine(
                "csDebber <debber.ini>\n" +
                "\n" +
                "Builds a debian package using the specified debber.ini file.");
        }

        Dictionary<string, string> md5sums = new Dictionary<string, string>();
        IniReader ini;
        SemanticVersion packageVersion;
        string packageFile;
        string packageName;

        csDebber(string file)
        {
            SystemConsole.WriteLine("Building debian package defined at <cyan>{0}", file);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(file));
            ini = IniReader.FromFile(file);
        }

        void Build()
        {
            int debberVer = 0;
            ini.GetValue("debber", "debber-version", ref debberVer);
            if (debberVer != 1)
            {
                throw new Exception("File requires a newer debber version. Please update!");
            }

            string binVer = ini.ReadSetting("debber", "debian-binary");
            if (binVer != "2.0")
            {
                throw new Exception("Invalid or unknown debian version!");
            }

            packageVersion = SemanticVersion.Parse(ini.ReadSetting("package", "version"));
            packageName = ini.ReadSetting("package", "name");
            packageFile = ini.ReadSetting("package", "output").Replace("{NAME}", packageName).Replace("{VERSION}", packageVersion.ToString());
            FileSystem.TouchFile(packageFile);

            SystemConsole.WriteLine("Create <cyan>data.tar.gz<default>...");
            byte[] data_tar_gz;
            using (MemoryStream ms = new MemoryStream())
            {
                TarWriter tarFile = new TarWriter(ms, true);
                foreach (string line in ini.ReadSection("data.tgz", true))
                {
                    string[] parts = line.Split(':');
                    if (parts.Length < 2)
                    {
                        continue;
                    }

                    switch (parts[0].Trim())
                    {
                        case "directory": PackDirectory(tarFile, parts[1]); break;
                        case "file": PackFiles(tarFile, parts); break;
                        default: throw new Exception(string.Format("Invalid setting {0} at [data.tgz]!", line));
                    }
                }
                tarFile.Close();
                data_tar_gz = ms.ToArray();
            }

            SystemConsole.WriteLine("Create <cyan>control.tar.gz<default>...");
            byte[] control_tar_gz;
            using (MemoryStream tarStream = new MemoryStream())
            {
                TarWriter tarFile = new TarWriter(tarStream, true);

                WriteControlFile(tarFile, "control", 640);
                #region md5sums
                using (MemoryStream ms = new MemoryStream())
                {
                    DataWriter writer = new DataWriter(ms);
                    foreach (KeyValuePair<string, string> md5sum in md5sums)
                    {
                        writer.Write(md5sum.Value);
                        writer.Write("  ");
                        writer.Write(md5sum.Key);
                        writer.Write("\n");
                    }
                    writer.Close();
                    SystemConsole.WriteLine("  file: <cyan>md5sums");
                    tarFile.AddFile("md5sums", ms.ToArray());
                }
                #endregion
                WriteControlFile(tarFile, "conffiles", 640);
                WriteControlFile(tarFile, "preinst", 700);
                WriteControlFile(tarFile, "postinst", 700);
                WriteControlFile(tarFile, "prerm", 700);
                WriteControlFile(tarFile, "postrm", 700);

                tarFile.Close();
                control_tar_gz = tarStream.ToArray();
            }

            using (MemoryStream ms = new MemoryStream())
            {
                var arFile = ArFile.CreateNewAr(ms);
                arFile.WriteHeader(ArHeader.CreateFile("debian-binary", 4));
                arFile.WriteData(Encoding.ASCII.GetBytes("2.0\n"));

                arFile.WriteHeader(ArHeader.CreateFile("control.tar.gz", control_tar_gz.Length));
                arFile.WriteData(control_tar_gz);

                arFile.WriteHeader(ArHeader.CreateFile("data.tar.gz", data_tar_gz.Length));
                arFile.WriteData(data_tar_gz);
                arFile.Close();

                File.WriteAllBytes(packageFile, ms.ToArray());
                SystemConsole.WriteLine("Completed <green>{0}<default> ...", packageFile);
            }
        }

        private void WriteControlFile(TarWriter tarFile, string fileName, int fileMode)
        {
            SystemConsole.WriteLine("  file: <cyan>{0}", fileName);
            using (MemoryStream ms = new MemoryStream())
            {
                DataWriter writer = new DataWriter(ms);
                foreach (string line in ini.ReadSection("control.tgz:" + fileName, false))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string text = line.Replace("{NAME}", packageName).Replace("{VERSION}", packageVersion.ToString());
                    writer.WriteLine(text);
                }
                writer.Close();
                byte[] data = ms.ToArray();
                tarFile.AddFile(fileName, data);
            }
        }

        void PackFiles(TarWriter tarFile, string[] parts)
        {
            string tarFilePath = parts[1].Trim();
            string dir = Path.GetDirectoryName(tarFilePath).Replace('\\', '/');
            SystemConsole.WriteLine("  file: <cyan>{0}", tarFilePath);
            byte[] bytes;
            if (parts.Length > 2)
            {
                string fileName = parts[2].Trim();
                bytes = File.ReadAllBytes(fileName);
            }
            else
            {
                string content = ini.ReadSection(tarFilePath, false).Join("\n");
                bytes = Encoding.UTF8.GetBytes(content);
            }
            using (MD5 md5 = MD5.Create())
            {
                string hash = md5.ComputeHash(bytes).ToHexString();
                md5sums.Add(tarFilePath, hash);
            }
            tarFile.AddFile(tarFilePath, bytes);
        }

        void PackDirectory(TarWriter tarFile, string target)
        {
            target = target.Trim();
            SystemConsole.WriteLine("  dir: <cyan>{0}", target);
            foreach (string line in ini.ReadSection(target))
            {
                bool recursive = false;
                string mask = line;
                if (line.Contains(":"))
                {
                    string[] parts = line.Split(new char[] { ':' }, 2);
                    string flag = parts[0].Trim();
                    switch (flag)
                    {
                        case "recursive": recursive = true; break;
                        default: throw new Exception(string.Format("Unknown flag {0} at [{1}]!", flag, target));
                    }
                    mask = parts[1].Trim();
                }
                foreach (FileItem file in FileSystem.FindFiles(mask, recursive: recursive))
                {
                    var dir = CheckDir(Path.GetDirectoryName(file.Relative));
                    dir = Path.Combine(target, dir).Replace('\\', '/');

                    string tarFilePath = Path.Combine(dir, file.Name).Replace('\\', '/');
                    SystemConsole.WriteLine("  file: <cyan>{0}", tarFilePath);
                    byte[] bytes = File.ReadAllBytes(file);
                    using (MD5 md5 = MD5.Create())
                    {
                        string hash = md5.ComputeHash(bytes).ToHexString();
                        md5sums.Add(tarFilePath, hash);
                    }
                    tarFile.AddFile(tarFilePath, bytes);
                }
            }
        }

        string CheckDir(string dir)
        {
            dir = dir.Replace('\\', '/');
            if (dir == ".")
            {
                return "";
            }

            if (dir.StartsWith("./"))
            {
                return dir.Substring(2);
            }

            return dir;
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Cave;
using Cave.Console;

namespace csObfuscate
{
    class Program
    {
        const string Banner = "csObfuscate - Release Obfuscation - (c) 2018 CaveSystems GmbH, Andreas Rohleder";

        public string LogSourceName => "csObfuscate";

        static void Main(string[] args)
        {
            new Program().Run(Arguments.FromArray(args));
        }

        private void Run(Arguments arguments)
        {
            try
            {
                if (arguments.IsHelpOptionFound()) { Help(); return; }
                string signatureFile = arguments.IsOptionPresent("sign") ? arguments.Options["sign"].Value : null;
                bool testLoad = arguments.IsOptionPresent("test");
                bool debug = arguments.IsOptionPresent("debug");
                string obfuscator = arguments.IsOptionPresent("obfuscator") ? arguments.Options["obfuscator"].Value : "obfuscar";
                if (arguments.IsOptionPresent("config"))
                {
                    string config = arguments.Options["config"].Value;
                    if (config.ToUpper() != "RELEASE")
                    {
                        SystemConsole.WriteLine("No obfuscation at configuration <cyan>{0}", config);
                        return;
                    }
                }
                foreach (string file in arguments.Parameters)
                {
                    if (!File.Exists(file))
                    {
                        throw new FileNotFoundException();
                    }

                    Obfuscate(obfuscator, file, signatureFile, testLoad: testLoad, debug: debug);
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    throw;
                }

                SystemConsole.WriteLine(ex.ToXT());
            }
            finally
            {
                if (Debugger.IsAttached || arguments.IsOptionPresent("wait"))
                {
                    SystemConsole.WriteLine("--- press <yellow>enter<default> to exit ---");
                    SystemConsole.ReadLine();
                }
            }
        }

        private void Obfuscate(string obfuscator, string file, string signatureFile, bool testLoad = false, bool debug = false)
        {
            switch (obfuscator.ToLower())
            {
                case "crypto":
                    CryptoObfuscate(file, signatureFile, testLoad: testLoad, debug: debug);
                    return;
                case "obfuscar":
                    Obfuscar(file, signatureFile, testLoad: testLoad, debug: debug);
                    return;
                default:
                    throw new Exception(string.Format("Unknown obfuscator {0}", obfuscator));
            }
        }

        private void Obfuscar(string file, string signatureFile, bool testLoad, bool debug, bool embed = false)
        {
            bool isProgram = Path.GetExtension(file).ToLower() == ".exe";
            if (!isProgram && embed)
            {
                return;
            }

            SystemConsole.Write("Obfuscate <cyan>{0}<default>...", Path.GetFileName(file));
            Stopwatch watch = Stopwatch.StartNew();

            string sourceDir = Path.GetDirectoryName(file);
            string workDir = FileSystem.Combine(sourceDir, "tmp");
            string configFile = FileSystem.Combine(workDir, "obfuscar.xml");
            Directory.CreateDirectory(workDir);

            using (StreamWriter writer = File.CreateText(configFile))
            {
                writer.WriteLine($"<?xml version='1.0'?>");
                writer.WriteLine($"<Obfuscator>");
                writer.WriteLine($"<Var name=\"InPath\" value=\"{sourceDir}\" />");
                writer.WriteLine($"<Var name=\"OutPath\" value=\"{workDir}\" />");
                writer.WriteLine($"<Var name=\"HidePrivateApi\" value=\"true\" />");
                writer.WriteLine($"<Var name=\"KeepPublicApi\" value=\"true\" />");
                writer.WriteLine($"<Var name=\"KeyFile\" value=\"{signatureFile}\" />");
                writer.WriteLine($"<Var name=\"RegenerateDebugInfo\" value=\"true\" />");
                writer.WriteLine($"<Module file=\"{file}\" />");
                if (embed)
                {
                    foreach (string dll in Directory.GetFiles(Path.GetDirectoryName(file), "*.dll"))
                    {
                        writer.WriteLine($"<Module file=\"{dll}\" />");
                    }
                }
                writer.WriteLine($"</Obfuscator>");
            }
            ProcessResult result = Shell.FindAndRun("Obfuscar.Console", "Obfuscar Console", $"-s \"{configFile}\"");
            if (debug)
            {
                foreach (string line in result.Combined.SplitNewLine())
                {
                    if (line.Trim().Length > 0)
                    {
                        SystemConsole.WriteLine("D " + line.Trim());
                    }
                }
            }
            if (result.ExitCode != 0)
            {
                SystemConsole.WriteLine(" {0} <red>error", StringExtensions.FormatTime(watch.Elapsed));
                SystemConsole.WriteLine();
                foreach (string line in result.Combined.SplitNewLine())
                {
                    SystemConsole.WriteLine(new XTItem(XTColor.Red, XTStyle.Default, line));
                }
                throw new Exception("Obfuscation Exception");
            }
            SystemConsole.WriteLine(" {0} <green>ok", StringExtensions.FormatTime(watch.Elapsed));

            ObfucatorComplete(sourceDir, file, embed, testLoad);
        }

        private void ObfucatorComplete(string sourceDir, string file, bool embed, bool testLoad)
        {
            if (embed)
            {
                Stopwatch watch = Stopwatch.StartNew();
                SystemConsole.Write("Clean <magenta>{0}<default>...", sourceDir);
                foreach (string removeFile in Directory.GetFiles(sourceDir, "*.*"))
                {
                    switch (Path.GetExtension(removeFile).ToLowerInvariant())
                    {
                        case ".exe": case ".dll": case ".pdb": File.Delete(removeFile); break;
                    }
                    SystemConsole.WriteLine(" {0} <green>ok", StringExtensions.FormatTime(watch.Elapsed));
                }
            }

            string outFile = FileSystem.Combine(sourceDir, "tmp", Path.GetFileName(file));
            if (testLoad)
            {
                Stopwatch watch = Stopwatch.StartNew();
                SystemConsole.Write("Testing <cyan>{0}<default>...", outFile);
                try
                {
                    Assembly.Load(File.ReadAllBytes(outFile));
                }
                catch
                {
                    SystemConsole.WriteLine(" {0} <red>error", StringExtensions.FormatTime(watch.Elapsed));
                    throw;
                }
                SystemConsole.WriteLine(" {0} <green>ok", StringExtensions.FormatTime(watch.Elapsed));
            }
            File.Delete(file);
            File.Move(FileSystem.Combine(sourceDir, "tmp", outFile), file);

            string originalPdb = Path.ChangeExtension(file, ".pdb");
            if (File.Exists(originalPdb))
            {
                File.Delete(originalPdb);
            }

            string newPdb = FileSystem.Combine(sourceDir, "tmp", Path.ChangeExtension(outFile, ".pdb"));
            if (File.Exists(newPdb))
            {
                File.Move(newPdb, originalPdb);
            }
        }

        private void CryptoObfuscate(string file, string signatureFileName, bool testLoad = false, bool debug = false, bool renameSymbols = false, bool embed = false)
        {
            bool isProgram = Path.GetExtension(file).ToLower() == ".exe";
            if (!isProgram && embed)
            {
                return;
            }

            SystemConsole.Write("Obfuscate <cyan>{0}<default>...", Path.GetFileName(file));
            Stopwatch watch = Stopwatch.StartNew();
            string @namespace = ".";

            string sourceDir = Path.GetDirectoryName(file);
            string workDir = FileSystem.Combine(sourceDir, "tmp");
            string obprojFile = FileSystem.Combine(workDir, Path.GetFileName(file) + ".obproj");
            Directory.CreateDirectory(workDir);

            using (StreamWriter writer = File.CreateText(obprojFile))
            {
                writer.WriteLine($"<Project ProjectFileVersion=\"4\" UseRelativePaths=\"True\" CheckVersionsWhileResolvingAssemblies=\"True\" DisabledFromMSBuild=\"False\" ID=\"" + Guid.NewGuid() + "\">");
                writer.WriteLine($"  <SearchDirectories />");//{Path.GetFileNameWithoutExtension(file)}
                writer.WriteLine($"  <OutputSettings OutputPath=\"{workDir}\" MainAssemblyPath=\"{workDir}\" OverwriteInputFiles=\"False\">");
                writer.WriteLine($"  </OutputSettings>");
                writer.WriteLine($"  <ObfuscationSettings RenamingScheme=\"2\" UseOverloadedFieldNames=\"True\" UseOverloadedMethodNames=\"True\" HonorObfuscationAttributes=\"True\" ExcludeSerializedTypes=\"True\" ProcessInlineStrings=\"True\" ForceXAMLProcessingModeExclude=\"False\" ForceRenameParams=\"False\" EncryptionActive=\"True\" CompressionActive=\"True\" />");
                writer.WriteLine($"  <Assembly Load=\"true\" Path=\"{file}\" KeyFilePath=\"{signatureFileName}\" KeyFileContainsPublicKeyOnly=\"False\" Rfc3161TimestampURL=\"False\" Embed=\"True\" AddExceptionReporting=\"False\" IsWinRTAssembly=\"False\">");
                writer.WriteLine($"    <ObfuscationSettings EncryptStrings=\"True\" EncryptMethods=\"True\" EncryptConstants=\"True\" SuppressReflector=\"{isProgram}\" ReduceMetaData=\"{isProgram}\" ObfuscationDisposition=\"1\" FlowObfuscation=\"1\" CodeMasking=\"0\" SuppressILDASM=\"True\" SuppressReflection=\"False\" CombineResources=\"True\" EncryptResources=\"True\" CompressResources=\"True\" MarkAsSealed=\"{isProgram}\" EnableTamperDetection=\"True\" EnableAntiDebugging=\"{isProgram}\" SymbolRenaming=\"{renameSymbols}\" HideExternalCalls=\"True\" HideInternalCalls=\"True\" GeneratePdbFile=\"False\" ObfuscatePdbFileNames=\"False\" IncludeLocalVariablesInPdbFile=\"True\" Encrypt=\"{isProgram}\" Compress=\"{isProgram}\" MSBuild=\"False\" ObfuscatedNamespace=\"{@namespace}\" RetainNamespace=\"False\" ModuleInitializationMethod=\"\" LicensingMerge=\"False\" RemoveConstants=\"{isProgram}\">");
                writer.WriteLine($"    </ObfuscationSettings>");
                writer.WriteLine($"  </Assembly>");
                if (embed)
                {
                    foreach (string dll in Directory.GetFiles(Path.GetDirectoryName(file), "*.dll"))
                    {
                        writer.WriteLine($"  <Assembly Load=\"true\" Path=\"{dll}\" KeyFilePath=\"{signatureFileName}\" KeyFileContainsPublicKeyOnly=\"False\" Rfc3161TimestampURL=\"False\" Embed=\"True\" AddExceptionReporting=\"False\" IsWinRTAssembly=\"False\">");
                        writer.WriteLine($"    <ObfuscationSettings EncryptStrings=\"True\" EncryptMethods=\"True\" EncryptConstants=\"True\" SuppressReflector=\"{isProgram}\" ReduceMetaData=\"{isProgram}\" ObfuscationDisposition=\"1\" FlowObfuscation=\"1\" CodeMasking=\"0\" SuppressILDASM=\"True\" SuppressReflection=\"False\" CombineResources=\"True\" EncryptResources=\"True\" CompressResources=\"True\" MarkAsSealed=\"{isProgram}\" EnableTamperDetection=\"True\" EnableAntiDebugging=\"{isProgram}\" SymbolRenaming=\"{renameSymbols}\" HideExternalCalls=\"True\" HideInternalCalls=\"True\" GeneratePdbFile=\"False\" ObfuscatePdbFileNames=\"False\" IncludeLocalVariablesInPdbFile=\"True\" Encrypt=\"{isProgram}\" Compress=\"{isProgram}\" MSBuild=\"False\" ObfuscatedNamespace=\"{@namespace}\" RetainNamespace=\"False\" ModuleInitializationMethod=\"\" LicensingMerge=\"False\" RemoveConstants=\"{isProgram}\">");
                        writer.WriteLine($"    </ObfuscationSettings>");
                        writer.WriteLine($"  </Assembly>");
                    }
                }
                writer.WriteLine($"  <Warnings SaveWarningsToFile=\"\" FailOnUnsuppressedWarning=\"False\" SaveSuppressedWarnings=\"False\" SaveUnsuppressedWarnings=\"False\" />");
                writer.WriteLine($"</Project>");
            }
            ProcessResult result = Shell.FindAndRun("co", "Crypto Obfuscator", $"projectfile={obprojFile}");
            if (debug)
            {
                foreach (string line in result.Combined.SplitNewLine())
                {
                    if (line.Trim().Length > 0)
                    {
                        SystemConsole.WriteLine("D " + line.Trim());
                    }
                }
            }
            if (result.ExitCode != 0)
            {
                SystemConsole.WriteLine(" {0} <red>error", StringExtensions.FormatTime(watch.Elapsed));
                SystemConsole.WriteLine();
                foreach (string line in result.Combined.SplitNewLine())
                {
                    SystemConsole.WriteLine(new XTItem(XTColor.Red, XTStyle.Default, line));
                }
                throw new Exception("Obfuscation Exception");
            }
            SystemConsole.WriteLine(" {0} <green>ok", StringExtensions.FormatTime(watch.Elapsed));

            ObfucatorComplete(sourceDir, file, embed, testLoad);
        }

        void Help()
        {
            SystemConsole.WriteLine(
                Banner +
                "\n" +
                "csObfuscate [<cyan>--config=<value><default>] [<cyan>--sign=<filename.snk><default>] [<cyan>--option<default>] <magenta>/path/to/file.dll\n" +
                "Obfuscates a release assembly\n" +
                "\n" +
                "  <cyan>--config=<value><default>:       if set to anything else than release nothing will be done.\n" +
                "  <cyan>--sign=<filename.snk><default>:  if set the assembly will be signed after obfuscation.\n" +
                "  <cyan>--test<default>:                 if set test load the assembly after obfuscation.\n" +
                "  <cyan>--obfuscator=<type><default>:    use <cyan>crypto<default> or <cyan>obfuscar<default> (default).\n" +
                "\n");
        }
    }
}

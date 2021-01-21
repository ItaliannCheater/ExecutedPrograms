using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExecutedPrograms {
    class Program {

        class ProgramInfo {
            public ProgramInfo(string fileName, DateTime? lastModified,
                DateTime? createdOn, double size) {
                this.FileName = fileName;
                this.LastModified = lastModified;
                this.CreatedOn = createdOn;
                this.Size = size;
            }

            public string FileName { get; }
            public DateTime? LastModified { get; }
            public DateTime? CreatedOn { get; }
            public double Size { get; }
        }

        enum Order {
            FileName,
            LastModified,
            CreatedOn,
            Size,
            Random
        }

        static HashSet<string> programs = new HashSet<string>();

        static void Main(string[] args) {
            List<ProgramInfo> programsInfo = new List<ProgramInfo>();
            Order orderFlag = Order.Random;
            bool save = false; string savePath = string.Empty;

            FileStream fstream = new FileStream(@"C:\tmpout.txt",
                FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fstream);
            TextWriter oldOut = Console.Out;

            if (args.Length > 0) {
                if (args.Contains("-orderby")) {
                    int index = Array.IndexOf(args, "-orderby");
                    string argValue = args[index + 1].ToLower();
                    if (argValue != "filename" && argValue != "lastmodified"
                        && argValue != "createdon" && argValue != "size") {
                        Console.WriteLine("[!] Missing argument value: " +
                        "(filename|lastmodified|createdon|size");
                        Environment.Exit(-1);
                    }
                    if (argValue == "filename")
                        orderFlag = Order.FileName;
                    else if (argValue == "lastmodified")
                        orderFlag = Order.LastModified;
                    else if (argValue == "createdon")
                        orderFlag = Order.CreatedOn;
                    else if (argValue == "size")
                        orderFlag = Order.Size;
                    else {
                        Console.WriteLine("[!] Invalid argument value");
                        Environment.Exit(-1);
                    }
                }

                if (args.Contains("-save")) {
                    int index = Array.IndexOf(args, "-save");
                    save = true; savePath = args[index + 1];
                }
            }

            getMuiCache(); getStore(); // Retrieves programs from registry

            foreach (string p in programs)
                programsInfo.Add(getProgramInfo(p));

            if (orderFlag == Order.FileName)
                programsInfo = programsInfo.OrderBy(p => p.FileName).ToList();
            if (orderFlag == Order.LastModified)
                programsInfo = programsInfo.OrderBy(p => p.LastModified).ToList();
            if (orderFlag == Order.CreatedOn)
                programsInfo = programsInfo.OrderBy(p => p.CreatedOn).ToList();
            if (orderFlag == Order.Size)
                programsInfo = programsInfo.OrderBy(p => p.Size).ToList();

            if (save) {
                fstream = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(fstream);
                Console.SetOut(writer);
            }

            Console.WriteLine("ExecutedPrograms by @italianncheater\n\n");

            foreach (ProgramInfo info in programsInfo) {
                string lastModified, createdOn, size;

                if (info.LastModified == null) lastModified = string.Empty;
                else lastModified = info.LastModified.ToString();

                if (info.CreatedOn == null) createdOn = string.Empty;
                else createdOn = info.CreatedOn.ToString();

                if (info.Size == 0) size = string.Empty;
                else size = info.Size.ToString() + "MB";

                Console.WriteLine("File: {0}\n => Last modified: {1}\n" +
                    " => Created on: {2}\n => Size: {3}",
                    info.FileName, lastModified, createdOn, size);
                Console.WriteLine("\n\n");
            }

            Console.SetOut(oldOut); writer.Close(); fstream.Close();

            if (!save) Console.ReadLine();
        }

        static void getMuiCache() {
            Regex rgx = new Regex(@"^(\w:\\.+.exe)(.FriendlyAppName|.ApplicationCompany)$");
            RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache");
            string[] values = key.GetValueNames();

            foreach (string v in values) {
                Match match = rgx.Match(v);
                if (!match.Success) continue;

                string program = match.Groups[1].Value;
                programs.Add(program);
            }
        }

        static void getStore() {
            Regex rgx = new Regex(@"^\w:\\.+.exe$");
            RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store");
            string[] values = key.GetValueNames();

            foreach (string v in values) {
                if (!Char.IsUpper(v[0])) continue;

                Match match = rgx.Match(v);
                if (!match.Success) continue;

                string program = match.Groups[0].Value;
                programs.Add(program);
            }
        }

        static ProgramInfo getProgramInfo(string fileName) {
            DateTime? lastModified = null, createdOn = null;
            double megaBytes = 0;
            FileInfo program = new FileInfo(fileName);

            if (program.Exists) {
                lastModified = program.LastWriteTime;
                createdOn = program.CreationTime;
                megaBytes = program.Length / 1048576d; // 1048576 = num of bytes in a megabyte
            }

            return new ProgramInfo(fileName, lastModified, createdOn, Math.Round(megaBytes, 2));
        }
    }
}

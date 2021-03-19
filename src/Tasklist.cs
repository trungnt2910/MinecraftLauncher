using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public static class Tasklist
    {
        //Somehow, the existing C# function does not work well.
        public static List<Process> GetProcessesByImageName(string name)
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "tasklist",
                Arguments = $"/FI \"IMAGENAME eq {name}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            proc.WaitForExit();
            if (proc.ExitCode != 0) throw new Exception($"tasklist failed with the return value: {proc.ExitCode}. Standard Error: {proc.StandardError.ReadToEnd()}");

            var outputStream = proc.StandardOutput;

            var result = ParseBoard(outputStream);

            var processes = new List<Process>();

            foreach (var r in result)
            {
                processes.Add(Process.GetProcessById(int.Parse(r[1])));
            }

            return processes;
        }

        public static List<(Process, string)> GetUwpAppsByImageName(string name)
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "tasklist",
                Arguments = $"/APPS /FI \"IMAGENAME eq {name}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            proc.WaitForExit();
            if (proc.ExitCode != 0) throw new Exception($"tasklist failed with the return value: {proc.ExitCode}. Standard Error: {proc.StandardError.ReadToEnd()}");

            var outputStream = proc.StandardOutput;

            var result = ParseBoard(outputStream);

            var processes = new List<(Process, string)>();

            foreach (var r in result)
            {
                processes.Add((Process.GetProcessById(int.Parse(r[1])), r[3]));
            }

            return processes;
        }

        private static List<List<string>> ParseBoard(StreamReader tw)
        {
            List<List<string>> result = new List<List<string>>();
            List<int> ColumnInfo = new List<int>();
            while (!tw.EndOfStream)
            {
                var line = tw.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                foreach (char ch in line)
                {
                    if (!(char.IsWhiteSpace(ch) || ch == '='))
                    {
                        goto bad;
                    }
                }
                for (int i = 0; i < line.Length; ++i)
                {
                    int j = line.IndexOf(' ', i);
                    if (j == -1) j = line.Length;
                    ColumnInfo.Add(j - i);
                    i = j;
                }
                break;
            bad: continue;
            }

            if (ColumnInfo.Count == 0) return result;

            while (!tw.EndOfStream)
            {
                List<string> attributes = new List<string>();
                var line = tw.ReadLine();
                if (line.Length == 0) continue;
                foreach (var info in ColumnInfo)
                {
                    var data = line.Substring(0, Math.Min(info, line.Length)).Trim();
                    if (line.Length > info + 1) line = line.Substring(info + 1);
                    attributes.Add(data);
                }
                result.Add(attributes);
            }

            return result;
        }
    }
}

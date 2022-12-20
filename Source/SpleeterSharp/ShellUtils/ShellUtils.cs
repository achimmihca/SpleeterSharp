using System.Diagnostics;
using System.Text;
using System;

namespace SpleeterSharp
{
    internal static class ShellUtils
    {
        public static ShellExecutionResult Execute(
            string cmd,
            Action<string> stdErrDataReceivedCallback = null,
            Action<string> stdOutDataReceivedCallback = null)
        {
            SpleeterSharpConfig.Config.LogAction?.Invoke($"Will execute: {cmd}");

            string escapedArgs = SpleeterSharpConfig.Config.IsWindows
                ? cmd
                : cmd.Replace("\"", "\\\"");
            StringBuilder outputBuilder = new StringBuilder();
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = SpleeterSharpConfig.Config.IsWindows
                        ? "cmd.exe"
                        : "/bin/bash",
                    Arguments = SpleeterSharpConfig.Config.IsWindows
                        ? $"/C \"{escapedArgs}\""
                        : $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };
            process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    stdErrDataReceivedCallback?.Invoke(e.Data);
                }

                if (stdErrDataReceivedCallback == null)
                {
                    SpleeterSharpConfig.Config.LogAction?.Invoke(e.Data);
                }

                outputBuilder.AppendLine(e.Data);
            };
            process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    stdOutDataReceivedCallback?.Invoke(e.Data);
                }

                if (stdOutDataReceivedCallback == null)
                {
                    SpleeterSharpConfig.Config.LogAction?.Invoke(e.Data);
                }

                outputBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.CancelOutputRead();
            process.CancelErrorRead();

            return new ShellExecutionResult()
            {
                ExitCode = process.ExitCode,
                Output = outputBuilder.ToString()
            };
        }
    }
}

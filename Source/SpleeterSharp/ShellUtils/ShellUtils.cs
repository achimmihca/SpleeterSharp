using System.Diagnostics;
using System.Text;
using System;

namespace SpleeterSharp
{
    internal static class ShellUtils
    {
        /**
         * Executes a command using cmd.exe on Windows or bash on Linux and macOS.
         */
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
                if (string.IsNullOrWhiteSpace(e.Data))
                {
                    return;
                }

                if (stdErrDataReceivedCallback == null)
                {
                    SpleeterSharpConfig.Config.LogAction?.Invoke(e.Data);
                }
                else
                {
                    stdErrDataReceivedCallback.Invoke(e.Data);
                }

                outputBuilder.AppendLine(e.Data);
            };
            process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
            {
                if (string.IsNullOrWhiteSpace(e.Data))
                {
                    return;
                }

                if (stdOutDataReceivedCallback == null)
                {
                    SpleeterSharpConfig.Config.LogAction?.Invoke(e.Data);
                }
                else
                {
                    stdOutDataReceivedCallback.Invoke(e.Data);
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

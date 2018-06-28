using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;

namespace Dwragge.RCloneClient.Common
{
    public class RCloneService : IRCloneService
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task ExecuteCommand(string commandString, Action<string> outputReceivedAction = null)
        {
            var process = CreateProcess(commandString, outputReceivedAction);
            var exitCode = await ExecuteCommandCoreAsync(process);
            if (exitCode == 1)
            {
                throw new InvalidOperationException("Failed to execute command, syntax was invalid");
            }

           if (exitCode != 0)
            {
                throw new InvalidOperationException($"Failed to execute command, exited with code {exitCode}");
            }
        }

        public async Task<IEnumerable<string>> GetRemotes()
        {
            var commandOutput = await ExecuteCommandReaderAsync("listremotes");
            return commandOutput.Split('\r');
        }

        private async Task<string> ExecuteCommandReaderAsync(string commandString)
        {
            var outputLines = new List<string>();
            var process = CreateProcess(commandString, str => outputLines.Add(str));
            
            var exitCode = await ExecuteCommandCoreAsync(process);
            if (exitCode != 0)
            {
                _logger.Error($"Failed to execute command, exit code was {exitCode}");
                return null;
            }

            return string.Join("\n", outputLines);
        }

        private async Task<int> ExecuteCommandCoreAsync(Process process)
        {
            _logger.Info($"Executing command {process.StartInfo.Arguments}");
            var startTime = DateTime.Now;
            
            if (!process.Start())
            {
                _logger.Error("Failed to start process");
                return -1;
            }
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var exitCode = await process.WaitForExitAsync();

            _logger.Info($"Command took {(DateTime.Now - startTime).TotalSeconds } seconds to execute");
            return exitCode;
        }

        private Process CreateProcess(string commandString, Action<string> outputReceivedAction = null)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("rclone", commandString)
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data)) return;
                if (args.Data.Contains("DEBUG"))
                {
                    var substring = args.Data.Substring(args.Data.IndexOf("DEBUG :", StringComparison.Ordinal) + 8);
                    if (string.IsNullOrEmpty(substring)) return;

                    _logger.Debug(substring);
                }

                if (args.Data.Contains("ERROR"))
                {
                    var substring = args.Data.Substring(args.Data.IndexOf("ERROR :", StringComparison.Ordinal) + 8);
                    if (string.IsNullOrEmpty(substring)) return;

                    _logger.Error(substring);
                }

                if (args.Data.Contains("INFO"))
                {
                    var substring = args.Data.Substring(args.Data.IndexOf("INFO  :", StringComparison.Ordinal) + 8);
                    if (string.IsNullOrEmpty(substring)) return;

                    _logger.Info(substring);
                }

                outputReceivedAction?.Invoke(args.Data);
            };

            process.OutputDataReceived += (sender, args) =>
            {
                if (string.IsNullOrEmpty(args.Data)) return;
                outputReceivedAction?.Invoke(args.Data);
            };

            return process;
        }
    }
}

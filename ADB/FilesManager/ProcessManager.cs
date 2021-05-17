using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ADB.FilesManager
{
    public class ProcessManager
    {
        private static int? _processId;

        public static int ProcessId
        {
            get
            {
                if (_processId == null)
                {
                    using (var thisProcess = System.Diagnostics.Process.GetCurrentProcess())
                    {
                        _processId = thisProcess.Id;
                    }
                }
                return _processId.Value;
            }
        }
        public static void LaunchLavaLink()
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (ProcessPort p in ProcessPorts.ProcessPortMap.FindAll(x => x.PortNumber == 2333))
                    {
                        Debug.WriteLine("port 2333 : " + p.ProcessId);
                        KillProcess(p.ProcessId);
                    }
                }
                catch
                {
                    Debug.WriteLine("GET process error");
                }

                var processInfo = new ProcessStartInfo("Launch.bat");
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;

                SetupBot.LogPopup($"[LAVALINK FileName]\t" + processInfo.FileName);

                _processId = Process.GetCurrentProcess().Id;
                SetupBot.LogPopup($"[LAVALINK PID]\t" + _processId);

                var process = Process.Start(processInfo);

                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    //SetupBot.LogPopup($"[LAVALINK]\t" + e.Data);
                };

                process.BeginOutputReadLine();

                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    SetupBot.LogPopup($"[LAVALINK ERROR]\t" + e.Data);
                };
                process.BeginErrorReadLine();

                process.WaitForExit();

                SetupBot.LogPopup($"[LAVALINK EXITCODE]\t" + process.ExitCode);

                process.Close();
            });
        }

        public static void KillProcess(int pid)
        {
            Process[] process = Process.GetProcesses();

            foreach (Process prs in process)
            {
                if (prs.Id == pid)
                {
                    prs.Kill();
                    break;
                }
            }
        }
    }
}
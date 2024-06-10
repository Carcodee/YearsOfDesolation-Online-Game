////////////////////////////////////////
//    Shared Editor Tool Utilities    //
//    by Kris Development             //
////////////////////////////////////////

//License: MIT
//GitLab: https://gitlab.com/KrisDevelopment/SETUtil

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SETUtil.CommandLine
{
    public static class CmdUtils
    {
        /// <summary>
        /// Run a command and capture output
        /// </summary>
        public static string Run(string command, string path = null, int timeout = 1800000, IEnumerator<string> input = null, bool redirectOutput = true)
        {

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            var _fileName = "cmd.exe";
            var _arguments = $"/C {command}";
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            var _fileName = "/bin/bash";
            var _arguments = $"-c '{command}'";
#else
                    UnityEngine.Debug.Log("Can't run command, unsupported platform.");
                    return string.Empty;
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
            string _output = string.Empty;
            var _dir = path ?? Directory.GetCurrentDirectory();

            using (var _proc = new Process())
            {
                _proc.StartInfo = new ProcessStartInfo()
                {
                    FileName = _fileName,
                    Arguments = _arguments,
                    WorkingDirectory = _dir,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized,
                    CreateNoWindow = true,
                    RedirectStandardOutput = redirectOutput,
                    RedirectStandardError = redirectOutput,
                    UseShellExecute = !redirectOutput,
                    RedirectStandardInput = input != null,
                };

                _proc.Start();
                do
                {
                    _proc.WaitForExit(timeout);
                    if (_proc.HasExited)
                    {
                        break;
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"[{DateTime.Now}] Timed out.");
                        if (input?.Current != null)
                        {
                            UnityEngine.Debug.Log($"[{DateTime.Now}] Trying provided input stream.");
                        }
                        else
                        {
                            _proc.Close();
                        }
                    }

                    if (input?.Current != null)
                    {
                        _proc.StandardInput.WriteLine(input.Current);
                    }
                } while (input != null && input.MoveNext());

                if (redirectOutput)
                {
                    _output = _proc.StandardOutput.ReadToEnd();
                }

                if (string.IsNullOrEmpty(_output) && _proc.ExitCode != 0)
                {
                    _output = $"Process exited with code {_proc.ExitCode} (ERROR)";
                    if (redirectOutput)
                    {
                        _output += $"\n{ _proc.StandardError.ReadToEnd()}";
                    }
                }
            }
            return _output;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Run a command, capture output and show log window when done. EDITOR ONLY.
        /// </summary>
        public static string RunAndShowLog(string command, string path = null, string label = "Command Output", int timeout = 1800000, IEnumerator<string> input = null)
        {
            System.Text.StringBuilder _log = new System.Text.StringBuilder();
            _log.AppendLine("--- Running command ---");
            _log.AppendLine($">>> {command}");
            _log.AppendLine("---");
            var _output = Run(command, path, timeout, input, true);
            _log.AppendLine(_output);
            _log.AppendLine("--- END ---");

            SETUtil.EditorUtil.ShowOperationLogWindow(label, _log);
            return _output;
        }
#endif
    }
}
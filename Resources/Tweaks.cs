﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Programmka
{
    public static class Methods
    {
        public static void RunInCMD(string[] commands)
        {
            foreach (var command in commands)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process();
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
        }
        public static async Task RunInCMD(string command, bool visible = false)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = !visible
            };

            using var process = new Process();
            process.StartInfo = startInfo;

            var processCompletion = new TaskCompletionSource<bool>();

            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => processCompletion.TrySetResult(true);

            process.Start();

            await processCompletion.Task;
        }
        public static void RunInPowerShell(string command, string arguments = "")
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"{arguments} -Command \"{command}\"",
                UseShellExecute = true,
                Verb = "runas"
            });
        }
        #region cleanup
        public static readonly System.Collections.Generic.List<string> AllTempPath =
    [
        @"C:\Windows\Temp",
        @"C:\Windows\SoftwareDistribution",
        @"C:\Windows\Prefetch",
        @"C:\Users\mr-4e\AppData\Local\Temp",
        @"C:\$Recycle.Bin"
    ];
        private static long GetFolderSize(string folderPath)
        {
            long folderSize = 0;
            if (!Directory.Exists(folderPath))
            {
                return folderSize;
            }
            foreach (string file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    FileInfo fileInfo = new(file);
                    folderSize += fileInfo.Length;
                }
                catch (Exception) { }
            }
            return folderSize;
        }
        public static long GetFullTempSize()
        {
            long tempSize = 0;
            foreach (string path in AllTempPath)
            {
                tempSize += GetFolderSize(path);
            }
            return tempSize;
        }
        public static string NormalizeByteSyze(long size)
        {
            string[] sizes = ["B", "KB", "MB", "GB", "TB"];
            float normalSize = size;
            var order = 0;
            while (normalSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                normalSize /= 1024;
            }
            return $"{normalSize:0.##}" + sizes[order];
        }
        public static void CleanFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) { return; }
            foreach (string file in Directory.GetFiles(folderPath))
            {
                try { File.Delete(file); }
                catch (Exception) { }
            }

            foreach (string directory in Directory.GetDirectories(folderPath))
            {
                try { Directory.Delete(directory, true); }
                catch (Exception) { }
            }
        }
        #endregion

        #region Register
        /// <summary>
        /// RegistryHive.CurrentUser or RegistryHive.LocalMachine
        /// </summary>
        public static bool ContainsReg(RegistryHive registryHive, string subkey, string key)
        {
            RegistryKey baseKey = (object)registryHive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => null
            };

            using var hkey = baseKey.OpenSubKey(subkey);
            return hkey?.GetValue(key) != null;
        }
        /// <summary>
        /// RegistryHive.CurrentUser or RegistryHive.LocalMachine ||| value = int/DWORD
        /// </summary>
        public static bool ContainsRegValue(RegistryHive registryHive, string subkey, string key, int iValue = 0, string sValue = "")
        {
            RegistryKey baseKey = (object)registryHive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => null
            };

            using var target = baseKey.OpenSubKey(subkey);
            if (target != null)
            {
                var targetValue = target.GetValue(key);
                if (string.IsNullOrEmpty(sValue))
                {
                    if (targetValue is int intValue)
                    {
                        return intValue == iValue;
                    }
                }
                else
                {
                    if (targetValue is string strValue)
                    {
                        return strValue == sValue;
                    }
                }
            }
            return false;
        }
        public static void DeleteReg(string subkey, string key)
        {
            using var hKey = Registry.LocalMachine.OpenSubKey(subkey, true);
            hKey?.DeleteValue(key);
        }
        public static void CreateReg(RegistryHive registryHive, string subkey, string key, string dir = "", string sValue = "", int iValue = 0)
        {
            RegistryKey baseKey = (object)registryHive switch
            {
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                _ => null
            };
            using var target = baseKey.CreateSubKey(subkey);
            if (target == null) return;
            var registryKey = string.IsNullOrEmpty(dir) ? target : target.CreateSubKey(dir, true);
            if (string.IsNullOrEmpty(sValue))
            {
                registryKey?.SetValue(key, iValue, RegistryValueKind.DWord);
            }
            else
            {
                registryKey?.SetValue(key, sValue, RegistryValueKind.String);
            }
        }
        public static void DeleteRegDir(string subkey, string dir)
        {
            using var key = Registry.LocalMachine.CreateSubKey(subkey, true);
            key?.DeleteSubKeyTree(dir, false);
        }
        public static void CreateRegDir(string subkey)
        {
            Registry.LocalMachine.CreateSubKey(subkey);
        }
        public static void SetWallpaperCompressionQuality(int quality)
        {
            const string subKey = @"Control Panel\Desktop";

            using var key = Registry.CurrentUser.OpenSubKey(subKey, writable: true);
            if (key != null)
            {
                key.SetValue("JPEGImportQuality", quality, RegistryValueKind.DWord);
            }
            else
            {
                using var newKey = Registry.CurrentUser.CreateSubKey(subKey);
                newKey?.SetValue("JPEGImportQuality", quality, RegistryValueKind.DWord);
            }
        }
        #endregion
    }
}
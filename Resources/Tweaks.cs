using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Programmka
{
    public class Methods
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
        public static async Task RunInCMD(string command)
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

            var processCompletion = new TaskCompletionSource<bool>();

            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) =>
            {
                processCompletion.TrySetResult(true);
            };

            process.Start();

            await processCompletion.Task;
        }
        public static string OpenFolderDialog()
        {
            using var dialog = new Ookii.Dialogs.WinForms.VistaFolderBrowserDialog();
            dialog.Description = "Выберите папку, в которую установлен WINRAR";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            return null;
        }

        #region Register
        /// <summary>
        /// For Registry.LocalMachine
        /// </summary>
        public static bool ContainsReg(string subkey, string key)
        {
            using var hkey = Registry.LocalMachine.OpenSubKey(subkey);
            return hkey?.GetValue(key) != null;
        }
        
        /// <summary>
        /// For Registry.CurrentUser;   value = int
        /// </summary>
        public static bool ContainsRegValue(string subkey, string key, int value)
        {
            using var target = Registry.CurrentUser.OpenSubKey(subkey);
            if (target != null)
            {
                var targetValue = target.GetValue(key);
                if (targetValue is int intValue)
                {
                    return intValue != value;
                }
            }
            return true;
        }
        public static void DeleteReg(string subkey, string key)
        {
            using var hKey = Registry.LocalMachine.OpenSubKey(subkey, true);
            hKey?.DeleteValue(key);
        }
        public static void CreateReg(string subkey, string key, string dir = "", string value = "")
        {
            using var target = Registry.LocalMachine.CreateSubKey(subkey);
            if (target == null) return;
            var registryKey = string.IsNullOrEmpty(dir) ? target : target.CreateSubKey(dir, true);
            registryKey?.SetValue(key, value, RegistryValueKind.String);
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
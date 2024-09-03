using Microsoft.Win32;
using System.Diagnostics;

namespace Programmka
{
    public class Functions
    {
        public void SetWallpaperCompressionQuality(int quality)
        {
            const string subKey = @"Control Panel\Desktop";

            using (var key = Registry.CurrentUser.OpenSubKey(subKey, writable: true))
            {
                if (key != null)
                {
                    key.SetValue("JPEGImportQuality", quality, RegistryValueKind.DWord);
                }
                else
                {
                    using (var newKey = Registry.CurrentUser.CreateSubKey(subKey))
                    {
                        newKey?.SetValue("JPEGImportQuality", quality, RegistryValueKind.DWord);
                    }
                }
            }
        }

        public void SetNetworkIcon(string[] commands)
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

                using (var process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
            }
        }
    }
}

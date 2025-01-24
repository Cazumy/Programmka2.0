using Microsoft.Win32;
using Programmka.Resources;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Programmka;
public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
{
    private bool _initializingLabels = true,
        _initializingDisk = true,
        _initializingAccess = true,
        _initializingObjects = true,
        _initializingNetwork = true,
        _initializngWallpaper = true;
    public MainWindow()
    {
        InitializeComponent();
        InitializeCheckBoxes();
    }
    private void MainWindow_Loaded(object senter, RoutedEventArgs e)
    {
        _initializingLabels = _initializingDisk =
            _initializingAccess = _initializingObjects = _initializingNetwork = _initializngWallpaper = false;
    }
    public void InitializeCheckBoxes()
    {
        DiskDuplicateButton.IsChecked = CheckDuplicate();
        LabelArrowsButton.IsChecked = CheckLabels();
        QuickAccessButton.IsChecked = CheckQuickAccess();
        Objects3DButton.IsChecked = Check3DObjects();
        NetworkIconButton.IsChecked = CheckNetworkIcon();
        WallpaperCompressionButton.IsChecked = CheckWallpaperCompression();
    }
    #region checks for checkboxes

    private static bool CheckDuplicate()
    {
        return Registry.LocalMachine.OpenSubKey(
                   @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders\{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}") != null;
    }
    private static bool CheckLabels()
    {
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons";

        using var hKey = Registry.LocalMachine.OpenSubKey(subKey);
        var value = hKey?.GetValue("29");
        if (value != null) return false;
        return true;
    }
    private static bool CheckQuickAccess()
    {
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";

        using var hKey = Registry.LocalMachine.OpenSubKey(subKey);
        var value = hKey?.GetValue("HubMode");
        if (value != null) return false;
        return true;
    }
    private static bool Check3DObjects()
    {
        const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";

        using var hKey = Registry.LocalMachine.OpenSubKey(keyPath);
        if (hKey != null) return true;

        return false;
    }
    private static bool CheckNetworkIcon()
    {
        bool attributes = true;
        bool isPinned = true;
        using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\CLSID\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\ShellFolder"))
        {
            if (key != null)
            {
                var value = key.GetValue("Attributes");
                if (value is int intValue)
                {
                    attributes = (uint)intValue != 0xb0940064U;
                }
            }

        }
        using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\CLSID\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}"))
        {
            if (key != null)
            {
                var value = key.GetValue("System.IsPinnedtoNameSpaceTree");
                if (value is int intValue)
                {
                    isPinned = intValue != 0;
                }
            }
        }
        return attributes && isPinned;
    }
    private static bool CheckWallpaperCompression()
    {
        const string subKey = @"Control Panel\Desktop";
        using var key = Registry.CurrentUser.OpenSubKey(subKey);
        if (key != null)
        {
            var value = key.GetValue("JPEGImportQuality");
            if (value is int intValue)
            {
                return intValue != 256;
            }
        }
        return true;
    }
    #endregion
    #region tweaks
    #region explorer tweaks
    private void RemoveDiskDuplicate(object sender, RoutedEventArgs e)
    {
        if (_initializingDisk) return;
        const string subKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders";
        const string keyName = "{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}";
        using (var key = Registry.LocalMachine.CreateSubKey(subKeyPath, true))
        {
            key?.DeleteSubKeyTree(keyName, false);
        }
        AnimateButton(DiskDuplicateButton);
    }
    private void ReturnDiskDuplicate(object sender, RoutedEventArgs e)
    {
        if (_initializingDisk) return;
        const string subKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders";
        const string keyName = "{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}";
        const string valueData = "Removable Drives";
        using (var key = Registry.LocalMachine.CreateSubKey(subKeyPath))
        {
            if (key == null) return;
            var newKey = key.OpenSubKey(keyName, true) ?? key.CreateSubKey(keyName);
            newKey?.SetValue("", valueData, RegistryValueKind.String);
        }
        AnimateButton(DiskDuplicateButton);
    }
    private void RemoveQuickAccess(object sender, RoutedEventArgs e)
    {
        if (_initializingAccess) return;
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";
        using (var hKey = Registry.LocalMachine.CreateSubKey(subKey))
        {
            hKey?.SetValue("HubMode", 1, RegistryValueKind.String);
        }
        AnimateButton(QuickAccessButton);
    }
    private void ReturnQuickAccess(object sender, RoutedEventArgs e)
    {
        if (_initializingAccess) return;
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";
        using (var hKey = Registry.LocalMachine.OpenSubKey(subKey, writable: true))
        {
            hKey?.DeleteValue("HubMode");
        }
        AnimateButton(QuickAccessButton);
    }
    private void RemoveObjects3D(object sender, RoutedEventArgs e)
    {
        if (_initializingObjects) return;
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";
        Registry.LocalMachine.DeleteSubKeyTree(subKey, throwOnMissingSubKey: false);
        AnimateButton(Objects3DButton);
    }
    private void ReturnObjects3D(object sender, RoutedEventArgs e)
    {
        if (_initializingObjects) return;
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";
        Registry.LocalMachine.CreateSubKey(subKey);
        AnimateButton(Objects3DButton);
    }
    private void RemoveNetworkIcon(object sender, RoutedEventArgs e)
    {
        if (_initializingNetwork) return;
        AnimateButton(NetworkIconButton);
        var commands = new[]
        {
            "reg add \"HKCU\\Software\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0940064\" /f >nul 2>&1",
            "reg add \"HKCU\\Software\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /t REG_DWORD /d \"0\" /f >nul 2>&1",
            "reg add \"HKCU\\Software\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0940064\" /f >nul",
            "reg add \"HKCU\\Software\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /t REG_DWORD /d \"0\" /f >nul 2>&1",
            "TI.exe cmd.exe /c reg add \"HKLM\\SOFTWARE\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0040064\" /f >nul 2>&1",
            "TI.exe cmd.exe /c reg delete \"HKLM\\SOFTWARE\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
            "TI.exe cmd.exe /c reg add \"HKLM\\SOFTWARE\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0040064\" /f >nul 2>&1",
            "TI.exe cmd.exe /c reg delete \"HKLM\\SOFTWARE\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
        };
        Methods.RunInCMD(commands);
    }
    private void ReturnNetworkIcon(object sender, RoutedEventArgs e)
    {
        if (_initializingNetwork) return;
        AnimateButton(NetworkIconButton);
        var commands = new[]
        {
            "reg add \"HKCU\\Software\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0040064\" /f >nul 2>&1",
            "reg delete \"HKCU\\Software\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
            "reg add \"HKCU\\Software\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0040064\" /f >nul",
            "reg delete \"HKCU\\Software\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
            "TI.exe cmd.exe /c reg add \"HKLM\\SOFTWARE\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0940064\" /f >nul 2>&1",
            "TI.exe cmd.exe /c reg delete \"HKLM\\SOFTWARE\\Classes\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
            "TI.exe cmd.exe /c reg add \"HKLM\\SOFTWARE\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\\ShellFolder\" /v \"Attributes\" /t REG_DWORD /d \"0xb0940064\" /f >nul 2>&1",
            "TI.exe cmd.exe /c reg delete \"HKLM\\SOFTWARE\\Classes\\WOW6432Node\\CLSID\\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\" /v \"System.IsPinnedtoNameSpaceTree\" /f >nul 2>&1",
        };
        Methods.RunInCMD(commands);
    }
    private void ReloadExplorer(object sender, RoutedEventArgs e)
    {
        foreach (var process in Process.GetProcessesByName("explorer"))
        {
            process.Kill();
        }
        Process.Start("explorer.exe");
    }
    #endregion
    #region desktop tweaks
    private void RemoveLabelArrows(object sender, RoutedEventArgs e)
    {
        if (_initializingLabels) return;
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons";
        using (var hKey = Registry.LocalMachine.CreateSubKey(subKey))
        {
            hKey?.SetValue("29", "", RegistryValueKind.String);
        }
        AnimateButton(LabelArrowsButton);
    }
    private void ReturnLabelArrows(object sender, RoutedEventArgs e)
    {
        if (_initializingLabels) return;
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons";
        Registry.LocalMachine.DeleteSubKeyTree(subKey, throwOnMissingSubKey: false);
        AnimateButton(LabelArrowsButton);
    }
    private void RemoveWallpaperCompression(object sender, RoutedEventArgs e)
    {
        if (_initializngWallpaper) return;

        Methods.SetWallpaperCompressionQuality(256);

        AnimateButton(WallpaperCompressionButton);
    }
    private void ReturnWallpaperCompression(object sender, RoutedEventArgs e)
    {
        if (_initializngWallpaper) return;

        Methods.SetWallpaperCompressionQuality(128);

        AnimateButton(WallpaperCompressionButton);
    }

    private void ChangeHighlightColor(object sender, RoutedEventArgs e) { }
    #endregion
    #region activation tweaks
    private void ActivateWindows(object sender, RoutedEventArgs e)
    {
        System.Windows.Forms.MessageBox.Show("Нажмите на клавиатуре клавишу \"1\" для активации Windows или клавишу \"2\" для активации продуктов Office", "Когда откроется окно с выбором для активации");
        var cmdProcessInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c powershell.exe -Command \"irm https://massgrave.dev/get | iex\"",
            Verb = "runas",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        Process.Start(cmdProcessInfo);
    }
    private void ActivateWinRar(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.Forms.MessageBox.Show(
        "Вы меняли директорию по умолчанию для установки WINRAR?",
        "Ответьте на вопрос",
        System.Windows.Forms.MessageBoxButtons.YesNoCancel,
        System.Windows.Forms.MessageBoxIcon.Question
        );
        string path;
        if (result == System.Windows.Forms.DialogResult.Cancel)
        {
            return;
        }
        if (result == System.Windows.Forms.DialogResult.Yes)
        {
            path = Methods.OpenFolderDialog();
        }
        else
        {
            path = @"C:\Program Files\WinRAR";
        }
        var key = @"RAR registration data
PROMSTROI GROUP
15 PC usage license
UID = 42079a849eb3990521f3
641221225021f37c3fecc934136f31d889c3ca46ffcfd8441d3d58
9157709ba0f6ded3a528605030bb9d68eae7df5fedcd1c12e96626
705f33dd41af323a0652075c3cb429f7fc3974f55d1b60e9293e82
ed467e6e4f126e19cccccf98c3b9f98c4660341d700d11a5c1aa52
be9caf70ca9cee8199c54758f64acc9c27d3968d5e69ecb901b91d
538d079f9f1fd1a81d656627d962bf547c38ebbda774df21605c33
eccb9c18530ee0d147058f8b282a9ccfc31322fafcbb4251940582";
        System.IO.File.WriteAllText(path + @"\rarreg.key.txt", key);
    }
    #endregion
    #region fix tweaks
    private void FixHardDisks(object sender, RoutedEventArgs e)
    {
        string command = "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\storahci\\Parameters\\Device\" /f /v TreatAsInternalPort /t REG_MULTI_SZ /d \"0\\0 1\\0 2\\0 3\\0 4\\0 5\\0 6\\0 7\\0 8\\0 9\\0 10\\0 11\\0 12\\0 13\\0 14\\0 15\\0 16";
        Methods.RunInCMD(command);
    }
    private void RemoveFixHardDisks(object sender, RoutedEventArgs e)
    {
        string command = "reg delete \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\storahci\\Parameters\\Device\" /v TreatAsInternalPort /f\r\n";
        Methods.RunInCMD(command);
    }
    #endregion
    #region downloads
    private void DownloadHEVC(object sender, RoutedEventArgs e){
        var hevcRef = @"http://tlu.dl.delivery.mp.microsoft.com/filestreamingservice/files/86f234a3-c022-48ad-a121-d789ee364721?P1=1737659645&P2=404&P3=2&P4=R%2bRHDulYTwrstLR2z0OgSN%2bMyIYAJm3dGBNVKGcM8QVwNABYBnD%2bTTvlvHqJAeQW3cikKB%2b0zzZnvKAKdTPiLQ%3d%3d";
        Process.Start(new ProcessStartInfo
        {
            FileName = hevcRef,
            UseShellExecute = true
        });
    }
    private void DownloadOffice(object sender, RoutedEventArgs e)
    {
        var fileName = @"Configuration.xml";
        StringBuilder fileContent = new(@"<Configuration ID=""aa6c9195-b180-4d82-b808-48f4d1886c73"">
  <Add OfficeClientEdition=""64"" Channel=""PerpetualVL2024"">
    <Product ID=""ProPlus2024Volume"" PIDKEY=""XJ2XN-FW8RK-P4HMP-DKDBV-GCVGB"">
      <Language ID=""ru-ru"" />" + "\n");
        var selectionWindow = new OfficeSelectionWindow();
        bool? result = selectionWindow.ShowDialog();
        if (!selectionWindow.IsConfirmed)
        {
            MessageBox.Show("Установка офиса отменена");
            return;
        }
        if (!selectionWindow.officeSelections[0]) { fileContent.Append(@"      <ExcludeApp ID=""Access"" />" + "\n"); }
        if (!selectionWindow.officeSelections[1]) { fileContent.Append(@"      <ExcludeApp ID=""OneDrive"" />" + "\n"); }
        if (!selectionWindow.officeSelections[2]) { fileContent.Append(@"      <ExcludeApp ID=""Outlook"" />" + "\n"); }
        if (!selectionWindow.officeSelections[3]) { fileContent.Append(@"      <ExcludeApp ID=""Publisher"" />" + "\n"); }
        if (!selectionWindow.officeSelections[4]) { fileContent.Append(@"      <ExcludeApp ID=""Excel"" />" + "\n"); }
        if (!selectionWindow.officeSelections[5]) { fileContent.Append(@"      <ExcludeApp ID=""Lync"" />" + "\n"); }
        if (!selectionWindow.officeSelections[6]) { fileContent.Append(@"      <ExcludeApp ID=""OneNote"" />" + "\n"); }
        if (!selectionWindow.officeSelections[7]) { fileContent.Append(@"      <ExcludeApp ID=""PowerPoint"" />" + "\n"); }
        if (!selectionWindow.officeSelections[8]) { fileContent.Append(@"      <ExcludeApp ID=""Word"" />" + "\n"); }
        fileContent.Append(@"    </Product>
  </Add>
  <Property Name=""SharedComputerLicensing"" Value=""0"" />
  <Property Name=""FORCEAPPSHUTDOWN"" Value=""FALSE"" />
  <Property Name=""DeviceBasedLicensing"" Value=""0"" />
  <Property Name=""SCLCacheOverride"" Value=""0"" />
  <Property Name=""AUTOACTIVATE"" Value=""1"" />
  <Updates Enabled=""TRUE"" />
  <RemoveMSI />
</Configuration>");
        System.IO.File.WriteAllText(@"C:\Program Files\Microsoft Office\" + fileName, fileContent.ToString());
        var urlOffice = @"https://officecdn.microsoft.com/pr/wsus/setup.exe";
        using (var webClient = new WebClient())
        {
            webClient.DownloadFile(urlOffice, @"C:\Program Files\Microsoft Office\setup.exe");
        }
        var reg = @"reg add ""HKCU\Software\Microsoft\Office\16.0\Common\ExperimentConfigs\Ecs"" /v ""CountryCode"" /t REG_SZ /d ""std::wstring|US"" /f";
        Methods.RunInCMD(reg);
        var command = @"cd /d ""C:\Program Files\Microsoft Office"" && setup.exe /configure Configuration.xml";

        Methods.RunInCMD(command);
    }
    #endregion
    #endregion
    #region decorations
    private static void AnimateButton(CheckBox button)
    {
        var toggleButton = (System.Windows.Shapes.Ellipse)button.Template.FindName("ToggleButton", button);

        if (button.IsChecked == true)
        {
            System.Windows.Media.Animation.ThicknessAnimation animation = new()
            {
                From = new Thickness(-1, 0, 0, 0),
                To = new Thickness(16, 0, 0, 0),
                Duration = System.TimeSpan.FromSeconds(0.16)
            };
            toggleButton.BeginAnimation(System.Windows.Shapes.Ellipse.MarginProperty, animation);
        }
        else
        {
            System.Windows.Media.Animation.ThicknessAnimation animation = new()
            {
                From = new Thickness(16, 0, 0, 0),
                To = new Thickness(-1, 0, 0, 0),
                Duration = System.TimeSpan.FromSeconds(0.16)
            };
            toggleButton.BeginAnimation(System.Windows.Shapes.Ellipse.MarginProperty, animation);
        }
    }
    private void TabItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is Border border && System.Windows.Media.VisualTreeHelper.GetParent(border) is TabItem tabItem)
        {
            System.Threading.Thread.Sleep(20);
            tabItemDescription.Text = tabItem.Name switch
            {
                "fileExplorerItem" => "Кастомизация проводника",
                "desktopItem" => "Кастомизация рабочего стола",
                "activationItem" => "Активация",
                "fixesItem" => "Исправление багов винды",
                "downloads" => "Загрузка приложений и файлов",
                _ => string.Empty,
            };
        }
    }
    private void TabItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        System.Threading.Thread.Sleep(30);
        tabItemDescription.Text = string.Empty;
    }
    #endregion
}
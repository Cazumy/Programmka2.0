using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Programmka;
public partial class MainWindow : Window
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
        System.Windows.Forms.MessageBox.Show("Нажмите на клавиатуре клавишу \"1\" для активации Windows или клавишу \"2\" для активации продуктов Office","Когда откроется окно с выбором для активации");
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
        System.Windows.Forms.MessageBoxButtons.YesNo,
        System.Windows.Forms.MessageBoxIcon.Question
    );
        string path;
        if (result == System.Windows.Forms.DialogResult.Yes)
        {
            path = ExplorerDialog.OpenFolderDialog();
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
        File.WriteAllText(path+@"\rarreg.key.txt", key);
    }
    #endregion
    #region fix tweaks
    private void FixHardDisks(object sender, RoutedEventArgs e)
    {
        string command = "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\storahci\\Parameters\\Device\" /f /v TreatAsInternalPort /t REG_MULTI_SZ /d ";
        if (BusNumberInput.Text != "" && int.TryParse(BusNumberInput.Text, out _))
        {
            command += BusNumberInput.Text;
        }
        Methods.RunInCMD(command);
    }
    #endregion
    #endregion
    #region decorations
    private static void AnimateButton(CheckBox button)
    {
        Ellipse toggleButton = (Ellipse)button.Template.FindName("ToggleButton", button);

        if (button.IsChecked == true)
        {
            ThicknessAnimation animation = new()
            {
                From = new Thickness(-1, 0, 0, 0),
                To = new Thickness(16, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.16)
            };
            toggleButton.BeginAnimation(Ellipse.MarginProperty, animation);
        }
        else
        {
            ThicknessAnimation animation = new()
            {
                From = new Thickness(16, 0, 0, 0),
                To = new Thickness(-1, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.16)
            };
            toggleButton.BeginAnimation(Ellipse.MarginProperty, animation);
        }
    }
    private void TabItem_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Border border && VisualTreeHelper.GetParent(border) is TabItem tabItem)
        {
            tabItemDescription.Text = tabItem.Name switch
            {
                "fileExplorerItem" => "Кастомизация проводника",
                "desktopItem" => "Кастомизация рабочего стола",
                "activationItem" => "Активация",
                "fixesItem" => "Исправление багов винды",
                _ => string.Empty,
            };
        }
    }
    private void TabItem_MouseLeave(object sender, MouseEventArgs e)
    {
        tabItemDescription.Text = string.Empty;
    }
    #endregion
}
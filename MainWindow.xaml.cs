using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Programmka;
/// <summary>
///     Логика взаимодействия для MaтinWindow.xaml
/// </summary>
public partial class MainWindow
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

    private static bool CheckDuplicate()
    {
        return Registry.LocalMachine.OpenSubKey(
                   @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders\{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}") != null;
    }
    private static bool CheckLabels()
    {
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons";

        using (var hKey = Registry.LocalMachine.OpenSubKey(subKey))
        {
            var value = hKey?.GetValue("29");
            if (value != null) return false;
        }
        return true;
    }
    private static bool CheckQuickAccess()
    {
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";

        using (var hKey = Registry.LocalMachine.OpenSubKey(subKey))
        {
            var value = hKey?.GetValue("HubMode");
            if (value != null) return false;
        }
        return true;
    }
    private static bool Check3DObjects()
    {
        const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";

        using (var hKey = Registry.LocalMachine.OpenSubKey(keyPath))
        {
            if (hKey != null) return true;
        }

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
        return attributes&&isPinned;
    }

    private static bool CheckWallpaperCompression()
    {
        const string subKey = @"Control Panel\Desktop";
        using (var key = Registry.CurrentUser.OpenSubKey(subKey))
        {
            if (key != null)
            {
                var value = key.GetValue("JPEGImportQuality");
                if (value is int intValue)
                {
                    return intValue != 256;
                }
            }
        }
        return true;
    }

    private static void AnimateButton(CheckBox button)
    {
        Ellipse toggleButton = (Ellipse)button.Template.FindName("ToggleButton", button);

        if (button.IsChecked == true)
        {
            ThicknessAnimation animation = new ThicknessAnimation
            {
                From = new Thickness(-1, 0, 0, 0),
                To = new Thickness(16, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.16)
            };
            toggleButton.BeginAnimation(Ellipse.MarginProperty, animation);
        }
        else
        {
            ThicknessAnimation animation = new ThicknessAnimation
            {
                From = new Thickness(16, 0, 0, 0),
                To = new Thickness(-1, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.16)
            };
            toggleButton.BeginAnimation(Ellipse.MarginProperty, animation);
        }
    }

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
    private void RemoveQuickAccess(object sender, RoutedEventArgs e)
    {
        if (_initializingAccess) return;
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";
        using (var hKey = Registry.LocalMachine.CreateSubKey(subKey))
        {
            hKey?.SetValue("HubMode",1,RegistryValueKind.String);
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
        using (Registry.LocalMachine.CreateSubKey(subKey))
        {

        }
        AnimateButton(Objects3DButton);
    }
    private void RemoveNetworkIcon(object sender, RoutedEventArgs e)
    {
        if (_initializingNetwork) return;
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
        Functions ni = new Functions();
        ni.SetNetworkIcon(commands);

        AnimateButton(NetworkIconButton);
    }
    private void ReturnNetworkIcon(object sender, RoutedEventArgs e)
    {
        if (_initializingNetwork) return;
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
        Functions ni = new Functions();
        ni.SetNetworkIcon(commands);

        AnimateButton(NetworkIconButton);
    }

    private void RemoveWallpaperCompression(object sender, RoutedEventArgs e)
    {
        if (_initializngWallpaper) return;

        Functions wc = new Functions();
        wc.SetWallpaperCompressionQuality(256);

        AnimateButton(WallpaperCompressionButton);
    }

    private void ReturnWallpaperCompression(object sender, RoutedEventArgs e)
    {
        if (_initializngWallpaper) return;

        Functions wc = new Functions();
        wc.SetWallpaperCompressionQuality(128);

        AnimateButton(WallpaperCompressionButton);
    }

    private void ActivateWindows(object sender, RoutedEventArgs e)
    {
        
    }
    private void ActivateWinRar(object sender, RoutedEventArgs e)
    {
        
    }


}
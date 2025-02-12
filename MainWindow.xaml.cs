﻿using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Programmka;
public partial class MainWindow
{
    #region init
    private readonly bool _ignoreToggleEvents = true;
    public MainWindow()
    {
        InitializeComponent();
        InitializeToggleButtons();
        InitializeShowcase();
        _ignoreToggleEvents = false;
        tempSizeText.Text = Methods.NormalizeByteSyze(Methods.GetFullTempSize());
        Closing += MainWindow_Closing;
    }
    private void InitializeToggleButtons()
    {
        ExeNotificationsButton.IsChecked = CheckExeNotifications();
        HibernationButton.IsChecked = CheckHibernation();
        MouseAccelerationButton.IsChecked = CheckMouseAcceleration();
        KeyStickingButton.IsChecked = CheckKeySticking();

        DiskDuplicateButton.IsChecked = CheckDuplicate();
        QuickAccessButton.IsChecked = CheckQuickAccess();
        Objects3DButton.IsChecked = Check3DObjects();
        NetworkIconButton.IsChecked = CheckNetworkIcon();
        FileExtensionsButton.IsChecked = CheckFileExtensions();
        
        LabelArrowsButton.IsChecked = CheckLabels();
        WallpaperCompressionButton.IsChecked = CheckWallpaperCompression();
    }
    private void InitializeShowcase()
    {
        #region explorer
        SetExplorerImage();
        qaImage.Visibility = CheckQuickAccess() ? Visibility.Collapsed : Visibility.Visible;
        tdoImage.Visibility = Check3DObjects() ? Visibility.Collapsed : Visibility.Visible;
        ddImage.Visibility = CheckDuplicate() ? Visibility.Collapsed : Visibility.Visible;
        niImage.Visibility = CheckNetworkIcon() ? Visibility.Collapsed : Visibility.Visible;
        #endregion
        #region desktop
        LoadWallpaperImage();
        desktopShowcase.Visibility = (!string.IsNullOrEmpty(_wallpaperPath) && !string.IsNullOrEmpty(_compressedWallpaperPath)) ? Visibility.Visible : Visibility.Collapsed;
        SetWallpaperImage();
        LabelarrowShowcase.Visibility = CheckLabels() ? Visibility.Collapsed : Visibility.Visible;
        #endregion
    }
    #region checks for ToggleButtons
    private static bool CheckExeNotifications()
    {
        const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
        const string key1 = "ConsentPromptBehaviorAdmin";
        const string key2 = "EnableLUA";
        const string key3 = "PromptOnSecureDesktop";
        return Methods.ContainsRegValue(RegistryHive.LocalMachine, subkey, key1, 0) && Methods.ContainsRegValue(RegistryHive.LocalMachine, subkey, key2, 0) && Methods.ContainsRegValue(RegistryHive.LocalMachine, subkey, key3, 0);
    }
    private static bool CheckHibernation()
    {
        const string subkey = @"SYSTEM\CurrentControlSet\Control\Power";
        const string key = "HibernateEnabled";
        return Methods.ContainsRegValue(RegistryHive.LocalMachine, subkey, key, 0);
    }
    private static bool CheckMouseAcceleration()
    {
        const string subkey = @"Control Panel\Mouse";
        const string key1 = "MouseSpeed", key2 = "MouseThreshold1", key3 = "MouseThreshold2";
        return Methods.ContainsRegValue(RegistryHive.CurrentUser, subkey, key1, 0, "0") && Methods.ContainsRegValue(RegistryHive.CurrentUser, subkey, key2, 0, "0") && Methods.ContainsRegValue(RegistryHive.CurrentUser, subkey, key3, 0, "0");
    }
    private static bool CheckKeySticking()
    {
        const string subkey = @"Control Panel\Accessibility\StickyKeys";
        const string key = "Flags";
        return Methods.ContainsRegValue(RegistryHive.CurrentUser, subkey, key, 0, "506");
    }

    private static bool CheckDuplicate()
    {
        const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders\{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}";
        return Registry.LocalMachine.OpenSubKey(subkey) == null;
    }
    private static bool CheckQuickAccess()
    {
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", key = "HubMode";
        return Methods.ContainsReg(RegistryHive.CurrentUser, subKey, key);
    }
    private static bool Check3DObjects()
    {
        const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";
        return Registry.LocalMachine.OpenSubKey(subkey) == null;
    }
    private static bool CheckNetworkIcon()
    {
        bool attributes = true;
        using (var target = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\CLSID\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}\ShellFolder"))
        {
            if (target != null)
            {
                var targetValue = target.GetValue("Attributes");
                if (targetValue is int intValue)
                {
                    attributes = (uint)intValue != 0xb0940064U; // not Methods.ContainsRegValue cause of uint
                }
            }
        }
        const string subkey = @"SOFTWARE\Classes\CLSID\{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}"; const string key = "System.IsPinnedtoNameSpaceTree"; const int value = 0;
        return !(attributes && !Methods.ContainsRegValue(RegistryHive.CurrentUser, subkey, key, value));
    }
    private static bool CheckFileExtensions()
    {
        const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        const string key = "HideFileExt";
        return Methods.ContainsRegValue(RegistryHive.CurrentUser, subkey, key, 0);
    }

    private static bool CheckLabels()
    {
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", key = "29";
        return Methods.ContainsReg(RegistryHive.LocalMachine, subKey, key);
    }
    private static bool CheckWallpaperCompression()
    {
        const string subKey = @"Control Panel\Desktop"; const string key = "JPEGImportQuality"; const int value = 256;
        return Methods.ContainsRegValue(RegistryHive.CurrentUser, subKey, key, value);
    }    
    #endregion
    #endregion
    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wallpapers")))
        {
            try
            {
                Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wallpapers"), true);
            }
            catch (Exception) { }
        }
    }
    #region tweaks
    #region base
    private void ExeNotifications(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) { return; }
        const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
        const string key1 = "ConsentPromptBehaviorAdmin";
        const string key2 = "EnableLUA";
        const string key3 = "PromptOnSecureDesktop";
        int value1, value2, value3;
        if (ExeNotificationsButton.IsChecked == true)
        {
            value1 = value2 = value3 = 0;
        }
        else
        {
            value1 = 2;
            value2 = value3 = 1;
        }
        Methods.CreateReg(RegistryHive.LocalMachine, subkey, key1, "", "", value1);
        Methods.CreateReg(RegistryHive.LocalMachine, subkey, key2, "", "", value2);
        Methods.CreateReg(RegistryHive.LocalMachine, subkey, key3, "", "", value3);
    }
    private void Hibernation(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        const string subkey = @"SYSTEM\CurrentControlSet\Control\Power";
        const string key = "HibernateEnabled";
        int value = (bool)HibernationButton.IsChecked ? 0 : 1;
        Methods.CreateReg(RegistryHive.LocalMachine, subkey, key, "", "", value);
    }
    private void MouseAcceleration(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        const string subkey = @"Control Panel\Mouse";
        const string key1 = "MouseSpeed", key2 = "MouseThreshold1", key3 = "MouseThreshold2";
        string value1, value2, value3;
        if (MouseAccelerationButton.IsChecked == true)
        {
            value1 = value2 = value3 = "0";
        }
        else
        {
            value1 = "1"; value2 = "6"; value3 = "10";
        }
        Methods.CreateReg(RegistryHive.CurrentUser, subkey, key1, "", value1);
        Methods.CreateReg(RegistryHive.CurrentUser, subkey, key2, "", value2);
        Methods.CreateReg(RegistryHive.CurrentUser, subkey, key3, "", value3);
    }
    private void KeySticking(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        const string subkey = @"Control Panel\Accessibility\StickyKeys";
        const string key = "Flags";
        string keyValue = (bool)KeyStickingButton.IsChecked ? "506" : "511";
        Methods.CreateReg(RegistryHive.CurrentUser, subkey, key, "", keyValue);
    }
    #endregion
    #region explorer tweaks
    private void DiskDuplicate(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders";
        const string dir = "{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}";
        if (DiskDuplicateButton.IsChecked == true)
        {
            Methods.DeleteRegDir(subkey, dir);
        }
        else
        {
            const string key = "Removable Drives";
            Methods.CreateReg(RegistryHive.LocalMachine, subkey, key, dir);
        }
        ddImage.Visibility = (bool)DiskDuplicateButton.IsChecked ? Visibility.Collapsed : Visibility.Visible;
    }
    private void QuickAccess(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";
        const string key = "HubMode";
        if (QuickAccessButton.IsChecked == true)
        {
            const string value = "1";
            Methods.CreateReg(RegistryHive.LocalMachine, subKey, key, "", value);
        }
        else
        {
            Methods.DeleteReg(subKey, key);
        }
        qaImage.Visibility = (bool)QuickAccessButton.IsChecked ? Visibility.Collapsed : Visibility.Visible;
    }
    private void Objects3D(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        if (Objects3DButton.IsChecked == true)
        {
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\"; const string dir = "{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";
            Methods.DeleteRegDir(subKey, dir);
        }
        else
        {
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";
            Methods.CreateRegDir(subKey);
        }
        tdoImage.Visibility = (bool)Objects3DButton.IsChecked ? Visibility.Collapsed : Visibility.Visible;
    }
    private void NetworkIcon(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        if (NetworkIconButton.IsChecked == true)
        {
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
        else
        {
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
        niImage.Visibility = (bool)NetworkIconButton.IsChecked ? Visibility.Collapsed : Visibility.Visible;
    }
    private void FileExtensions(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        const string key = "HideFileExt";
        if (FileExtensionsButton.IsChecked == true)
        {
            const int value = 0;
            Methods.CreateReg(RegistryHive.CurrentUser, subkey, key, "", "", value);
        }
        else
        {
            const int value = 1;
            Methods.CreateReg(RegistryHive.CurrentUser, subkey, key, "", "", value);
        }
        SetExplorerImage();
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
    #region showcase
    private string _wallpaperPath;
    private string _compressedWallpaperPath;
    private void LoadWallpaperImage()
    {
        const string registryKey = @"Control Panel\Desktop";
        const string registryValue = "WallPaper";
        using RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey);
        if (key == null) return;

        string wallpaper = key.GetValue(registryValue)?.ToString();
        if (!File.Exists(wallpaper) || string.IsNullOrEmpty(wallpaper))
        {
            wallpaper = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Windows\Themes\TranscodedWallpaper");
            if (!File.Exists(wallpaper))
            {
                MessageBox.Show("Аллах акбар");
                this.Close();
                return;
            }
        }

        string appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wallpapers");
        if (!Directory.Exists(appFolder))
            Directory.CreateDirectory(appFolder);

        string compressedWallpaper = Path.Combine(appFolder, "compressed_wallpaper.jpg");

        using var bmp = new System.Drawing.Bitmap(wallpaper);
        using var ms = new MemoryStream();
        var jpegCodec = ImageCodecInfo.GetImageDecoders()
            .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 35L);
        bmp.Save(compressedWallpaper, jpegCodec, encoderParams);

        _wallpaperPath = wallpaper;
        _compressedWallpaperPath = compressedWallpaper;
    }
    private void SetWallpaperImage()
    {
        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(CheckWallpaperCompression() ? _wallpaperPath : _compressedWallpaperPath);
        bitmap.EndInit();
        bitmap.Freeze();
        WallpaperImage.Source = bitmap;
    }
    #endregion
    private void LabelArrows(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        if (LabelArrowsButton.IsChecked == true)
        {
            const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons";
            const string key = "29";
            using var target = Registry.LocalMachine.CreateSubKey(subkey);
            target?.SetValue(key, "", RegistryValueKind.String);
        }
        else
        {
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\"; const string dir = "Shell Icons";
            Methods.DeleteRegDir(subKey, dir);
        }
        LabelarrowShowcase.Visibility = (bool)LabelArrowsButton.IsChecked ? Visibility.Collapsed : Visibility.Visible;
    }
    private void WallpaperCompression(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        int keyValue = (bool)WallpaperCompressionButton.IsChecked ? 256 : 128;
        Methods.SetWallpaperCompressionQuality(keyValue);
        SetWallpaperImage();
    }
    private void ChangeHighlightColor(object sender, RoutedEventArgs e)
    {
        var color = ColorPicker.SelectedBrush.Color;
        var rgbValue = $"{color.R} {color.G} {color.B}";
        const string subkey = @"Control Panel\Colors";
        Methods.CreateReg(RegistryHive.CurrentUser, subkey, "Hilight", "", rgbValue);
        tabItemDescription.Text = "Готово";
    }
    #endregion
    #region activation tweaks
    private void ActivateWindows(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Нажмите на клавиатуре клавишу \"1\" для активации Windows или клавишу \"2\" для активации продуктов Office", "Когда откроется окно с выбором для активации");
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
        LoadingStatus();
        using var dialog = new TaskDialog
        {
            Caption = "Ответьте на вопрос",
            InstructionText = "Вы меняли директорию по умолчанию для установки WINRAR?",
            Icon = TaskDialogStandardIcon.Information,
            StandardButtons = TaskDialogStandardButtons.Yes |
                              TaskDialogStandardButtons.No |
                              TaskDialogStandardButtons.Cancel
        };
        var result = dialog.Show();
        string selectedFolderPath = null;
        if (result == TaskDialogResult.Cancel) { LoadingStatus(false); return;  }
        else if (result == TaskDialogResult.No)
        {
            selectedFolderPath = @"C:\Program Files\WinRAR";
        }
        else
        {
            using var folderDialog = new CommonOpenFileDialog { IsFolderPicker = true, Title = "Выберите папку, в которой установлен WinRar" };
            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                selectedFolderPath = folderDialog.FileName;
            }
        }
        if (string.IsNullOrEmpty(selectedFolderPath)) return;
        const string key = @"RAR registration data
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
        File.WriteAllText(selectedFolderPath + @"\rarreg.key.txt", key);
        LoadingStatus(false);
    }
    private void BecameAdminWin10(object sender, RoutedEventArgs e)
    {
        Methods.RunInCMD($"net user {Environment.UserName} /active:yes");
        tabItemDescription.Text = "Пользователь стал администратором";
    }
    #endregion
    #region fix tweaks
    private void FixHardDisks(object sender, RoutedEventArgs e)
    {
        const string command = "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\storahci\\Parameters\\Device\" /f /v TreatAsInternalPort /t REG_MULTI_SZ /d \"0\\0 1\\0 2\\0 3\\0 4\\0 5\\0 6\\0 7\\0 8\\0 9\\0 10\\0 11\\0 12\\0 13\\0 14\\0 15\\0 16";
        Methods.RunInCMD(command);
        tabItemDescription.Text = "Готово";
    }
    private void RemoveFixHardDisks(object sender, RoutedEventArgs e)
    {
        const string command = "reg delete \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\storahci\\Parameters\\Device\" /v TreatAsInternalPort /f\r\n";
        Methods.RunInCMD(command);
        tabItemDescription.Text = "Готово";
    }
    private void ReturnLabelArrows(object sender, RoutedEventArgs e)
    {
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\"; const string dir = "Shell Icons";
        Methods.DeleteRegDir(subKey, dir);
        tabItemDescription.Text = "Готово";
    }
    private void RepairSystem(object sender, RoutedEventArgs e)
    {
        const string commandCmd = "sfc /scannow";
        const string commandPowerShell = "dism /Online /Cleanup-Image /RestoreHealth";
        _ = Methods.RunInCMD(commandCmd, true);
        Methods.RunInPowerShell(commandPowerShell);
    }
    #endregion
    #region downloads
    private async void DownloadOffice(object sender, RoutedEventArgs e)
    {
        LoadingStatus();
        const string fileName = "Configuration.xml";
        System.Text.StringBuilder fileContent = new("""
<Configuration ID="aa6c9195-b180-4d82-b808-48f4d1886c73">
  <Add OfficeClientEdition="64" Channel="PerpetualVL2024">
    <Product ID="ProPlus2024Volume" PIDKEY="XJ2XN-FW8RK-P4HMP-DKDBV-GCVGB">
      <Language ID="ru-ru" />
""" + "\n");
        var selectionWindow = new Resources.OfficeSelectionWindow();
        bool? result = selectionWindow.ShowDialog();
        if (!selectionWindow.IsConfirmed)
        {
            tabItemDescription.Text = "Установка офиса отменена";
            LoadingStatus(false);
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
        fileContent.Append("""
    </Product>
  </Add>
  <Property Name="SharedComputerLicensing" Value="0" />
  <Property Name="FORCEAPPSHUTDOWN" Value="FALSE" />
  <Property Name="DeviceBasedLicensing" Value="0" />
  <Property Name="SCLCacheOverride" Value="0" />
  <Property Name="AUTOACTIVATE" Value="1" />
  <Updates Enabled="TRUE" />
  <RemoveMSI />
</Configuration>
""");
        File.WriteAllText(@"C:\Program Files\Microsoft Office\" + fileName, fileContent.ToString());
        const string urlOffice = "https://officecdn.microsoft.com/pr/wsus/setup.exe";
        using (var webClient = new System.Net.WebClient())
        {
            webClient.DownloadFile(urlOffice, @"C:\Program Files\Microsoft Office\setup.exe");
        }
        const string reg = @"reg add ""HKCU\Software\Microsoft\Office\16.0\Common\ExperimentConfigs\Ecs"" /v ""CountryCode"" /t REG_SZ /d ""std::wstring|US"" /f";
        Methods.RunInCMD(reg);
        const string command = @"cd /d ""C:\Program Files\Microsoft Office"" && setup.exe /configure Configuration.xml";
        LoadingStatus(false);
        await Methods.RunInCMD(command);
    }
    #endregion
    #region cleanup
    private void CheckWinSxS(object sender, RoutedEventArgs e)
    {
        const string command = "Dism.exe /Online /Cleanup-Image /AnalyzeComponentStore";
        Methods.RunInPowerShell(command, "-NoExit");
    }
    private void CleanupWinSxS(object sender, RoutedEventArgs e)
    {
        const string command1 = "Dism.exe /Online /Cleanup-Image /StartComponentCleanup";
        Methods.RunInPowerShell(command1);
        const string command2 = "Dism.exe /Online /Cleanup-Image /StartComponentCleanup /ResetBase";
        Methods.RunInPowerShell(command2);
    }
    private void CleanupTemp(object sender, RoutedEventArgs e)
    {
        LoadingStatus();
        long prevSize = Methods.GetFullTempSize();
        foreach (string path in Methods.AllTempPath)
        {
            Methods.CleanFolder(path);
        }
        tempSizeText.Text = Methods.NormalizeByteSyze(Methods.GetFullTempSize());
        var byteDiff = prevSize - Methods.GetFullTempSize();
        tabItemDescription.Text = $"Успешно очищено: {Methods.NormalizeByteSyze(byteDiff)}";
        LoadingStatus(false);
    }
    #endregion
    #endregion
    #region decorations
    private void LoadingStatus(bool state = true)
    {
        LoadingCircleObject.IsRunning = state;
        LoadingCircleObject.Visibility = state ? Visibility.Visible : Visibility.Hidden;
    }
    private void TabItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is Border border && VisualTreeHelper.GetParent(border) is TabItem tabItem)
        {
            tabItemDescription.Text = tabItem.Name switch
            {
                "baseTweaksItem" => "Базовые твики",
                "fileExplorerItem" => "Кастомизация проводника",
                "desktopItem" => "Кастомизация рабочего стола",
                "activationItem" => "Активация",
                "fixesItem" => "Исправление багов системы",
                "downloads" => "Загрузка приложений и файлов",
                "cleanup" => "Очистка системы",
                _ => string.Empty,
            };
        }
    }
    private void TabItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        tabItemDescription.Text = string.Empty;
    }
    private void SetExplorerImage()
    {
        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(CheckFileExtensions() ?
            "pack://application:,,,/Resources/Images/Showcase/explorer_ex.png" :
            "pack://application:,,,/Resources/Images/Showcase/explorer_noex.png", UriKind.Absolute);
        bitmap.EndInit();
        explorerShowcaseImage.Source = bitmap;
    }
    private void ColorPickerPreview(object sender, HandyControl.Data.FunctionEventArgs<Color> e)
    {
        var colorBorder = "#" + ColorPicker.SelectedBrush.ToString().Substring(3);
        HighlightColorImage.BorderBrush = (Brush)new BrushConverter().ConvertFromString(colorBorder);
        var colorBackground = "#30" + ColorPicker.SelectedBrush.ToString().Substring(3);
        HighlightColorImage.Background = (Brush)new BrushConverter().ConvertFromString(colorBackground);
    }
    #endregion
}
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Programmka;
public partial class MainWindow
{
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
        DiskDuplicateButton.IsChecked = CheckDuplicate();
        QuickAccessButton.IsChecked = CheckQuickAccess();
        Objects3DButton.IsChecked = Check3DObjects();
        NetworkIconButton.IsChecked = CheckNetworkIcon();
        FileExtensionsButton.IsChecked = CheckFileExtensions();
        ExeNotificationsButton.IsChecked = CheckExeNotifications();
        LabelArrowsButton.IsChecked = CheckLabels();
        WallpaperCompressionButton.IsChecked = CheckWallpaperCompression();
    }
    private void InitializeShowcase()
    {
        #region explorer
        SetExplorerImage();
        qaImage.Width = CheckQuickAccess() ? 0 : 149;
        tdoImage.Width = Check3DObjects() ? 0 : 149;
        ddImage.Width = CheckDuplicate() ? 0 : 149;
        niImage.Width = CheckNetworkIcon() ? 0 : 149;
        #endregion
        #region desktop
        LoadWallpaperImage();
        desktopShowcase.Visibility = (!string.IsNullOrEmpty(_wallpaperPath) && !string.IsNullOrEmpty(_compressedWallpaperPath)) ? Visibility.Visible : Visibility.Collapsed;
        SetWallpaperImage();
        LabelarrowShowcase.Visibility = CheckLabels() ? Visibility.Collapsed : Visibility.Visible;
        #endregion
    }
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
    #region checks for ToggleButtons
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
    private static bool CheckExeNotifications()
    {
        const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
        const string key1 = "ConsentPromptBehaviorAdmin";
        const string key2 = "EnableLUA";
        const string key3 = "PromptOnSecureDesktop";
        return Methods.ContainsRegValue(RegistryHive.LocalMachine, subkey, key1, 0) && Methods.ContainsRegValue(RegistryHive.LocalMachine, subkey, key2, 0) && Methods.ContainsRegValue(RegistryHive.LocalMachine, subkey, key3, 0);
    }
    #endregion
    #region tweaks
    #region explorer tweaks
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
    private void DiskDuplicate(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        if (DiskDuplicateButton.IsChecked == true)
        {
            ddImage.Width = 0;
            const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders";
            const string dir = "{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}";
            Methods.DeleteRegDir(subkey, dir);
        }
        else
        {
            ddImage.Width = 151;
            const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders";
            const string dir = "{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}";
            const string key = "Removable Drives";
            Methods.CreateReg(RegistryHive.LocalMachine, subkey, key, dir);
        }
    }
    private void QuickAccess(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        if (QuickAccessButton.IsChecked == true)
        {
            qaImage.Width = 0;
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";
            const string key = "HubMode";
            const string value = "1";
            Methods.CreateReg(RegistryHive.LocalMachine, subKey, key, "", value);
        }
        else
        {
            qaImage.Width = 151;
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";
            const string key = "HubMode";
            Methods.DeleteReg(subKey, key);
        }
    }
    private void Objects3D(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        if (Objects3DButton.IsChecked == true)
        {
            tdoImage.Width = 0;
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\"; const string dir = "{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";
            Methods.DeleteRegDir(subKey, dir);
        }
        else
        {
            tdoImage.Width = 151;
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";
            Methods.CreateRegDir(subKey);
        }
    }
    private void NetworkIcon(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        if (NetworkIconButton.IsChecked == true)
        {
            niImage.Width = 0;
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
            niImage.Width = 151;
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
    private void ExeNotifications(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) { return; }
        const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
        const string key1 = "ConsentPromptBehaviorAdmin";
        const string key2 = "EnableLUA";
        const string key3 = "PromptOnSecureDesktop";
        if (ExeNotificationsButton.IsChecked == true)
        {
            Methods.CreateReg(RegistryHive.LocalMachine, subkey, key1, "", "", 0);
            Methods.CreateReg(RegistryHive.LocalMachine, subkey, key2, "", "", 0);
            Methods.CreateReg(RegistryHive.LocalMachine, subkey, key3, "", "", 0);
        }
        else
        {
            Methods.CreateReg(RegistryHive.LocalMachine, subkey, key1, "", "", 2);
            Methods.CreateReg(RegistryHive.LocalMachine, subkey, key2, "", "", 1);
            Methods.CreateReg(RegistryHive.LocalMachine, subkey, key3, "", "", 1);
        }
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
        if (string.IsNullOrEmpty(wallpaper) && !File.Exists(wallpaper))
        {
            desktopShowcase.Visibility = Visibility.Collapsed;
            return;
        }

        string appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Wallpapers");
        if (!Directory.Exists(appFolder))
            Directory.CreateDirectory(appFolder);

        string compressedWallpaper = Path.Combine(appFolder, "compressed_wallpaper.jpg");

        using (var bmp = new Bitmap(wallpaper))//странно 129038712803710273081273081273
        using (var ms = new MemoryStream())
        {
            var jpegCodec = ImageCodecInfo.GetImageDecoders()
                .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 35L);
            bmp.Save(compressedWallpaper, jpegCodec, encoderParams);
        }

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
            LabelarrowShowcase.Visibility = Visibility.Collapsed;
            const string subkey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons";
            const string key = "29";
            using var target = Registry.LocalMachine.CreateSubKey(subkey);
            target?.SetValue(key, "", RegistryValueKind.String);
        }
        else
        {
            LabelarrowShowcase.Visibility = Visibility.Visible;
            const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\"; const string dir = "Shell Icons";
            Methods.DeleteRegDir(subKey, dir);
        }
    }
    private void WallpaperCompression(object sender, RoutedEventArgs e)
    {
        if (_ignoreToggleEvents) return;
        if (WallpaperCompressionButton.IsChecked == true)
        {
            Methods.SetWallpaperCompressionQuality(256);
        }
        else
        {
            Methods.SetWallpaperCompressionQuality(128);
        }
        SetWallpaperImage();
    }
    private void ChangeHighlightColor(object sender, RoutedEventArgs e) { }
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
        var dialog = new TaskDialog
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
        if (result == TaskDialogResult.Cancel) { return; }
        else if (result == TaskDialogResult.No)
        {
            selectedFolderPath = @"C:\Program Files\WinRAR";
        }
        else
        {
            var folderDialog = new CommonOpenFileDialog { IsFolderPicker = true };
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
        System.IO.File.WriteAllText(selectedFolderPath + @"\rarreg.key.txt", key);
    }
    private void BecameAdminWin10(object sender, RoutedEventArgs e)
    {
        Methods.RunInCMD($"net user {System.Environment.UserName} /active:yes");
    }
    #endregion
    #region fix tweaks
    private void FixHardDisks(object sender, RoutedEventArgs e)
    {
        const string command = "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\storahci\\Parameters\\Device\" /f /v TreatAsInternalPort /t REG_MULTI_SZ /d \"0\\0 1\\0 2\\0 3\\0 4\\0 5\\0 6\\0 7\\0 8\\0 9\\0 10\\0 11\\0 12\\0 13\\0 14\\0 15\\0 16";
        Methods.RunInCMD(command);
    }
    private void RemoveFixHardDisks(object sender, RoutedEventArgs e)
    {
        const string command = "reg delete \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\storahci\\Parameters\\Device\" /v TreatAsInternalPort /f\r\n";
        Methods.RunInCMD(command);
    }
    private void ReturnLabelArrows(object sender, RoutedEventArgs e)
    {
        const string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\"; const string dir = "Shell Icons";
        Methods.DeleteRegDir(subKey, dir);
    }
    #endregion
    #region downloads
    private void DownloadHEVC(object sender, RoutedEventArgs e)
    {
        const string hevcRef = "http://tlu.dl.delivery.mp.microsoft.com/filestreamingservice/files/86f234a3-c022-48ad-a121-d789ee364721?P1=1737659645&P2=404&P3=2&P4=R%2bRHDulYTwrstLR2z0OgSN%2bMyIYAJm3dGBNVKGcM8QVwNABYBnD%2bTTvlvHqJAeQW3cikKB%2b0zzZnvKAKdTPiLQ%3d%3d";
        Process.Start(new ProcessStartInfo
        {
            FileName = hevcRef,
            UseShellExecute = true
        });
    }
    private async void DownloadOffice(object sender, RoutedEventArgs e)
    {
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
        System.IO.File.WriteAllText(@"C:\Program Files\Microsoft Office\" + fileName, fileContent.ToString());
        const string urlOffice = "https://officecdn.microsoft.com/pr/wsus/setup.exe";
        using (var webClient = new System.Net.WebClient())
        {
            webClient.DownloadFile(urlOffice, @"C:\Program Files\Microsoft Office\setup.exe");
        }
        const string reg = @"reg add ""HKCU\Software\Microsoft\Office\16.0\Common\ExperimentConfigs\Ecs"" /v ""CountryCode"" /t REG_SZ /d ""std::wstring|US"" /f";
        Methods.RunInCMD(reg);
        const string command = @"cd /d ""C:\Program Files\Microsoft Office"" && setup.exe /configure Configuration.xml";

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
        long prevSize = Methods.GetFullTempSize();
        foreach (string path in Methods.AllTempPath)
        {
            Methods.CleanFolder(path);
        }
        tempSizeText.Text = Methods.NormalizeByteSyze(Methods.GetFullTempSize());
        var byteDiff = prevSize - Methods.GetFullTempSize();
        tabItemDescription.Text = $"Успешно очищено: {Methods.NormalizeByteSyze(byteDiff)}";
    }
    #endregion
    #endregion
    #region decorations
    private void TabItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is Border border && System.Windows.Media.VisualTreeHelper.GetParent(border) is TabItem tabItem)
        {
            tabItemDescription.Text = tabItem.Name switch
            {
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
    #endregion
}
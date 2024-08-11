using System;
using System.Windows;
using Microsoft.Win32;
namespace Programmka2._0
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool Initializing_Labels = true,
            Initializing_Disk = true,
            Initializing_Access = true,
            Initializing_Objects = true,
            Initializing_Network = true;
        public MainWindow()
        {
            InitializeComponent();

            InitializeCheckBoxes();
        }
        private void MainWindow_Loaded(object senter, RoutedEventArgs e)
        {
            Initializing_Labels = Initializing_Disk = Initializing_Access = Initializing_Objects = Initializing_Network = false;
        }
        public void InitializeCheckBoxes()
        {
            DiskDuplicate.IsChecked = CheckDuplicate();

            LabelArrows.IsChecked = CheckLabels();

            QuickAccess.IsChecked = CheckQuickAccess();

            Objects3D.IsChecked = Check3DObjects();
        }
        private bool CheckDuplicate()
        {
            if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders\{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}") != null)
            {
                return true;
            }
            return false;
        }
        private bool CheckLabels()
        {
            string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons";
            string paramName = "29";

            using (RegistryKey hKey = Registry.LocalMachine.OpenSubKey(subKey))
            {
                if (hKey != null)
                {
                    object value = hKey.GetValue(paramName);
                    if (value != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        private bool CheckQuickAccess()
        {
            string subKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer";
            string paramName = "HubMode";

            using (RegistryKey hKey = Registry.LocalMachine.OpenSubKey(subKey))
            {
                if (hKey != null)
                {
                    object value = hKey.GetValue(paramName);
                    if (value != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private bool Check3DObjects()
        {
            string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}";

            using (RegistryKey hKey = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (hKey != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void DiskDuplicate_On(object sender, RoutedEventArgs e)
        {
            if (Initializing_Disk) return;
            string subKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders";
            string keyName = "{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}";
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(subKeyPath, true))
                {
                    if (key != null)
                    {
                        key.DeleteSubKeyTree(keyName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private void DiskDuplicate_Off(object sender, RoutedEventArgs e)
        {
            if (Initializing_Disk) return;
            string subKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders";
            string keyName = "{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}";
            string valueData = "Removable Drives";
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(subKeyPath))
                {
                    if (key != null)
                    {
                        // Проверяем, существует ли ключ
                        var newKey = key.OpenSubKey(keyName, true);
                        if (newKey == null)
                        {
                            // Создаем ключ, если он не существует
                            newKey = key.CreateSubKey(keyName);
                        }
                        if (newKey != null)
                        {
                            newKey.SetValue("", valueData, RegistryValueKind.String);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void LabelArrows_On(object sender, RoutedEventArgs e)
        {
            if (Initializing_Labels) return;
        }
        private void LabelArrows_Off(object sender, RoutedEventArgs e)
        {
            if (Initializing_Labels) return;
        }

        private void QuickAccess_On(object sender, RoutedEventArgs e)
        {
            if (Initializing_Access) return;
        }
        private void QuickAccess_Off(object sender, RoutedEventArgs e)
        {
            if (Initializing_Access) return;
        }

        private void Objects3D_On(object sender, RoutedEventArgs e)
        {
            if (Initializing_Objects) return;
        }
        private void Objects3D_Off(object sender, RoutedEventArgs e)
        {
            if (Initializing_Objects) return;
        }

        private void Network_icon_On(object sender, RoutedEventArgs e)
        {
            if (Initializing_Network) return;
        }
        private void Network_icon_Off(object sender, RoutedEventArgs e)
        {
            if (Initializing_Network) return;
        }
    }
}

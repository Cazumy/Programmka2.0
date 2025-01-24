using System.Windows;

namespace Programmka.Resources
{
    /// <summary>
    /// Логика взаимодействия для OfficeSelectionWindow.xaml
    /// </summary>
    public partial class OfficeSelectionWindow : MahApps.Metro.Controls.MetroWindow
    {
        public bool[] officeSelections = new bool[9];
        public bool IsConfirmed { get; private set; } = false;
        public OfficeSelectionWindow()
        {
            InitializeComponent();
        }
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void UpdateSelection(int index, bool isOn)
        {
            officeSelections[index] = isOn;
        }
        private void AccessSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(0, AccessToggle.IsOn);
        }
        private void OneDriveDesktopSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(1, OneDriveDesktopToggle.IsOn);
        }
        private void OutlookSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(2, OutlookToggle.IsOn);
        }
        private void PublisherSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(3, PublisherToggle.IsOn);
        }
        private void ExcelSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(4, ExcelToggle.IsOn);
        }
        private void SkypeSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(5, SkypeToggle.IsOn);
        }
        private void OneNoteSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(6, OneNoteToggle.IsOn);
        }
        private void PowerPointSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(7, PowerPointToggle.IsOn);
        }
        private void WordSelection(object sender, RoutedEventArgs e)
        {
            UpdateSelection(8, WordToggle.IsOn);
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }
    }
}

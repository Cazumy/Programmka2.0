using System.Windows;

namespace Programmka.Resources
{
    /// <summary>
    /// Логика взаимодействия для OfficeSelectionWindow.xaml
    /// </summary>
    public partial class OfficeSelectionWindow : MahApps.Metro.Controls.MetroWindow
    {
        public bool[] officeSelections = new bool[10];
        public OfficeSelectionWindow()
        {
            InitializeComponent();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void AccessSelection(object sender, RoutedEventArgs e)
        {
            officeSelections[0] = AccessToggle.IsOn ? true : false;
        }
        private void OneDriveGrooveSelection(object sender, RoutedEventArgs e)
        {
            officeSelections[1] = AccessToggle.IsOn ? true : false;
        }
        private void OneDriveDesktopSelection(object sender, RoutedEventArgs e)
        {
            officeSelections[2] = AccessToggle.IsOn ? true : false;
        }
        private void OutlookSelection(object sender, RoutedEventArgs e)
        {
            officeSelections[3] = AccessToggle.IsOn ? true : false;
        }
        private void PublisherSelection(object sender, RoutedEventArgs e)
        {
            officeSelections[4] = AccessToggle.IsOn ? true : false;
        }
        private void ExcelSelection(object sender, RoutedEventArgs e)
        {
            officeSelections[5] = AccessToggle.IsOn ? true : false;
        }
        private void SkypeSelection(object sender, RoutedEventArgs e)
        {
            officeSelections[6] = AccessToggle.IsOn ? true : false;
        }
        private void OneNoteSelection(object sender, RoutedEventArgs e)
        {
            officeSelections[7] = AccessToggle.IsOn ? true : false;
        }
        private void PowerPointSelection(object sender, RoutedEventArgs e)
        {
            officeSelections[8] = AccessToggle.IsOn ? true : false;
        }
        private void WordSelection(object sender, RoutedEventArgs e)
        {
            officeSelections[9] = AccessToggle.IsOn ? true : false;
        }
    }
}

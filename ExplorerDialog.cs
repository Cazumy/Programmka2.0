using Ookii.Dialogs.WinForms;

namespace Programmka
{
    public class ExplorerDialog
    {
        public static string OpenFolderDialog()
        {
            using var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "�������� �����, � ������� ���������� WINRAR";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            return null;
        }
    }
}
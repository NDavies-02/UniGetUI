using System.Data;
using System.Diagnostics;
using System.Security.Authentication;
using System.Security.Cryptography;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using UniGetUI.Core.Data;
using UniGetUI.Core.Logging;
using UniGetUI.Core.SettingsEngine;
using UniGetUI.Core.Tools;
using UniGetUI.Interface.SoftwarePages;
using UniGetUI.PackageEngine.Enums;
using UniGetUI.Pages.DialogPages;
using UniGetUI.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UniGetUI.Pages.SettingsPages.GeneralPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Backup : Page, ISettingsPage
    {
        public Backup()
        {
            this.InitializeComponent();
            EnablePackageBackupUI(Settings.Get(Settings.K.EnablePackageBackup_LOCAL));
            ResetBackupDirectory.Content = CoreTools.Translate("Reset");
            OpenBackupDirectory.Content = CoreTools.Translate("Open");
                    }

        public bool CanGoBack => true;

        public string ShortTitle => CoreTools.Translate("Backup and Restore");

        public event EventHandler? RestartRequired;
        public event EventHandler<Type>? NavigationRequested
        {
            add { }
            remove { }
        }

        public void ShowRestartBanner(object? sender, EventArgs e) =>
            RestartRequired?.Invoke(this, e);

        private void ChangeBackupDirectory_Click(object sender, EventArgs e)
        {
            ExternalLibraries.Pickers.FolderPicker openPicker = new(
                MainApp.Instance.MainWindow.GetWindowHandle()
            );
            string folder = openPicker.Show();
            if (folder != string.Empty)
            {
                Settings.SetValue(Settings.K.ChangeBackupOutputDirectory, folder);
                BackupDirectoryLabel.Text = folder;
                ResetBackupDirectory.IsEnabled = true;
            }
        }

        public void EnablePackageBackupUI(bool enabled)
        {
            EnableBackupTimestampingCheckBox.IsEnabled = enabled;
            ChangeBackupFileNameTextBox.IsEnabled = enabled;
            ChangeBackupDirectory.IsEnabled = enabled;
            BackupNowButton_LOCAL.IsEnabled = enabled;

            if (enabled)
            {
                if (!Settings.Get(Settings.K.ChangeBackupOutputDirectory))
                {
                    BackupDirectoryLabel.Text = CoreData.UniGetUI_DefaultBackupDirectory;
                    ResetBackupDirectory.IsEnabled = false;
                }
                else
                {
                    BackupDirectoryLabel.Text = Settings.GetValue(
                        Settings.K.ChangeBackupOutputDirectory
                    );
                    ResetBackupDirectory.IsEnabled = true;
                }
            }
        }

        private void ResetBackupPath_Click(object sender, RoutedEventArgs e)
        {
            BackupDirectoryLabel.Text = CoreData.UniGetUI_DefaultBackupDirectory;
            Settings.Set(Settings.K.ChangeBackupOutputDirectory, false);
            ResetBackupDirectory.IsEnabled = false;
        }

        private void OpenBackupPath_Click(object sender, RoutedEventArgs e)
        {
            string directory = Settings.GetValue(Settings.K.ChangeBackupOutputDirectory);
            if (directory == "")
                directory = CoreData.UniGetUI_DefaultBackupDirectory;

            directory = directory.Replace("/", "\\");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            CoreTools.Launch(directory);
        }

        private void DoBackup_LOCAL_Click(object sender, EventArgs e) =>
            _ = _doBackup_LOCAL_Click();

        private static async Task _doBackup_LOCAL_Click()
        {
            int loadingId = DialogHelper.ShowLoadingDialog(
                CoreTools.Translate("Performing backup, please wait...")
            );
            await InstalledPackagesPage.BackupPackages_LOCAL();
            DialogHelper.HideLoadingDialog(loadingId);
        }
       
    }
}

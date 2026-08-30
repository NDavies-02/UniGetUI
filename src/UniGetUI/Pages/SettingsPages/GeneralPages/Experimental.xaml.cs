using Microsoft.UI.Xaml.Controls;
using UniGetUI.Core.Tools;
using Microsoft.UI.Xaml;
using UniGetUI.Pages.SettingsPages.GeneralPages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UniGetUI.Pages.SettingsPages.GeneralPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Experimental : Page, ISettingsPage
    {
        public Experimental()
        {
            this.InitializeComponent();
        }

        private void SettingsExperimental_Loaded(object sender, RoutedEventArgs e)
        {
            MainApp.Instance.MainWindow.GlobalSearchBox.Visibility = Visibility.Collapsed;
        }

        private void SettingsExperimental_Unloaded(object sender, RoutedEventArgs e)
        {
            MainApp.Instance.MainWindow.GlobalSearchBox.Visibility = Visibility.Visible;
        }

        public bool CanGoBack => true;

        public string ShortTitle =>
            CoreTools.Translate("Experimental settings and developer options");

        public event EventHandler? RestartRequired;
        public event EventHandler<Type>? NavigationRequested
        {
            add { }
            remove { }
        }

        public void ShowRestartBanner(object sender, EventArgs e) =>
            RestartRequired?.Invoke(this, e);
    }
}

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using UniGetUI.Core.Data;
using UniGetUI.Core.Logging;

namespace UniGetUI
{
    public static class EntryPoint
    {
        private const uint MessageBoxYesNo = 0x00000004;
        private const uint MessageBoxIconQuestion = 0x00000020;
        private const uint MessageBoxIconError = 0x00000010;
        private const int MessageBoxResultYes = 6;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(
            nint hWnd,
            string text,
            string caption,
            uint type
        );

        [STAThread]
        private static void Main(string[] args)
        {
            // Having an async main method breaks WebView2
            try
            {
                if (!EnsureAdministrator())
                {
                    Environment.Exit(0);
                    return;
                }

                if (args.Contains(CLIHandler.HELP))
                {
                    CLIHandler.Help();
                    Environment.Exit(0);
                }
                else if (args.Contains(CLIHandler.MIGRATE_WINGETUI_TO_UNIGETUI))
                {
                    int ret = CLIHandler.WingetUIToUniGetUIMigrator();
                    Environment.Exit(ret);
                }
                else if (
                    args.Contains(CLIHandler.UNINSTALL_UNIGETUI)
                    || args.Contains(CLIHandler.UNINSTALL_WINGETUI)
                )
                {
                    int ret = CLIHandler.UninstallUniGetUI();
                    Environment.Exit(ret);
                }
                else if (args.Contains(CLIHandler.IMPORT_SETTINGS))
                {
                    int ret = CLIHandler.ImportSettings();
                    Environment.Exit(ret);
                }
                else if (args.Contains(CLIHandler.EXPORT_SETTINGS))
                {
                    int ret = CLIHandler.ExportSettings();
                    Environment.Exit(ret);
                }
                else if (args.Contains(CLIHandler.ENABLE_SETTING))
                {
                    int ret = CLIHandler.EnableSetting();
                    Environment.Exit(ret);
                }
                else if (args.Contains(CLIHandler.DISABLE_SETTING))
                {
                    int ret = CLIHandler.DisableSetting();
                    Environment.Exit(ret);
                }
                else if (args.Contains(CLIHandler.SET_SETTING_VAL))
                {
                    int ret = CLIHandler.SetSettingsValue();
                    Environment.Exit(ret);
                }
                else if (args.Contains(CLIHandler.ENABLE_SECURE_SETTING))
                {
                    int ret = CLIHandler.EnableSecureSetting();
                    Environment.Exit(ret);
                }
                else if (args.Contains(CLIHandler.DISABLE_SECURE_SETTING))
                {
                    int ret = CLIHandler.DisableSecureSetting();
                    Environment.Exit(ret);
                }
                else if (args.Contains(CLIHandler.ENABLE_SECURE_SETTING_FOR_USER))
                {
                    int ret = CLIHandler.EnableSecureSettingForUser();
                    Environment.Exit(ret);
                }
                else if (args.Contains(CLIHandler.DISABLE_SECURE_SETTING_FOR_USER))
                {
                    int ret = CLIHandler.DisableSecureSettingForUser();
                    Environment.Exit(ret);
                }
                else
                {
                    CoreData.WasDaemon = CoreData.IsDaemon = args.Contains(CLIHandler.DAEMON);
                    _ = AsyncMain();
                }
            }
            catch (Exception e)
            {
                CrashHandler.ReportFatalException(e);
            }
        }

        private static bool EnsureAdministrator()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);

            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                return true;
            }

            int result = MessageBox(
                nint.Zero,
                "UniGetUI needs administrator privileges to run. Restart the application as administrator?",
                "Administrator privileges required",
                MessageBoxYesNo | MessageBoxIconQuestion
            );

            if (result != MessageBoxResultYes)
            {
                return false;
            }

            try
            {
                string? executablePath = Environment.ProcessPath;
                if (string.IsNullOrWhiteSpace(executablePath))
                {
                    return false;
                }

                ProcessStartInfo startInfo = new()
                {
                    FileName = executablePath,
                    Verb = "runas",
                    UseShellExecute = true,
                    WorkingDirectory = AppContext.BaseDirectory,
                };

                foreach (string argument in Environment.GetCommandLineArgs().Skip(1))
                {
                    startInfo.ArgumentList.Add(argument);
                }

                Process.Start(startInfo);
                return false;
            }
            catch (Win32Exception)
            {
                // The user cancelled the UAC prompt.
                return false;
            }
            catch (Exception ex)
            {
                MessageBox(
                    nint.Zero,
                    $"Could not restart UniGetUI as administrator:\n\n{ex.Message}",
                    "Unable to restart",
                    MessageBoxIconError
                );

                return false;
            }
        }

        /// <summary>
        /// UniGetUI app main entry point
        /// </summary>
        private static async Task AsyncMain()
        {
            try
            {
                string textart = $"""
                       __  __      _ ______     __  __  ______
                      / / / /___  (_) ____/__  / /_/ / / /  _/
                     / / / / __ \/ / / __/ _ \/ __/ / / // /
                    / /_/ / / / / / /_/ /  __/ /_/ /_/ // /
                    \____/_/ /_/_/\____/\___/\__/\____/___/
                        Welcome to UniGetUI Version {CoreData.VersionName}
                    """;

                Logger.ImportantInfo(textart);
                Logger.ImportantInfo("  ");
                Logger.ImportantInfo($"Build {CoreData.BuildNumber}");
                Logger.ImportantInfo($"Data directory {CoreData.UniGetUIDataDirectory}");
                Logger.ImportantInfo($"Encoding Code Page set to {CoreData.CODE_PAGE}");

                // WinRT single-instance fancy stuff
                WinRT.ComWrappersSupport.InitializeComWrappers();
                bool isRedirect = await DecideRedirection();

                // If this is the main instance, start the app
                if (!isRedirect)
                {
                    Application.Start(
                        (_) =>
                        {
                            DispatcherQueueSynchronizationContext context = new(
                                DispatcherQueue.GetForCurrentThread()
                            );
                            SynchronizationContext.SetSynchronizationContext(context);
                            var app = new MainApp();
                        }
                    );
                }
            }
            catch (Exception e)
            {
                CrashHandler.ReportFatalException(e);
            }
        }

        /// <summary>
        /// Default WinUI Redirector
        /// </summary>
        private static async Task<bool> DecideRedirection()
        {
            try
            {
                // IDK how does this work, I copied it from the MS Docs
                // example on single-instance apps using unpackaged AppSdk + WinUI3
                bool isRedirect = false;

                var keyInstance = AppInstance.FindOrRegisterForKey(CoreData.MainWindowIdentifier);
                if (keyInstance.IsCurrent)
                {
                    keyInstance.Activated += async (_, e) =>
                    {
                        if (Application.Current is MainApp baseInstance)
                        {
                            await baseInstance.ShowMainWindowFromRedirectAsync(e);
                        }
                    };
                }
                else
                {
                    isRedirect = true;
                    AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();
                    await keyInstance.RedirectActivationToAsync(args);
                }
                return isRedirect;
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                return false;
            }
        }
    }
}

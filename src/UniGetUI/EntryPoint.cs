using System.ComponentModel;
using System.Diagnostics;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using UniGetUI.Core.Data;
using UniGetUI.Core.Logging;

namespace UniGetUI
{
    public static class EntryPoint
    {
        [STAThread]
        private static void Main(string[] args)
        {
            // Having an async main method breaks WebView2
            try
            {
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

        public static bool RestartAsAdministrator()
        {
            try
            {
                string? executablePath = Environment.ProcessPath;
                if (string.IsNullOrWhiteSpace(executablePath))
                {
                    return false;
                }

                ProcessStartInfo startInfo = new() // Launch elevated process
                {
                    FileName = executablePath,
                    Verb = "runas",
                    UseShellExecute = true,
                    WorkingDirectory = AppContext.BaseDirectory,
                };

                // Add existing args
                foreach (string argument in Environment.GetCommandLineArgs().Skip(1))
                {
                    startInfo.ArgumentList.Add(argument);
                }

                // Add an explicit marker so the new elevated instance can avoid single-instance redirection
                startInfo.ArgumentList.Add(CLIHandler.RESTARTED_AS_ADMIN);

                Process.Start(startInfo);
                return true;
            }
            catch (Win32Exception)
            {
                // The user cancelled the UAC prompt.
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Could not restart UniGetUI as administrator.");
                Logger.Error(ex);
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
                        Welcome to UniGetUI (NDavies-02 Fork) v{CoreData.VersionName}
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
        /// WinUI Redirector has been changed to handle admin restarts whilst preserving single-instance behaviour
        /// In short, restarted instance attempts to become main instance, failing that, use unique identifier key
        /// Which key is used is time-based: the original instance needs to close quickly enough so that the elevated instance can acquire the main key
        /// </summary>
        private static async Task<bool> DecideRedirection()
        {
            try
            {
                bool isRedirect = false;

                var args = Environment.GetCommandLineArgs();
                bool restartAsAdminMarker = args.Any(a => a == CLIHandler.RESTARTED_AS_ADMIN);

                string registerKey = CoreData.MainWindowIdentifier;
                AppInstance keyInstance;

                if (restartAsAdminMarker) // If this is a "restarted as admin" instance from the banner prompt...
                {
                    // Acquire the normal main key for a short period so the elevated instance becomes the canonical instance.
                    const int maxAttempts = 10; // ~1s total (10 * 100ms from Task.Delay)
                    int attempts = 0;
                    while (true) // Keep checking what key the elevated instance is using
                    {
                        keyInstance = AppInstance.FindOrRegisterForKey(registerKey); // Attempt to acquire the main key
                        if (keyInstance.IsCurrent) //If successful...
                        {
                            // ...we successfully became the main instance
                            break;
                        }

                        attempts++;
                        if (attempts >= maxAttempts) // Once attempts exceeded, give up, use a unique key
                        {
                            registerKey = CoreData.MainWindowIdentifier + "-elevated-" + Environment.ProcessId;
                            keyInstance = AppInstance.FindOrRegisterForKey(registerKey);
                            break;
                        }

                        await Task.Delay(100);
                    }
                }
                else
                {
                    keyInstance = AppInstance.FindOrRegisterForKey(registerKey);
                }

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
                    AppActivationArguments argsActivation = AppInstance.GetCurrent().GetActivatedEventArgs();
                    await keyInstance.RedirectActivationToAsync(argsActivation);
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

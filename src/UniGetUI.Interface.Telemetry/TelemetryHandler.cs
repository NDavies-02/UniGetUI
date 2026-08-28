using UniGetUI.Core.Data;
using UniGetUI.Core.Language;
using UniGetUI.Core.Logging;
using UniGetUI.Core.SettingsEngine;
using UniGetUI.Core.Tools;
using UniGetUI.PackageEngine;
using UniGetUI.PackageEngine.Enums;
using UniGetUI.PackageEngine.Interfaces;

namespace UniGetUI.Interface.Telemetry;

public enum TEL_InstallReferral
{
    DIRECT_SEARCH,
    FROM_BUNDLE,
    FROM_WEB_SHARE,
    ALREADY_INSTALLED,
}

public enum TEL_OP_RESULT
{
    SUCCESS,
    FAILED,
    CANCELED,
}

public static class TelemetryHandler
{
    private const string HOST = "http://localhost:3000";
    private const string HOST = "https://marticliment.com/unigetui/statistics";

    // Force-disable telemetry at runtime. Set to false to re-enable.
    private const bool FORCE_DISABLE_TELEMETRY = true;

    private static readonly Settings.K[] SettingsToSend =
    [
        Settings.K.DisableAutoUpdateWingetUI,
        Settings.K.EnableUniGetUIBeta,
        Settings.K.DisableSystemTray,
        Settings.K.DisableNotifications,
        Settings.K.DisableAutoCheckforUpdates,
        Settings.K.AutomaticallyUpdatePackages,
        Settings.K.AskToDeleteNewDesktopShortcuts,
        Settings.K.EnablePackageBackup_LOCAL,
        Settings.K.DoCacheAdminRights,
        Settings.K.DoCacheAdminRightsForBatches,
        Settings.K.ForceLegacyBundledWinGet,
        Settings.K.UseSystemChocolatey,
    ];

    // -------------------------------------------------------------------------

    public static async Task InitializeAsync()
    {
        try
        {
            if (FORCE_DISABLE_TELEMETRY)
            {
                Logger.Debug("[Telemetry] Forced disabled - skipping InitializeAsync");
                return;
            }
            //TELEMETRY REMOVED
            else
            {
                Logger.Warn(
                    $"[Telemetry] Call to /activity failed with error code {response.StatusCode}"
                );
            }
        }
        catch (Exception ex)
        {
            Logger.Error("[Telemetry] Hard crash when calling /activity");
            Logger.Error(ex);
        }
    }

    // -------------------------------------------------------------------------

    public static void InstallPackage(
        IPackage package,
        TEL_OP_RESULT status,
        TEL_InstallReferral source
    ) => PackageEndpoint(package, "install", status, source.ToString());

    public static void UpdatePackage(IPackage package, TEL_OP_RESULT status) =>
        PackageEndpoint(package, "update", status);

    public static void DownloadPackage(
        IPackage package,
        TEL_OP_RESULT status,
        TEL_InstallReferral source
    ) => PackageEndpoint(package, "download", status, source.ToString());

    public static void UninstallPackage(IPackage package, TEL_OP_RESULT status) =>
        PackageEndpoint(package, "uninstall", status);

    public static void PackageDetails(IPackage package, string eventSource) =>
        PackageEndpoint(package, "details", eventSource: eventSource);

    public static void SharedPackage(IPackage package, string eventSource) =>
        PackageEndpoint(package, "share", eventSource: eventSource);

    private static async void PackageEndpoint(
        IPackage package,
        string endpoint,
        TEL_OP_RESULT? result = null,
        string? eventSource = null
    )
    {
        try
        {
            if (FORCE_DISABLE_TELEMETRY)
            {
                Logger.Debug($"[Telemetry] Forced disabled - skipping /package/{endpoint}");
                return;
            }
            //TELEMETRY REMOVED
            else
            {
                Logger.Warn(
                    $"[Telemetry] Call to /package/{endpoint} failed with error code {response.StatusCode}"
                );
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[Telemetry] Hard crash when calling /package/{endpoint}");
            Logger.Error(ex);
        }
    }

    // -------------------------------------------------------------------------

    public static void ImportBundle(BundleFormatType type) =>
        BundlesEndpoint("import", type.ToString());

    public static void ExportBundle(BundleFormatType type) =>
        BundlesEndpoint("export", type.ToString());

    public static void ExportBatch() => BundlesEndpoint("export", "PS1_SCRIPT");

    private static async void BundlesEndpoint(string endpoint, string type)
    {
        try
        {
            if (FORCE_DISABLE_TELEMETRY)
            {
                Logger.Debug($"[Telemetry] Forced disabled - skipping /bundles/{endpoint}");
                return;
            }

            //TELEMETRY REMOVED
            else
            {
                Logger.Warn(
                    $"[Telemetry] Call to /bundles/{endpoint} failed with error code {response.StatusCode}"
                );
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[Telemetry] Hard crash when calling /bundles/{endpoint}");
            Logger.Error(ex);
        }
    }

    private static string GetRandomizedId()
    {
        string ID = Settings.GetValue(Settings.K.TelemetryClientToken);
        if (ID.Length != 64)
        {
            ID = CoreTools.RandomString(64);
            Settings.SetValue(Settings.K.TelemetryClientToken, ID);
        }

        return ID;
    }
}

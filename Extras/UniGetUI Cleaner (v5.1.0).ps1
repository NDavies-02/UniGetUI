# Check for admin rights and prompt UAC if elevation is needed
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

# First try to use Windows Terminal with PS7, otherwise use standard PowerShell 5.x window
if (-not $isAdmin) {
    Write-Host "This script requires administrative rights. You will be prompted by User Account Control. " -ForegroundColor Cyan
    $arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`""
    if (Get-Command wt.exe -ErrorAction SilentlyContinue) {
        Start-Process -FilePath "wt.exe" -ArgumentList "pwsh.exe $arguments" -Verb RunAs
    } else {
        Start-Process -FilePath "powershell.exe" -ArgumentList $arguments -Verb RunAs
    }
    exit
}

# List of deletion targets. Remove the language folders and corresponding .json files for languages you want to keep from the list.
$itemsToDelete = @(
    "C:/Program Files/UniGetUI/af-ZA",
    "C:/Program Files/UniGetUI/am-ET",
    "C:/Program Files/UniGetUI/ar-SA",
    "C:/Program Files/UniGetUI/as-IN",
    "C:/Program Files/UniGetUI/az-Latn-AZ",
    "C:/Program Files/UniGetUI/bg-BG",
    "C:/Program Files/UniGetUI/bn-IN",
    "C:/Program Files/UniGetUI/bs-Latn-BA",
    "C:/Program Files/UniGetUI/ca-ES",
    "C:/Program Files/UniGetUI/ca-Es-VALENCIA",
    "C:/Program Files/UniGetUI/cs-CZ",
    "C:/Program Files/UniGetUI/cy-GB",
    "C:/Program Files/UniGetUI/da-DK",
    "C:/Program Files/UniGetUI/de-DE",
    "C:/Program Files/UniGetUI/el-GR",
    "C:/Program Files/UniGetUI/es-ES",
    "C:/Program Files/UniGetUI/es-MX",
    "C:/Program Files/UniGetUI/et-EE",
    "C:/Program Files/UniGetUI/eu-ES",
    "C:/Program Files/UniGetUI/fa-IR",
    "C:/Program Files/UniGetUI/fi-FI",
    "C:/Program Files/UniGetUI/fil-PH",
    "C:/Program Files/UniGetUI/fr-CA",
    "C:/Program Files/UniGetUI/fr-FR",
    "C:/Program Files/UniGetUI/ga-IE",
    "C:/Program Files/UniGetUI/gd-gb",
    "C:/Program Files/UniGetUI/gl-ES",
    "C:/Program Files/UniGetUI/gu-IN",
    "C:/Program Files/UniGetUI/he-IL",
    "C:/Program Files/UniGetUI/hi-IN",
    "C:/Program Files/UniGetUI/hr-HR",
    "C:/Program Files/UniGetUI/hu-HU",
    "C:/Program Files/UniGetUI/hy-AM",
    "C:/Program Files/UniGetUI/id-ID",
    "C:/Program Files/UniGetUI/is-IS",
    "C:/Program Files/UniGetUI/it-IT",
    "C:/Program Files/UniGetUI/ja-JP",
    "C:/Program Files/UniGetUI/ka-GE",
    "C:/Program Files/UniGetUI/kk-KZ",
    "C:/Program Files/UniGetUI/km-KH",
    "C:/Program Files/UniGetUI/kn-IN",
    "C:/Program Files/UniGetUI/kok-IN",
    "C:/Program Files/UniGetUI/ko-KR",
    "C:/Program Files/UniGetUI/lb-LU",
    "C:/Program Files/UniGetUI/lo-LA",
    "C:/Program Files/UniGetUI/lt-LT",
    "C:/Program Files/UniGetUI/lv-LV",
    "C:/Program Files/UniGetUI/mi-NZ",
    "C:/Program Files/UniGetUI/mk-MK",
    "C:/Program Files/UniGetUI/ml-IN",
    "C:/Program Files/UniGetUI/mr-IN",
    "C:/Program Files/UniGetUI/ms-MY",
    "C:/Program Files/UniGetUI/mt-MT",
    "C:/Program Files/UniGetUI/nb-NO",
    "C:/Program Files/UniGetUI/ne-NP",
    "C:/Program Files/UniGetUI/nl-NL",
    "C:/Program Files/UniGetUI/nn-NO",
    "C:/Program Files/UniGetUI/or-IN",
    "C:/Program Files/UniGetUI/pa-IN",
    "C:/Program Files/UniGetUI/pl-PL",
    "C:/Program Files/UniGetUI/pt-BR",
    "C:/Program Files/UniGetUI/pt-PT",
    "C:/Program Files/UniGetUI/quz-PE",
    "C:/Program Files/UniGetUI/ro-RO",
    "C:/Program Files/UniGetUI/ru-RU",
    "C:/Program Files/UniGetUI/sk-SK",
    "C:/Program Files/UniGetUI/sl-SI",
    "C:/Program Files/UniGetUI/sq-AL",
    "C:/Program Files/UniGetUI/sr-Cyrl-BA",
    "C:/Program Files/UniGetUI/sr-Cyrl-RS",
    "C:/Program Files/UniGetUI/sr-Latn-RS",
    "C:/Program Files/UniGetUI/sv-SE",
    "C:/Program Files/UniGetUI/ta-IN",
    "C:/Program Files/UniGetUI/te-IN",
    "C:/Program Files/UniGetUI/th-TH",
    "C:/Program Files/UniGetUI/tr-TR",
    "C:/Program Files/UniGetUI/tt-RU",
    "C:/Program Files/UniGetUI/ug-CN",
    "C:/Program Files/UniGetUI/uk-UA",
    "C:/Program Files/UniGetUI/ur-PK",
    "C:/Program Files/UniGetUI/uz-Latn-UZ",
    "C:/Program Files/UniGetUI/vi-VN",
    "C:/Program Files/UniGetUI/zh-CN",
    "C:/Program Files/UniGetUI/zh-TW",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_af.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_ar.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_be.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_bg.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_bn.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_ca.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_cs.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_da.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_de.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_el.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_es.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_es-MX.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_et.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_fa.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_fi.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_fil.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_fr.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_gl.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_gu.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_he.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_hi.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_hr.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_hu.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_id.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_it.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_ja.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_ka.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_kn.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_ko.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_ku.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_lt.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_mk.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_mr.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_nb.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_nl.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_nn.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_pl.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_pt_BR.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_pt_PT.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_ro.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_ru.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_sa.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_si.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_sk.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_sl.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_sq.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_sr.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_sv.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_ta.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_tg.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_th.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_tr.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_ua.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_ur.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_vi.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_zh_CN.json",
    "C:/Program Files/UniGetUI/Assets/Languages/lang_zh_TW.json",
    "C:/Program Files/UniGetUI/WingetUI.exe",
    "C:/Program Files/UniGetUI/UniGetUI.Installer.exe",
    "$env:LOCALAPPDATA/UniGetUI/CachedLanguageFiles",
    "$env:LOCALAPPDATA/UniGetUI/CachedMedia",
    "$env:LOCALAPPDATA/UniGetUI/CachedMetadata",
    "$env:LOCALAPPDATA/UniGetUI/UniGetUI Updater.exe"
)

# 3. List targets and prompt confirmation
$itemsToDelete | ForEach-Object { Write-Host " - $_" }
Write-Host "`nThe above language files, along with the legacy WingetUI executable and cache files, will be deleted." -ForegroundColor Red
Write-Host "`nEnsure you have selected your preferred language in UniGetUI, and remove the corresponding folder and .json file from the script. English is retained by default." -ForegroundColor Red

$confirmation = Read-Host "`nType 'YES' to permanently delete these items (case-sensitive, any other input will cancel)."
if ($confirmation -cne "YES") {
    Write-Host "`nDeletion cancelled. Nothing has been deleted." -ForegroundColor Red
    Pause
    exit
}

# 4. Perform deletion
Write-Host "`nDeletion in progress...`n" -ForegroundColor Green

foreach ($item in $itemsToDelete) {
    if (Test-Path -LiteralPath $item) {
        Write-Host "Deleting: $item" -ForegroundColor Yellow
        try {
            # Remove '-WhatIf' when ready for live deletion
            Remove-Item -LiteralPath $item -Recurse -Force -ErrorAction Stop
            Write-Host "Successfully processed: $item" -ForegroundColor Green
        }
        catch {
            Write-Host "Failed to delete $item. Reason: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "Skipped (not found): $item" -ForegroundColor DarkGray
    }
}

# Restore empty cache folders
$itemsToRecreate = @(
    "$env:LOCALAPPDATA/UniGetUI/CachedLanguageFiles",
    "$env:LOCALAPPDATA/UniGetUI/CachedMedia",
    "$env:LOCALAPPDATA/UniGetUI/CachedMetadata"
)

Write-Host "`nRecreating empty cache folders...`n"
foreach ($folder in $itemsToRecreate) {
    New-Item -ItemType Directory -Path $folder -Force -ErrorAction Stop | Out-Null
}
Write-Host "`nOperation completed." -ForegroundColor Cyan
Pause
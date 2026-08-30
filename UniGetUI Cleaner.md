# UniGetUI Cleaner
The UniGetUI cleaner is a simple PowerShell script that removes unused language files, cache files, and installation/update files to clean up UniGetUI's installation folder and save space.

> [!IMPORTANT]  
> This script will only work correctly if UniGetUI is installed to `C:/Program Files/UniGetUI`. It will effectively do nothing if UniGetUI is installed elsewhere.

> [!WARNING]  
> If you use any language other than **English**, or use the legacy `WingetUI` executable, you MUST read the **Script Customisation** section. Do not add the translation files for English to the script.


## Usage
1. Download the latest version of the script [here](https://github.com/NDavies-02/UniGetUI/releases/latest/download/). It is the file named `UniGetUI.Cleanup.ps1`. If there is no such file, download the version from the source code [here](https://github.com/NDavies-02/UniGetUI/blob/main/Extras/UniGetUI%20Cleaner.ps1).
2. Navigate to the downloaded file (it does not matter where you save it).
3. Run the file -  it is reccomended to use `Right Click > Run with PowerShell`.
> [!TIP]  
> The script will first try to use PowerShell 7 in a Windows Terminal window. If Windows Terminal is not installed, the system PowerShell 5.1 will be used instead.
4. You will be prompted by User Account Control if the script is run without elevation. Elevation is needed to access `C:/Program Files`.  
5. The list of files that will be targeted for deletion will be displayed. To confirm, type `YES` then press ENTER. Any other input will cancel the operation.
6. Upon confirmation, the files will be deleted. Files already deleted or otherwise not found will be skipped.
7. Press ENTER to close the Windows Terminal/PowerShell window.

## Reversal
The only way to restore the deleted language files and legacy `WingetUI` executable is to reinstall UniGetUI (settings are preserved, there is no need to uninstall first).  
By extension, **updates restore the files too**. Therefore you should run the script again after a reinstall/update if you wish to remove the files.

## Script Customisation
You may wish to edit the list of files to be deleted. To do so, open the script in a text editor of your choice (`Right Click > Open with...`).  
The only section you need to customise is the file list, starting after the line that reads `$itemsToDelete = @(`.
> [!TIP]  
> Every item in the list should be a file or folder path in "quotes", followed by a comma (unless it is the last item in the list).

### Retain legacy executable
Remove `"C:/Program Files/UniGetUI/WingetUI.exe",` from the list.
### Retain a particular language
1. Identify the [language code](https://en.wikipedia.org/wiki/List_of_ISO_639_language_codes) for the language you wish to keep. For example, French is `fr-FR`.
2. Remove the corresponding folder from the list, such as `"C:/Program Files/UniGetUI/fr-FR",`.
3. Remove the language asset file from the list, such as `"C:/Program Files/UniGetUI/Assets/Languages/lang_fr.json",`.

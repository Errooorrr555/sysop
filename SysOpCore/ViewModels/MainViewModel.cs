using SysOpCore.Models;
using SysOpCore.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SysOpCore.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly CommandRunnerService _runner = new();
    private readonly EnvironmentService _environment = new();
    private readonly NativeSessionService _nativeSession = new();
    private string _output = "Ready.";
    private string _selectedNav = "Dashboard";
    private bool _alwaysOnTop;
    private bool _useMonospaceFont = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> Navigation { get; } = ["Dashboard", "System Repair", "Security & Access", "Boot & Drivers", "Embedded Apps"];
    public ObservableCollection<ActionCard> Cards { get; } = [];

    public string Header => $"Environment: {(IsWinRe ? "WinRE" : "Normal")} | Current User: {CurrentUser} | Secure Boot Status: {SecureBoot}";
    public bool IsWinRe => _environment.IsWinRe();
    public bool ReducedAnimations => IsWinRe;
    public string CurrentUser => _environment.CurrentUser;
    public string SecureBoot => _environment.SecureBootStatus();

    public string SelectedNav
    {
        get => _selectedNav;
        set
        {
            _selectedNav = value;
            RefreshCards();
            OnPropertyChanged();
        }
    }

    public string Output
    {
        get => _output;
        set
        {
            _output = value;
            OnPropertyChanged();
        }
    }

    public string WinReSavePath { get; set; } = @"C:\Program Files\SR";

    public bool AlwaysOnTop
    {
        get => _alwaysOnTop;
        set
        {
            _alwaysOnTop = value;
            OnPropertyChanged();
        }
    }

    public bool UseMonospaceFont
    {
        get => _useMonospaceFont;
        set
        {
            _useMonospaceFont = value;
            OnPropertyChanged();
        }
    }

    public MainViewModel() => RefreshCards();

    public async Task ExecuteCardAsync(ActionCard card)
    {
        Output = $"Running: {card.Title}";

        if (card.CommandKey == "logoff")
        {
            var success = _nativeSession.ForceLogoffCurrentSession();
            Output = success ? "ExitWindowsEx logoff request submitted." : "ExitWindowsEx failed.";
            return;
        }

        var result = card.CommandKey switch
        {
            "boottore" => await _runner.RunAsync("reagentc", "/boottore"),
            "uac" => await _runner.RunAsync("reg", "add HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System /v EnableLUA /t REG_DWORD /d 1 /f"),
            "sfc" => await _runner.RunAsync("sfc", "/scannow"),
            "dism" => await _runner.RunAsync("dism", "/Online /Cleanup-Image /RestoreHealth"),
            "disable-test" => await _runner.RunAsync("bcdedit", "/set testsigning off"),
            "ctrl-alt-del" => await _runner.RunAsync("reg", "add HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System /v DisableCAD /t REG_DWORD /d 0 /f"),
            "fix-assoc" => await _runner.RunAsync("cmd", "/c assoc .exe=exefile && assoc .msi=Msi.Package && assoc .lnk=lnkfile && assoc .reg=regfile"),
            "fixmbr" => await _runner.RunAsync("bootrec", "/fixmbr"),
            "fixboot" => await _runner.RunAsync("bootrec", "/fixboot"),
            "taskmgr" => await _runner.RunAsync("tasklist", ""),
            _ => "Module stub ready. Implementing internal engine for this card is the next step."
        };

        Output = string.IsNullOrWhiteSpace(result) ? "Done." : result;
    }

    private void RefreshCards()
    {
        Cards.Clear();
        foreach (var card in GetCards(SelectedNav))
        {
            Cards.Add(card);
        }
    }

    private static IEnumerable<ActionCard> GetCards(string nav) => nav switch
    {
        "System Repair" =>
        [
            new() { Title = "User Logout", Description = "Force immediate logoff using ExitWindowsEx.", CommandKey = "logoff", IsDangerous = true },
            new() { Title = "Enter WinRE", Description = "Reboot to Windows Recovery Environment.", CommandKey = "boottore" },
            new() { Title = "Run Dialogue (SYSTEM)", Description = "Custom run launcher with elevated SYSTEM context.", CommandKey = "run-system" },
            new() { Title = "Restore Russian Language", Description = "Re-apply RU keyboard layout and MUI defaults.", CommandKey = "lang-ru" },
            new() { Title = "Repair System Fonts", Description = "Rebuild font cache and restore default font registry keys.", CommandKey = "fonts-restore" },
            new() { Title = "Enable UAC", Description = "Restore maximum UAC enforcement.", CommandKey = "uac" },
            new() { Title = "SFC Scannow", Description = "Run integrity scan in embedded output panel.", CommandKey = "sfc" },
            new() { Title = "Restore LogonUI", Description = "Revert login screen branding to defaults.", CommandKey = "logonui" },
            new() { Title = "Disable Test Mode", Description = "Disable TESTSIGNING and remove watermark.", CommandKey = "disable-test" },
            new() { Title = "Fix Fonts (Encoding)", Description = "Fix missing character rendering/encoding fallback.", CommandKey = "fonts-encoding" },
            new() { Title = "Emergency Recovery", Description = "Run DISM health restore chain.", CommandKey = "dism" },
            new() { Title = "Driver Cleanup", Description = "List and remove unused driver packages.", CommandKey = "driver-cleanup" },
            new() { Title = "Associations Manager", Description = "Restore .exe/.msi/.lnk/.reg associations.", CommandKey = "fix-assoc" }
        ],
        "Security & Access" =>
        [
            new() { Title = "User Account Management", Description = "Create/reset local users with admin role support.", CommandKey = "user-manager" },
            new() { Title = "Sticky Keys Backdoor Mitigation", Description = "Toggle utilman/sethc default or recovery cmd mode.", CommandKey = "sticky-toggle", IsDangerous = true },
            new() { Title = "Take Ownership", Description = "Grant Administrators full access for a selected path.", CommandKey = "takeown" },
            new() { Title = "Require CTRL+ALT+DEL", Description = "Enable secure attention sequence at logon.", CommandKey = "ctrl-alt-del" }
        ],
        "Boot & Drivers" =>
        [
            new() { Title = "WinRE Save/Load Utility", Description = "Capture/apply WinRE state to configured save folder.", CommandKey = "winre-state" },
            new() { Title = "MBR Repair", Description = "Run bootrec /fixmbr and /fixboot for legacy BIOS.", CommandKey = "fixmbr", IsDangerous = true },
            new() { Title = "Disk Removal", Description = "Diskpart wrapper for clean/delete volume operations.", CommandKey = "diskpart-ui", IsDangerous = true }
        ],
        "Embedded Apps" =>
        [
            new() { Title = "Task Manager", Description = "Internal process list with task controls.", CommandKey = "taskmgr" },
            new() { Title = "Registry Editor", Description = "Internal registry tree and value editor.", CommandKey = "regedit" },
            new() { Title = "File Explorer", Description = "Dual-pane file manager for explorer-less systems.", CommandKey = "file-explorer" },
            new() { Title = "Browser", Description = "Emergency WebView2 downloader for drivers/tools.", CommandKey = "browser" },
            new() { Title = "System Cleanup", Description = "Clean temp/update cache/prefetch directories.", CommandKey = "cleanup" },
            new() { Title = "Startup Manager", Description = "Manage startup entries from Run keys and startup folders.", CommandKey = "startup" }
        ],
        _ =>
        [
            new() { Title = "Environment Overview", Description = "Live operating context and operational readiness.", CommandKey = "status" },
            new() { Title = "Operational Warnings", Description = "Highlights constrained features in WinRE sessions.", CommandKey = "warning" }
        ]
    };

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

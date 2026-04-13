using Microsoft.Win32;
using System.Security.Principal;

namespace SysOpCore.Services;

public sealed class EnvironmentService
{
    public bool IsWinRe()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MiniNT");
            return key is not null;
        }
        catch
        {
            return false;
        }
    }

    public string CurrentUser => WindowsIdentity.GetCurrent().Name;

    public string SecureBootStatus()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
            var value = key?.GetValue("UEFISecureBootEnabled");
            return (value is int i && i == 1) ? "On" : "Off";
        }
        catch
        {
            return "Unknown";
        }
    }
}

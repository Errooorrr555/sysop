using System.Runtime.InteropServices;

namespace SysOpCore.Services;

public sealed class NativeSessionService
{
    private const uint EwxLogoff = 0x00000000;
    private const uint EwxForce = 0x00000004;
    private const uint ShtdnReasonMajorOther = 0x00000000;
    private const uint ShtdnReasonMinorOther = 0x00000000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

    public bool ForceLogoffCurrentSession() => ExitWindowsEx(EwxLogoff | EwxForce, ShtdnReasonMajorOther | ShtdnReasonMinorOther);
}

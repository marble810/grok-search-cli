using System.Runtime.InteropServices;

namespace XaiSearchCli.Configuration;

public static class StdinDetector
{
    public static bool HasStdinInput => IsStdinRedirectedWithData();

    private static bool IsStdinRedirectedWithData()
    {
        if (!Console.IsInputRedirected)
            return false;

        if (!OperatingSystem.IsWindows())
            return true;

        var handle = GetStdHandle(STD_INPUT_HANDLE);
        if (handle == INVALID_HANDLE_VALUE || handle == 0)
            return false;

        switch (GetFileType(handle))
        {
            case FILE_TYPE_PIPE:
                return HasPipeData(handle);
            case FILE_TYPE_CHAR:
                return false;
            default:
                return true;
        }
    }

    private static bool HasPipeData(nint pipeHandle)
    {
        if (PeekNamedPipe(pipeHandle, null, 0, out _, out var totalBytesAvail, out _))
            return totalBytesAvail > 0;
        return true; // on error, assume data to avoid hanging
    }

    private const int STD_INPUT_HANDLE = -10;
    private static readonly nint INVALID_HANDLE_VALUE = new(-1);
    private const int FILE_TYPE_PIPE = 3;
    private const int FILE_TYPE_CHAR = 2;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int GetFileType(nint hFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PeekNamedPipe(
        nint hNamedPipe,
        [Out] byte[]? lpBuffer,
        uint nBufferSize,
        out uint lpBytesRead,
        out uint lpTotalBytesAvail,
        out uint lpBytesLeftThisMessage);
}

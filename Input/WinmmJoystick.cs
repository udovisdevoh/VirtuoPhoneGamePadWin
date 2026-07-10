using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace VirtuoPhone.Input;

/// <summary>
/// Zero-dependency reader for the classic Windows joystick API (winmm <c>joyGetPosEx</c>). Enough for arcade
/// sticks like the Mayflash F500 in DirectInput mode: up to 32 buttons, X/Y/Z/R/U/V axes and a POV hat.
///
/// This is the low-level device layer only. <c>IControllerInput</c> (next) will sit on top and add the
/// logical action mapping + remapping; keep this class a thin, swappable HID/joystick source.
/// </summary>
public static class WinmmJoystick
{
    [StructLayout(LayoutKind.Sequential)]
    private struct JOYINFOEX
    {
        public uint dwSize, dwFlags;
        public uint dwXpos, dwYpos, dwZpos, dwRpos, dwUpos, dwVpos;
        public uint dwButtons, dwButtonNumber, dwPOV;
        public uint dwReserved1, dwReserved2;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct JOYCAPS
    {
        public ushort wMid, wPid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szPname;
        public uint wXmin, wXmax, wYmin, wYmax, wZmin, wZmax;
        public uint wNumButtons;
        public uint wPeriodMin, wPeriodMax;
        public uint wRmin, wRmax, wUmin, wUmax, wVmin, wVmax;
        public uint wCaps, wMaxAxes, wNumAxes, wMaxButtons;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szRegKey;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szOEMVxD;
    }

    [DllImport("winmm.dll")] private static extern uint joyGetNumDevs();
    [DllImport("winmm.dll", CharSet = CharSet.Unicode)] private static extern uint joyGetDevCapsW(UIntPtr id, ref JOYCAPS caps, uint cb);
    [DllImport("winmm.dll")] private static extern uint joyGetPosEx(uint id, ref JOYINFOEX info);

    private const uint JOYERR_NOERROR = 0;
    private const uint JOY_RETURNALL = 0x000000FF;
    private const uint JOYCAPS_HASPOV = 0x0010;
    private const uint POV_CENTERED = 0xFFFF;

    /// <summary>Static capabilities of a joystick slot.</summary>
    public readonly record struct Caps(string Name, int Buttons, int Axes, bool HasPov);

    /// <summary>A polled snapshot. Axes are normalized to -1..1; <see cref="PovDegrees"/> is -1 when centered.</summary>
    public readonly record struct State(uint Buttons, float X, float Y, int PovDegrees);

    private readonly record struct AxisRange(uint Xmin, uint Xmax, uint Ymin, uint Ymax);
    private static readonly ConcurrentDictionary<int, AxisRange> axisCache = new();

    /// <summary>Number of joystick slots the driver supports (not how many are plugged in).</summary>
    public static int MaxDevices => (int)joyGetNumDevs();

    public static bool TryGetCaps(int id, out Caps caps)
    {
        var jc = new JOYCAPS();
        caps = default;
        if (joyGetDevCapsW((UIntPtr)(uint)id, ref jc, (uint)Marshal.SizeOf<JOYCAPS>()) != JOYERR_NOERROR) return false;
        axisCache[id] = new AxisRange(jc.wXmin, jc.wXmax, jc.wYmin, jc.wYmax);
        caps = new Caps(jc.szPname, (int)jc.wNumButtons, (int)jc.wNumAxes, (jc.wCaps & JOYCAPS_HASPOV) != 0);
        return true;
    }

    /// <summary>Poll a joystick slot. Returns false if nothing is plugged into that slot.</summary>
    public static bool TryPoll(int id, out State state)
    {
        var info = new JOYINFOEX { dwSize = (uint)Marshal.SizeOf<JOYINFOEX>(), dwFlags = JOY_RETURNALL };
        state = default;
        if (joyGetPosEx((uint)id, ref info) != JOYERR_NOERROR) return false;

        if (!axisCache.TryGetValue(id, out var range))
        {
            TryGetCaps(id, out _);                       // fills the cache
            axisCache.TryGetValue(id, out range);
        }

        float x = Normalize(info.dwXpos, range.Xmin, range.Xmax);
        float y = Normalize(info.dwYpos, range.Ymin, range.Ymax);
        int pov = info.dwPOV == POV_CENTERED ? -1 : (int)(info.dwPOV / 100);   // centidegrees -> degrees
        state = new State(info.dwButtons, x, y, pov);
        return true;
    }

    private static float Normalize(uint v, uint min, uint max)
    {
        if (max <= min) return 0f;
        float t = (v - min) / (float)(max - min);        // 0..1
        return Math.Clamp(t * 2f - 1f, -1f, 1f);         // -1..1  (min = up/left = -1)
    }

    /// <summary>Map a poll snapshot to one of the 9 grid cells (POV hat preferred, else the X/Y axes).</summary>
    public static string ToDirection(in State s, float threshold = 0.5f)
    {
        if (s.PovDegrees >= 0)
            return s.PovDegrees switch
            {
                0 => "Up", 45 => "Up-Right", 90 => "Right", 135 => "Down-Right",
                180 => "Down", 225 => "Down-Left", 270 => "Left", 315 => "Up-Left",
                _ => $"POV {s.PovDegrees}°",
            };

        string h = s.X < -threshold ? "Left" : s.X > threshold ? "Right" : "";
        string v = s.Y < -threshold ? "Up" : s.Y > threshold ? "Down" : "";
        if (v == "" && h == "") return "Center";
        return v == "" ? h : h == "" ? v : $"{v}-{h}";
    }
}

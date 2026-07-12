using System.Collections.Generic;
using NAudio.CoreAudioApi;

namespace VirtuoPhone;

/// <summary>Lists the active WASAPI render (output) endpoints for the audio-settings device picker.</summary>
public static class AudioDevices
{
    public readonly record struct Device(string Id, string Name);

    public static List<Device> List()
    {
        var result = new List<Device>();
        try
        {
            using var en = new MMDeviceEnumerator();
            foreach (var d in en.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                result.Add(new Device(d.ID, d.FriendlyName));
                d.Dispose();
            }
        }
        catch { /* no audio devices / no COM — return whatever we have */ }
        return result;
    }
}

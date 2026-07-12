using System;
using VirtuoPhone.Config;

namespace VirtuoPhone;

/// <summary>
/// Chooses which <see cref="ISoundPool"/> implementation instruments create, and holds the current
/// <see cref="AudioSettings"/> the default (NAudio/WASAPI) engine reads. Headless code or tests can swap the
/// factory, e.g. <c>AudioBackend.Factory = polyphony =&gt; new DummySoundPool(polyphony);</c> before building an
/// <see cref="VirtuoPhone.Models.Instrument"/>. Changing <see cref="Settings"/> takes effect the next time an
/// instrument (and thus its sound pool) is built.
/// </summary>
public static class AudioBackend
{
    public static AudioSettings Settings { get; set; } = new AudioSettings();

    public static Func<int, ISoundPool> Factory { get; set; } = polyphony => new NAudioSoundPool(polyphony, Settings);

    public static ISoundPool Create(int polyphony) => Factory(polyphony);
}

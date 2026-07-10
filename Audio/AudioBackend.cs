using System;

namespace VirtuoPhone;

/// <summary>
/// Chooses which <see cref="ISoundPool"/> implementation instruments create.
/// Defaults to the real NAudio/WASAPI engine; headless code or tests can swap it, e.g.
/// <c>AudioBackend.Factory = polyphony =&gt; new DummySoundPool(polyphony);</c> before building an
/// <see cref="VirtuoPhone.Models.Instrument"/>.
/// </summary>
public static class AudioBackend
{
    public static Func<int, ISoundPool> Factory { get; set; } = polyphony => new NAudioSoundPool(polyphony);

    public static ISoundPool Create(int polyphony) => Factory(polyphony);
}

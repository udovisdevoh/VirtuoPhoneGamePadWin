using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Sample
{
    private int originalPitch;

    private int resourceId;

    private int soundId;

    private float volume;

    /**
    * @param note note
    * @param resourceId resource's id
    */
    public Sample(Note note, int resourceId, float volume)
    {
        originalPitch = note.getPitch();
        this.resourceId = resourceId;
        this.volume = volume;
    }

    public int GetOriginalPitch()
    {
        return originalPitch;
    }

    public float GetPitchMultiplicator(int desiredPitch, float pitchBend)
    {
        if (desiredPitch == originalPitch && pitchBend == 0f)
        return 1.0f;
        else
        return (float)Math.Pow(1.0594632, (double)(desiredPitch - originalPitch) + pitchBend);
    }

    public int GetResourceId()
    {
        return resourceId;
    }

    public void setSoundId(int soundId)
    {
        this.soundId = soundId;
    }

    public int GetSoundId()
    {
        return soundId;
    }

    public float GetVolume()
    {
        return volume;
    }

    // Java-style wrappers for converted code
    public int getOriginalPitch() => GetOriginalPitch();
    public float getPitchMultiplicator(int desiredPitch, float pitchBend) => GetPitchMultiplicator(desiredPitch, pitchBend);
    public int getResourceId() => GetResourceId();
    public int getSoundId() => GetSoundId();
    public float getVolume() => GetVolume();
}

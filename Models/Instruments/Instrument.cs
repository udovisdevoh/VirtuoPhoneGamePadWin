using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public abstract class Instrument : IEnumerable<Sample>
{
    private Random random;

    private MultiSampleSet[] multiSampleList;

    private Drone? drone;

    private ISoundPool soundPool;

    private HashSet<Sample> sampleSet;

    private int minPitchToPlay;

    private PlayingStringMemory playingStringMemory;

    private int stringCount;

    private int polyphony;

    private bool isMuteOnChangeFretSameString;

    private bool isAutoLoopKeepNoteUntilNewNote;

    private bool isPitchBend;

    private bool isAutoLoop;

    private float attackSeconds;

    private float releaseSeconds;

    private bool isMonophonic;

    public Instrument()
    {
        random = new Random();
        stringCount = BuildStringCount();
        isMuteOnChangeFretSameString = BuildIsMuteOnChangeFretSameString();
        isAutoLoop = BuildIsAutoLoop();
        isAutoLoopKeepNoteUntilNewNote = BuildIsAutoLoopKeepNoteUntilNewNote();
        isPitchBend = BuildIsPitchBend();
        attackSeconds = BuildAttackSeconds();
        releaseSeconds = BuildReleaseSeconds();
        isMonophonic = BuildIsMonophonic();
        polyphony = stringCount * 2;
        soundPool = AudioBackend.Create(polyphony);
        //streamIdByStringBeingPlayed = new int[AppController.STRING_COUNT];
        multiSampleList = new MultiSampleSet[128];
        sampleSet = new HashSet<Sample>();
        playingStringMemory = new PlayingStringMemory(stringCount);
        LoadSamples();

        for (int i = 0; i < multiSampleList.Length; i++)
        {
            if (multiSampleList[i] != null)
            {
                foreach (Sample sample in multiSampleList[i])
                {
                    int soundId = soundPool.Load(sample.GetResourceId(), 1);
                    sample.SetSoundId(soundId);
                }
            }
        }

        InterpolateBlankSamples();
        minPitchToPlay = BuildMinPitchToPlay();

        LoadDrone();
        if (drone != null)
        {
            drone.IsLazyHarmonic(BuildIsLazyHarmonicDrone());
            drone.IsMinimizePitchShift(BuildIsDroneMinimizePitchShift());
            drone.IsFixed(BuildIsDroneFixed());
        }
    }

    protected abstract bool BuildIsAutoLoopKeepNoteUntilNewNote();

    // Optional per-instrument envelope in seconds; 0 = no fade (default). Only instruments that need
    // smoothing (e.g. the bowed violin) override these — plucked/percussive ones stay hard-edged.
    protected virtual float BuildAttackSeconds() => 0f;

    protected virtual float BuildReleaseSeconds() => 0f;

    // Monophonic instruments (e.g. bagpipes chanter) sound one melody note at a time. Default: polyphonic.
    protected virtual bool BuildIsMonophonic() => false;

    // A fixed drone ignores joystick chord changes and holds its own note (like real bagpipes); it only
    // moves when the layout is modulated. Default: the drone follows the chord (e.g. the sitar tampura).
    protected virtual bool BuildIsDroneFixed() => false;

    protected abstract bool BuildIsDroneMinimizePitchShift();

    protected abstract bool BuildIsLazyHarmonicDrone();

    protected abstract bool BuildIsPitchBend();

    protected abstract void LoadDrone();

    protected abstract bool BuildIsAutoLoop();

    protected abstract bool BuildIsMuteOnChangeFretSameString();

    protected abstract int BuildStringCount();

    /**
    * @return below that pitch, remain silent
    */
    protected abstract int BuildMinPitchToPlay();

    protected void AddSample(Note note, int resourceId)
    {
        AddSample(note, resourceId, 1.0f);
    }

    /**
    * @param note note
    * @param resourceId resource id
    */
    protected void AddSample(Note note, int resourceId, float volume)
    {
        Sample sample = new Sample(note, resourceId, volume);

        int pitch = sample.GetOriginalPitch();

        if (pitch > 127 || pitch < 0)
        {
            return;
        }

        if (multiSampleList[pitch] == null)
        {
            multiSampleList[pitch] = new MultiSampleSet();
        }

        multiSampleList[pitch].AddSample(sample);
        sampleSet.Add(sample);
    }

    protected void setDrone(Drone drone)
    {
        this.drone = drone;
        int soundId = soundPool.Load(drone.GetSample().GetResourceId(), 1);
        drone.GetSample().SetSoundId(soundId);
    }

    protected abstract void LoadSamples();

    private void InterpolateBlankSamples()
    {
        for (int i = 0; i < 128; i++)
        {
            if (multiSampleList[i] == null)
            {
                if (i > 0 && multiSampleList[i - 1] != null)
                {
                    multiSampleList[i] = multiSampleList[i - 1];
                }
                else if (i == 0)
                {
                    MultiSampleSet? lowestMultiSampleSet = GetLowestMultiSampleSet();
                    if (lowestMultiSampleSet != null)
                    {
                        multiSampleList[i] = lowestMultiSampleSet;
                    }
                }
            }
        }
    }

    private MultiSampleSet? GetLowestMultiSampleSet()
    {
        for (int i = 0; i < 128; i++)
        {
            if (multiSampleList[i] != null)
            {
                return multiSampleList[i];
            }
        }
        return null;
    }

    public void Release()
    {
        soundPool.Release();
    }

    public IEnumerator<Sample> GetEnumerator()
    {
        return sampleSet.GetEnumerator();
    }

    public int Play(int desiredPitch, int stringId, float pitchBend)
    {
        while (desiredPitch < 0)
        {
            desiredPitch += 12;
        }

        while (desiredPitch > 128)
        {
            desiredPitch -= 12;
        }

        if (desiredPitch < minPitchToPlay)
        {
            return 0;
        }

        Sample sample = multiSampleList[desiredPitch].GetRandomSample(random);
        float rate = sample.GetPitchMultiplicator(desiredPitch, pitchBend);

        if (isMuteOnChangeFretSameString)
        {
            int previousStreamIdSameString = playingStringMemory.GetStreamIdFromString(stringId);
            if (previousStreamIdSameString != 0)
            soundPool.Stop(previousStreamIdSameString);
        }

        int streamId;

        if (isAutoLoop)
        {
            streamId = soundPool.Play(sample.GetSoundId(), sample.GetVolume(), sample.GetVolume(), 1, -1, rate, attackSeconds, releaseSeconds);
        }
        else
        {
            streamId = soundPool.Play(sample.GetSoundId(), sample.GetVolume(), sample.GetVolume(), 1, 0, rate, attackSeconds, releaseSeconds);
        }


        if (isMuteOnChangeFretSameString)
        playingStringMemory.Remember(stringId, streamId);

        return streamId;
    }

    public int Play(Note note, int stringId, float pitchBend)
    {
        if (note == null || note.GetPitch() == -1)
        {
            return 0;
        }

        return Play(note.GetPitch(), stringId, pitchBend);
    }

    public int GetStringCount()
    {
        return stringCount;
    }

    public int GetMinPitchToPlay()
    {
        return minPitchToPlay;
    }

    public float GetAttackSeconds() => attackSeconds;

    public float GetReleaseSeconds() => releaseSeconds;

    public bool IsMonophonic() => isMonophonic;

    public bool IsAutoLoop()
    {
        return isAutoLoop;
    }

    public void Stop(int streamId)
    {
        soundPool.Stop(streamId);
    }

    public Drone? GetDrone()
    {
        return drone;
    }

    public void StopDrone()
    {
        if (drone != null)
        {
            drone.Stop(soundPool);
        }
    }

    public bool IsPitchBend()
    {
        return isPitchBend;
    }

    public void SetStreamPitch(int streamId, Note note, float pitchBend)
    {
        if (streamId > 0)
        {
            int desiredPitch = note.GetPitch();

            while (desiredPitch < 0)
            {
                desiredPitch += 12;
            }

            while (desiredPitch > 128)
            {
                desiredPitch -= 12;
            }

            if (desiredPitch < minPitchToPlay)
            {
                return;
            }

            Sample sample = multiSampleList[desiredPitch].GetRandomSample(random);
            float rate = sample.GetPitchMultiplicator(desiredPitch, pitchBend);

            soundPool.SetRate(streamId, rate);
        }
    }

    public ISoundPool GetSoundPool()
    {
        return soundPool;
    }

    public bool IsAutoLoopKeepNoteUntilNewNote()
    {
        return isAutoLoopKeepNoteUntilNewNote;
    }

    public void StopAllNotes(PointerMemory pointerMemory)
    {
        foreach (int streamIdToMute in pointerMemory.GetStreamList())
        {
            Stop(streamIdToMute);
        }
        pointerMemory.ClearStreamList();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

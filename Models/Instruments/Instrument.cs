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

    private Drone drone;

    private SoundPool soundPool;

    private HashSet<Sample> sampleSet;

    private int minPitchToPlay;

    private PlayingStringMemory playingStringMemory;

    private int stringCount;

    private int polyphony;

    private bool isMuteOnChangeFretSameString;

    private bool isAutoLoopKeepNoteUntilNewNote;

    private bool isPitchBend;

    private bool isAutoLoop;

    public Instrument()
    {
        random = new Random();
        stringCount = BuildStringCount();
        isMuteOnChangeFretSameString = BuildIsMuteOnChangeFretSameString();
        isAutoLoop = BuildIsAutoLoop();
        isAutoLoopKeepNoteUntilNewNote = BuildIsAutoLoopKeepNoteUntilNewNote();
        isPitchBend = BuildIsPitchBend();
        polyphony = stringCount * 2;
        soundPool = new SoundPool(polyphony, AudioManager.STREAM_MUSIC, 0);
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
        }
    }

    protected virtual bool BuildIsAutoLoopKeepNoteUntilNewNote() => false;

    protected virtual bool BuildIsDroneMinimizePitchShift() => false;

    protected virtual bool BuildIsLazyHarmonicDrone() => false;

    protected virtual bool BuildIsPitchBend() => false;

    protected virtual void LoadDrone() { }

    protected virtual bool BuildIsAutoLoop() => false;

    protected virtual bool BuildIsMuteOnChangeFretSameString() => false;

    protected virtual int BuildStringCount() => 6;

    /**
    * @return below that pitch, remain silent
    */
    protected virtual int BuildMinPitchToPlay() => 0;

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

    protected virtual void LoadSamples() { }

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
                    multiSampleList[i] = GetLowestMultiSampleSet();
                }
            }
        }
    }

    /**
    * @return sample with the lowest pitch
    */
    private MultiSampleSet GetLowestMultiSampleSet()
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
            streamId = soundPool.Play(sample.GetSoundId(), sample.GetVolume(), sample.GetVolume(), 1, -1, rate);
        }
        else
        {
            streamId = soundPool.Play(sample.GetSoundId(), sample.GetVolume(), sample.GetVolume(), 1, 0, rate);
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

    public bool IsAutoLoop()
    {
        return isAutoLoop;
    }

    public void Stop(int streamId)
    {
        soundPool.Stop(streamId);
    }

    public Drone GetDrone()
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

    public SoundPool GetSoundPool()
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

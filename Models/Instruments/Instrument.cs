using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models.Instruments;
















/**
* @author Guillaume
* Instrucment (collection of samples)
*/
public abstract class Instrument : IEnumerable<Sample>
{
    private Random random;

    private MultiSampleSet[] multiSampleList;

    private Drone drone = null;

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

    public Instrument(Context context)
    {
        random = new Random();
        stringCount = buildStringCount();
        isMuteOnChangeFretSameString = buildIsMuteOnChangeFretSameString();
        isAutoLoop = buildIsAutoLoop();
        isAutoLoopKeepNoteUntilNewNote = buildIsAutoLoopKeepNoteUntilNewNote();
        isPitchBend = buildIsPitchBend();
        polyphony = stringCount * 2;
        soundPool = new SoundPool(polyphony, AudioManager.STREAM_MUSIC, 0);
        //streamIdByStringBeingPlayed = new int[AppController.STRING_COUNT];
        multiSampleList = new MultiSampleSet[128];
        sampleSet = new HashSet<Sample>();
        playingStringMemory = new PlayingStringMemory(stringCount);
        loadSamples();

        for (int i = 0; i < multiSampleList.length; i++)
        {
            if (multiSampleList[i] != null)
            {
                for (Sample sample : multiSampleList[i])
                {
                    int soundId = soundPool.load(context, sample.getResourceId(), 1);
                    sample.setSoundId(soundId);
                }
            }
        }

        interpolateBlankSamples();
        minPitchToPlay = buildMinPitchToPlay();

        loadDrone(context);
        if (drone != null)
        {
            drone.isLazyHarmonic(buildIsLazyHarmonicDrone());
            drone.isMinimizePitchShift(buildIsDroneMinimizePitchShift());
        }
    }

    protected abstract bool buildIsAutoLoopKeepNoteUntilNewNote();

    protected abstract bool buildIsDroneMinimizePitchShift();

    protected abstract bool buildIsLazyHarmonicDrone();

    protected abstract bool buildIsPitchBend();

    protected abstract void loadDrone(Context context);

    protected abstract bool buildIsAutoLoop();

    protected abstract bool buildIsMuteOnChangeFretSameString();

    protected abstract int buildStringCount();

    /**
    * @return below that pitch, remain silent
    */
    protected abstract int buildMinPitchToPlay();

    protected void addSample(Note note, int resourceId)
    {
        addSample(note, resourceId, 1.0f);
    }

    /**
    * @param note note
    * @param resourceId resource id
    */
    protected void addSample(Note note, int resourceId, float volume)
    {
        Sample sample = new Sample(note, resourceId, volume);

        int pitch = sample.getOriginalPitch();

        if (pitch > 127 || pitch < 0)
        return;

        if (multiSampleList[pitch] == null)
        multiSampleList[pitch] = new MultiSampleSet();

        multiSampleList[pitch].addSample(sample);
        sampleSet.add(sample);
    }

    protected void setDrone(Drone drone, Context context)
    {
        this.drone = drone;
        int soundId = soundPool.load(context, drone.getSample().getResourceId(), 1);
        drone.getSample().setSoundId(soundId);
    }

    protected abstract void loadSamples();

    private void interpolateBlankSamples()
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
                    multiSampleList[i] = getLowestMultiSampleSet();
                }
            }
        }
    }

    /**
    * @return sample with the lowest pitch
    */
    private MultiSampleSet getLowestMultiSampleSet()
    {
        for (int i = 0; i < 128; i++)
        if (multiSampleList[i] != null)
        return multiSampleList[i];
        return null;
    }

    public void release()
    {
        soundPool.release();
    }

    public IEnumerator<Sample> iterator()
    {
        return sampleSet.GetEnumerator();
    }

    public int play(int desiredPitch, int stringId, float pitchBend)
    {
        while (desiredPitch < 0)
        desiredPitch += 12;

        while (desiredPitch > 128)
        desiredPitch -= 12;

        if (desiredPitch < minPitchToPlay)
        return 0;

        Sample sample = multiSampleList[desiredPitch].getRandomSample(random);
        float rate = sample.getPitchMultiplicator(desiredPitch, pitchBend);

        if (isMuteOnChangeFretSameString)
        {
            int previousStreamIdSameString = playingStringMemory.getStreamIdFromString(stringId);
            if (previousStreamIdSameString != 0)
            soundPool.stop(previousStreamIdSameString);
        }

        int streamId;

        if (isAutoLoop)
        streamId = soundPool.play(sample.getSoundId(), sample.getVolume(), sample.getVolume(), 1, -1, rate);
        else
        streamId = soundPool.play(sample.getSoundId(), sample.getVolume(), sample.getVolume(), 1, 0, rate);


        if (isMuteOnChangeFretSameString)
        playingStringMemory.remember(stringId, streamId);

        return streamId;
    }

    public int play(Note note, int stringId, float pitchBend)
    {
        if (note == null || note.getPitch() == -1)
        return 0;

        return play(note.getPitch(), stringId, pitchBend);
    }

    public int getStringCount()
    {
        return stringCount;
    }

    public bool isAutoLoop()
    {
        return isAutoLoop;
    }

    public void stop(int streamId)
    {
        soundPool.stop(streamId);
    }

    public Drone getDrone()
    {
        return drone;
    }

    public void stopDrone()
    {
        if (drone != null)
        drone.stop(soundPool);
    }

    public bool isPitchBend()
    {
        return isPitchBend;
    }

    public void setStreamPitch(int streamId, Note note, float pitchBend)
    {
        if (streamId > 0)
        {
            int desiredPitch = note.getPitch();

            while (desiredPitch < 0)
            desiredPitch += 12;

            while (desiredPitch > 128)
            desiredPitch -= 12;

            if (desiredPitch < minPitchToPlay)
            return;

            Sample sample = multiSampleList[desiredPitch].getRandomSample(random);
            float rate = sample.getPitchMultiplicator(desiredPitch, pitchBend);

            soundPool.setRate(streamId, rate);
        }
    }

    public SoundPool getSoundPool()
    {
        return soundPool;
    }

    public bool isAutoLoopKeepNoteUntilNewNote()
    {
        return isAutoLoopKeepNoteUntilNewNote;
    }

    public void stopAllNotes(PointerMemory pointerMemory)
    {
        for (int streamIdToMute : pointerMemory.getStreamList())
        {
            stop(streamIdToMute);
        }
        pointerMemory.clearStreamList();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

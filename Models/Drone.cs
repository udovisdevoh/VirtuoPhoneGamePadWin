using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;




/**
* @author Guillaume Lacasse
* (for sitar or bagpipe etc)
*/
public class Drone
{
    private bool isLazyHarmonic = false;

    private bool isMinimizePitchShift = false;

    private int noteType;

    private int resourceId;

    private int currentChord = -1;

    private int streamId = -1;

    private Sample sample;

    private float rate = 1f;

    private float targetRate = 1f;

    private float pitchAdjustSpeedMultiplicator;

    private float volumeAdjustSpeedMultiplicator;

    private float volume;

    /**
    * @param noteType note type (semitones)
    * @param resourceId resource id
    * @param volume volume
    * @param pitchAdjustSpeedMultiplicator 0: instant
    */
    public Drone(int noteType, int resourceId, float volume, float pitchAdjustSpeedMultiplicator, float volumeAdjustSpeedMultiplicator)
    {
        this.noteType = noteType;
        this.resourceId = resourceId;
        Note note = new Note(this.noteType % 12, 0);
        sample = new Sample(note, this.resourceId, volume);
        this.pitchAdjustSpeedMultiplicator = pitchAdjustSpeedMultiplicator;
        this.volumeAdjustSpeedMultiplicator = volumeAdjustSpeedMultiplicator;
    }

    public void onPlayNoteUpdate(SoundPool soundPool, GuitarPreset guitarPreset, Point coordinates)
    {
        int droneFundamentalNoteType = currentChord;

        int chordFundamentalNoteType = guitarPreset.getChordFundamentalNoteTypeAt(coordinates);

        if (isMinimizePitchShift())
        {
            chordFundamentalNoteType = getClosestHarmonizedPitchToOriginalSample(chordFundamentalNoteType);
        }

        bool isDroneNeedToChangeForChord;

        setVolume(soundPool, sample.getVolume());

        if (isLazyHarmonic())
        {
            int interval = Math.Abs((droneFundamentalNoteType % 12) - (chordFundamentalNoteType % 12));
            isDroneNeedToChangeForChord = (interval != 5 && interval != 7 && interval != 0);
        }
        else
        {
            isDroneNeedToChangeForChord = droneFundamentalNoteType != chordFundamentalNoteType;
        }

        if (!isPlaying() || isDroneNeedToChangeForChord)
        {

            setCurrentChord(chordFundamentalNoteType);

            if (isPlaying())
            setPitch(soundPool, chordFundamentalNoteType);
            else
            play(soundPool, chordFundamentalNoteType);
        }
    }

    private void setVolume(SoundPool soundPool, float volumeToSet)
    {
        volume = volumeToSet;
        if (streamId > 0)
        soundPool.setVolume(streamId, volume, volume);
    }

    public void onTickUpdate(SoundPool soundPool)
    {
        adjustPicth(soundPool);
        autoAdjustVolumeDecreaseWithTime(soundPool);
    }

    private void autoAdjustVolumeDecreaseWithTime(SoundPool soundPool)
    {
        if (volume > 0 && volumeAdjustSpeedMultiplicator > 0f)
        {
            volume /= volumeAdjustSpeedMultiplicator;

            if (streamId > 0)
            soundPool.setVolume(streamId, volume, volume);
        }
    }

    private void adjustPicth(SoundPool soundPool)
    {
        if (rate < targetRate && pitchAdjustSpeedMultiplicator > 0f)
        {
            rate *= pitchAdjustSpeedMultiplicator;

            if (rate > targetRate)
            rate = targetRate;

            if (streamId > 0)
            soundPool.setRate(streamId, rate);

        }
        else if (rate > targetRate && pitchAdjustSpeedMultiplicator > 0f)
        {
            rate /= pitchAdjustSpeedMultiplicator;

            if (rate < targetRate)
            rate = targetRate;

            if (streamId > 0)
            soundPool.setRate(streamId, rate);
        }
    }

    public Sample getSample()
    {
        return sample;
    }

    public int GetCurrentChord()
    {
        return currentChord;
    }

    public void setCurrentChord(int currentChord)
    {
        this.currentChord = currentChord;
    }

    public int GetStreamId()
    {
        return streamId;
    }

    public void setStreamId(int streamId)
    {
        this.streamId = streamId;
    }

    public int GetNoteType()
    {
        return noteType;
    }

    public int GetClosestHarmonizedPitchToOriginalSample(int chordFundamentalNoteType)
    {
        int originalSampleFundamental = sample.getOriginalPitch() % 12;

        int bestHarmonizedFundamental = originalSampleFundamental;

        int upperFifth = chordFundamentalNoteType + 7;
        int lowerFifth = chordFundamentalNoteType - 7;
        int upperFourth = chordFundamentalNoteType + 5;
        int lowerFourth = chordFundamentalNoteType - 5;


        int fundamentalDifference = getPitchAbsoluteDifferenceFromNoteToNote(chordFundamentalNoteType, bestHarmonizedFundamental);


        if (getPitchAbsoluteDifferenceFromNoteToNote(chordFundamentalNoteType, upperFifth) < fundamentalDifference)
        bestHarmonizedFundamental = upperFifth;

        if (getPitchAbsoluteDifferenceFromNoteToNote(chordFundamentalNoteType, lowerFifth) < fundamentalDifference)
        bestHarmonizedFundamental = lowerFifth;

        if (getPitchAbsoluteDifferenceFromNoteToNote(chordFundamentalNoteType, upperFourth) < fundamentalDifference)
        bestHarmonizedFundamental = upperFourth;

        if (getPitchAbsoluteDifferenceFromNoteToNote(chordFundamentalNoteType, lowerFourth) < fundamentalDifference)
        bestHarmonizedFundamental = lowerFourth;

        return bestHarmonizedFundamental;
    }

    private int getPitchAbsoluteDifferenceFromNoteToNote(int fromNote, int toNote)
    {
        int difference = Math.Abs(fromNote - toNote);

        while (difference > 6)
        difference -= 12;
        while (difference < -6)
        difference += 12;

        return difference;
    }

    public void stop(SoundPool soundPool)
    {
        if (streamId > 0)
        soundPool.stop(streamId);

        streamId = -1;
        rate = 1f;
    }

    public void setPitch(SoundPool soundPool, int desiredPitch)
    {
        targetRate = getSample().getPitchMultiplicator(desiredPitch, 0f);
        if (pitchAdjustSpeedMultiplicator <= 0f)
        {
            rate = targetRate;
            if (streamId > 0)
            {
                soundPool.setRate(streamId, rate);
            }
        }
    }

    public void play(SoundPool soundPool, int desiredPitch)
    {
        desiredPitch %= 12;

        int originalPitch = noteType;

        while (desiredPitch - originalPitch > 6)
        desiredPitch -= 12;

        while (desiredPitch - originalPitch < -6)
        desiredPitch += 12;

        rate = sample.getPitchMultiplicator(desiredPitch, 0f);
        targetRate = rate;
        streamId = soundPool.play(sample.getSoundId(), sample.getVolume(), sample.getVolume(), 1, -1, rate);
        volume = sample.getVolume();
    }

    public bool isLazyHarmonic()
    {
        return isLazyHarmonic;
    }

    public bool isMinimizePitchShift()
    {
        return isMinimizePitchShift;
    }

    public bool isPlaying()
    {
        return streamId > 0;
    }

    public void isLazyHarmonic(bool isLazyHarmonic)
    {
        this.isLazyHarmonic = isLazyHarmonic;
    }

    public void isMinimizePitchShift(bool isMinimizePitchShift)
    {
        this.isMinimizePitchShift = isMinimizePitchShift;
    }
}

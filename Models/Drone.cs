using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

#warning Cleanup en cours, rendu à ce fichier, faire le cleanup pour le reste aussi

public class Drone
{
    private bool _isLazyHarmonic = false;

    private bool _isMinimizePitchShift = false;

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

    public void OnPlayNoteUpdate(SoundPool soundPool, GuitarPreset guitarPreset, Point coordinates)
    {
        int droneFundamentalNoteType = currentChord;

        int chordFundamentalNoteType = guitarPreset.GetChordFundamentalNoteTypeAt(coordinates);

        if (isMinimizePitchShift())
        {
            chordFundamentalNoteType = GetClosestHarmonizedPitchToOriginalSample(chordFundamentalNoteType);
        }

        bool isDroneNeedToChangeForChord;

        SetVolume(soundPool, sample.GetVolume());

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

            SetCurrentChord(chordFundamentalNoteType);

            if (isPlaying())
            setPitch(soundPool, chordFundamentalNoteType);
            else
            play(soundPool, chordFundamentalNoteType);
        }
    }

    private void SetVolume(SoundPool soundPool, float volumeToSet)
    {
        volume = volumeToSet;
        if (streamId > 0)
        soundPool.setVolume(streamId, volume, volume);
    }

    public void OnTickUpdate(SoundPool soundPool)
    {
        AdjustPicth(soundPool);
        AutoAdjustVolumeDecreaseWithTime(soundPool);
    }

    private void AutoAdjustVolumeDecreaseWithTime(SoundPool soundPool)
    {
        if (volume > 0 && volumeAdjustSpeedMultiplicator > 0f)
        {
            volume /= volumeAdjustSpeedMultiplicator;

            if (streamId > 0)
            soundPool.setVolume(streamId, volume, volume);
        }
    }

    private void AdjustPicth(SoundPool soundPool)
    {
        if (rate < targetRate && pitchAdjustSpeedMultiplicator > 0f)
        {
            rate *= pitchAdjustSpeedMultiplicator;

            if (rate > targetRate)
            rate = targetRate;

            if (streamId > 0)
            soundPool.SetRate(streamId, rate);

        }
        else if (rate > targetRate && pitchAdjustSpeedMultiplicator > 0f)
        {
            rate /= pitchAdjustSpeedMultiplicator;

            if (rate < targetRate)
            rate = targetRate;

            if (streamId > 0)
            soundPool.SetRate(streamId, rate);
        }
    }

    public Sample GetSample()
    {
        return sample;
    }

    public int GetCurrentChord()
    {
        return currentChord;
    }

    public void SetCurrentChord(int currentChord)
    {
        this.currentChord = currentChord;
    }

    public int GetStreamId()
    {
        return streamId;
    }

    public void SetStreamId(int streamId)
    {
        this.streamId = streamId;
    }

    public int GetNoteType()
    {
        return noteType;
    }

    public int GetClosestHarmonizedPitchToOriginalSample(int chordFundamentalNoteType)
    {
        int originalSampleFundamental = sample.GetOriginalPitch() % 12;

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

    public void Stop(SoundPool soundPool)
    {
        if (streamId > 0)
        soundPool.Stop(streamId);

        streamId = -1;
        rate = 1f;
    }

    public void setPitch(SoundPool soundPool, int desiredPitch)
    {
        targetRate = GetSample().GetPitchMultiplicator(desiredPitch, 0f);
        if (pitchAdjustSpeedMultiplicator <= 0f)
        {
            rate = targetRate;
            if (streamId > 0)
            {
                soundPool.SetRate(streamId, rate);
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

        rate = sample.GetPitchMultiplicator(desiredPitch, 0f);
        targetRate = rate;
        streamId = soundPool.Play(sample.GetSoundId(), sample.GetVolume(), sample.GetVolume(), 1, -1, rate);
        volume = sample.GetVolume();
    }

    public bool isLazyHarmonic()
    {
        return _isLazyHarmonic;
    }

    public bool isMinimizePitchShift()
    {
        return _isMinimizePitchShift;
    }

    public bool isPlaying()
    {
        return streamId > 0;
    }

    public void IsLazyHarmonic(bool isLazyHarmonic)
    {
        this._isLazyHarmonic = isLazyHarmonic;
    }

    public void IsMinimizePitchShift(bool isMinimizePitchShift)
    {
        this._isMinimizePitchShift = isMinimizePitchShift;
    }
}

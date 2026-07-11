using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Drone
{
    private bool isLazyHarmonic = false;

    private bool isMinimizePitchShift = false;

    private bool isFixed = false;

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

    public Drone(int noteType, int resourceId, float volume, float pitchAdjustSpeedMultiplicator, float volumeAdjustSpeedMultiplicator)
    {
        this.noteType = noteType;
        this.resourceId = resourceId;
        Note note = new Note(this.noteType % 12, 0);
        sample = new Sample(note, this.resourceId, volume);
        this.pitchAdjustSpeedMultiplicator = pitchAdjustSpeedMultiplicator;
        this.volumeAdjustSpeedMultiplicator = volumeAdjustSpeedMultiplicator;
    }

    public void OnPlayNoteUpdate(ISoundPool soundPool, GuitarPreset guitarPreset, Point coordinates)
    {
        OnPlayNoteUpdate(soundPool, guitarPreset.GetChordFundamentalNoteTypeAt(coordinates));
    }

    /// <summary>Update the drone to follow a chord root (note type 0-11): start it, or glide/retune it.</summary>
    public void OnPlayNoteUpdate(ISoundPool soundPool, int chordFundamentalNoteType)
    {
        if (isFixed) chordFundamentalNoteType = noteType;   // fixed drone: hold our own note, ignore chords

        int droneFundamentalNoteType = currentChord;

        if (IsMinimizePitchShift())
        {
            chordFundamentalNoteType = GetClosestHarmonizedPitchToOriginalSample(chordFundamentalNoteType);
        }

        bool isDroneNeedToChangeForChord;

        SetVolume(soundPool, sample.GetVolume());

        if (IsLazyHarmonic())
        {
            int interval = Math.Abs((droneFundamentalNoteType % 12) - (chordFundamentalNoteType % 12));
            isDroneNeedToChangeForChord = (interval != 5 && interval != 7 && interval != 0);
        }
        else
        {
            isDroneNeedToChangeForChord = droneFundamentalNoteType != chordFundamentalNoteType;
        }

        if (!IsPlaying() || isDroneNeedToChangeForChord)
        {

            SetCurrentChord(chordFundamentalNoteType);

            if (IsPlaying())
            {
                SetPitch(soundPool, chordFundamentalNoteType);
            }
            else
            {
                Play(soundPool, chordFundamentalNoteType);
            }
        }
    }

    private void SetVolume(ISoundPool soundPool, float volumeToSet)
    {
        volume = volumeToSet;
        if (streamId > 0)
        {
            soundPool.SetVolume(streamId, volume, volume);
        }
    }

    public void OnTickUpdate(ISoundPool soundPool)
    {
        // Pitch is now glided sample-accurately by the voice itself (see SetPitch); only the volume decays here.
        AutoAdjustVolumeDecreaseWithTime(soundPool);
    }

    private void AutoAdjustVolumeDecreaseWithTime(ISoundPool soundPool)
    {
        if (volume > 0 && volumeAdjustSpeedMultiplicator > 0f)
        {
            volume /= volumeAdjustSpeedMultiplicator;

            if (streamId > 0)
            {
                soundPool.SetVolume(streamId, volume, volume);
            }
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


        int fundamentalDifference = GetPitchAbsoluteDifferenceFromNoteToNote(chordFundamentalNoteType, bestHarmonizedFundamental);


        if (GetPitchAbsoluteDifferenceFromNoteToNote(chordFundamentalNoteType, upperFifth) < fundamentalDifference)
        bestHarmonizedFundamental = upperFifth;

        if (GetPitchAbsoluteDifferenceFromNoteToNote(chordFundamentalNoteType, lowerFifth) < fundamentalDifference)
        bestHarmonizedFundamental = lowerFifth;

        if (GetPitchAbsoluteDifferenceFromNoteToNote(chordFundamentalNoteType, upperFourth) < fundamentalDifference)
        bestHarmonizedFundamental = upperFourth;

        if (GetPitchAbsoluteDifferenceFromNoteToNote(chordFundamentalNoteType, lowerFourth) < fundamentalDifference)
        bestHarmonizedFundamental = lowerFourth;

        return bestHarmonizedFundamental;
    }

    private int GetPitchAbsoluteDifferenceFromNoteToNote(int fromNote, int toNote)
    {
        int difference = Math.Abs(fromNote - toNote);

        while (difference > 6)
        {
            difference -= 12;
        }

        while (difference < -6)
        {
            difference += 12;
        }

        return difference;
    }

    public void Stop(ISoundPool soundPool)
    {
        if (streamId > 0)
        soundPool.Stop(streamId);

        streamId = -1;
        rate = 1f;
    }

    public void SetPitch(ISoundPool soundPool, int desiredPitch)
    {
        float previousRate = rate;
        targetRate = GetSample().GetPitchMultiplicator(desiredPitch, 0f);
        rate = targetRate;   // logical current rate; the voice glides to it below

        if (streamId <= 0) return;

        if (pitchAdjustSpeedMultiplicator <= 0f || previousRate <= 0f)
        {
            soundPool.SetRate(streamId, targetRate);   // snap
            return;
        }

        // Smooth, sample-accurate portamento to the new root. (The old approach stepped the rate once per
        // ~60 Hz tick — at a 1.1 multiplier that jumped ~1.6 semitones per tick and sounded stepped.) Keep the
        // SAME overall glide the per-tick multiplier used to give: numTicks = |ln(ratio)| / |ln(multiplier)|.
        const float tickSeconds = 1f / 60f;
        double lnStep = Math.Abs(Math.Log(pitchAdjustSpeedMultiplicator));
        double lnRatio = Math.Abs(Math.Log(targetRate / previousRate));
        float glideSeconds = lnStep > 0 ? (float)(lnRatio / lnStep) * tickSeconds : 0f;
        soundPool.GlideToRate(streamId, targetRate, glideSeconds);
    }

    public void Play(ISoundPool soundPool, int desiredPitch)
    {
        desiredPitch %= 12;

        int originalPitch = noteType;

        while (desiredPitch - originalPitch > 6)
        {
            desiredPitch -= 12;
        }

        while (desiredPitch - originalPitch < -6)
        {
            desiredPitch += 12;
        }

        rate = sample.GetPitchMultiplicator(desiredPitch, 0f);
        targetRate = rate;
        streamId = soundPool.Play(sample.GetSoundId(), sample.GetVolume(), sample.GetVolume(), 1, -1, rate);
        volume = sample.GetVolume();
    }

    public bool IsLazyHarmonic()
    {
        return isLazyHarmonic;
    }

    public bool IsMinimizePitchShift()
    {
        return isMinimizePitchShift;
    }

    public bool IsPlaying()
    {
        return streamId > 0;
    }

    public void IsLazyHarmonic(bool isLazyHarmonic)
    {
        this.isLazyHarmonic = isLazyHarmonic;
    }

    public void IsFixed(bool isFixed)
    {
        this.isFixed = isFixed;
    }

    public bool IsFixed()
    {
        return isFixed;
    }

    public void IsMinimizePitchShift(bool isMinimizePitchShift)
    {
        this.isMinimizePitchShift = isMinimizePitchShift;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Note
{
    private int pitch;

    public const int C = 0;

    public const int CSharp = 1;

    public const int DFlat = 1;

    public const int D = 2;

    public const int DSharp = 3;

    public const int EFlat = 3;

    public const int E = 4;

    public const int F = 5;

    public const int FSharp = 6;

    public const int GFlat = 6;

    public const int G = 7;

    public const int GSharp = 8;

    public const int AFlat = 8;

    public const int A = 9;

    public const int ASharp = 10;

    public const int BFlat = 10;

    public const int B = 11;

    /**
    * @param noteType Use constant defined in Note class
    * @param octave octave number
    */
    public Note(int noteType, int octave)
    {
        noteType = noteType % 12;
        pitch = noteType + octave * 12;
    }

    public string GetName()
    {
        int noteType = pitch % 12;
        switch (noteType)
        {
            case 0:
            return "C";
            case 1:
            return "C#";
            case 2:
            return "D";
            case 3:
            return "D#";
            case 4:
            return "E";
            case 5:
            return "F";
            case 6:
            return "F#";
            case 7:
            return "G";
            case 8:
            return "G#";
            case 9:
            return "A";
            case 10:
            return "A#";
            case 11:
            return "B";
            default:
            return "";
        }
    }

    /**
    * @param pitch absolute pitch (semitones)
    */
    public Note(int pitch)
    {
        this.pitch = pitch;
    }

    /**
    * @return Pitch (in semitones)
    */
    public int GetPitch()
    {
        return pitch;
    }

    public void SetPitch(int pitch)
    {
        this.pitch = pitch;
    }

    // Java-style wrappers kept for converted code
    public int getPitch() => GetPitch();
    public void setPitch(int p) => SetPitch(p);
    public string getName() => GetName();
}

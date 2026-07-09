using System;
using System.Collections.Generic;

#warning Todo remove android stub and replace all of this will real code

// Minimal stubs for Android/Audio classes referenced by converted code.

public static class AudioManager
{
    public const int STREAM_MUSIC = 3;
}

public class SoundPool
{
    private int nextId = 1;
    public SoundPool(int polyphony, int streamType, int srcQuality) { }

    public int Load(int resourceId, int priority) => nextId++;
    public int Play(int soundId, float leftVolume, float rightVolume, int priority, int loop, float rate) => nextId++;
    public void Stop(int streamId) { }
    public void SetVolume(int streamId, float left, float right) { }
    public void SetRate(int streamId, float rate) { }
    public void Release() { }
}

public class PointerMemory
{
    private List<int> streams = new List<int>();
    public IEnumerable<int> GetStreamList() => streams;
    public void ClearStreamList() => streams.Clear();
}

public class AppController
{
    public const int STRING_COUNT = 6;
    private static AppController instance = new AppController();
    public static AppController getAppController() => instance;
    public VirtuoPhone.Models.Instrument getInstrument() => new VirtuoPhone.Models.Instruments.DummyInstrument();
}

namespace VirtuoPhone.Models.Instruments
{
    // Minimal concrete Instrument used as a safe fallback by converted code.
    public class DummyInstrument : Instrument
    {
        protected override bool BuildIsAutoLoopKeepNoteUntilNewNote() => false;

        protected override bool BuildIsDroneMinimizePitchShift() => false;

        protected override bool BuildIsLazyHarmonicDrone() => false;

        protected override bool BuildIsPitchBend() => false;

        protected override void LoadDrone() { }

        protected override bool BuildIsAutoLoop() => false;

        protected override bool BuildIsMuteOnChangeFretSameString() => false;

        protected override int BuildStringCount() => 6;

        protected override int BuildMinPitchToPlay() => 0;

        protected override void LoadSamples() { }
    }
}

public static class R
{
    public static class Raw
    {
        // Resource stubs referenced across instruments (values arbitrary)
        public const int bagpipese0 = 1;
        public const int bagpipesa0 = 2;
        public const int bagpipesd1 = 3;
        public const int bagpipesg1 = 4;
        public const int bagpipesb1 = 5;
        public const int bagpipese2 = 6;
        public const int bagpipesa2 = 7;
        public const int bagpipesdroneloopa = 8;

        public const int guimb1 = 20;
        public const int guimb2 = 21;
        public const int guimb3 = 22;
        public const int guimb5 = 23;
        public const int guimb6 = 24;
        public const int guimb7 = 25;
        public const int guimb8 = 26;
        public const int guimb9 = 27;
        public const int guimb10 = 28;
        public const int guimb11 = 29;
        public const int guimb12 = 30;
        public const int guimb14 = 31;
        public const int guimb15 = 32;
        public const int guimb17 = 33;
        public const int guimb20 = 34;
        public const int guimb23 = 35;
        public const int guimb25 = 36;
        public const int guimb26 = 37;
        public const int guimb30 = 38;
        public const int guimb31 = 39;
        public const int guimb32 = 40;
        public const int guimb34 = 41;
        public const int guimb35 = 42;
        public const int guimb36 = 43;

        public const int guitare2 = 60;
        public const int guitara2 = 61;
        public const int guitard3 = 62;
        public const int guitarg3 = 63;
        public const int guitarb3 = 64;
        public const int guitare4 = 65;

        public const int harpe0 = 80;
        public const int harpa0 = 81;
        public const int harpd1 = 82;
        public const int harpg1 = 83;
        public const int harpb1 = 84;
        public const int harpe2 = 85;
        public const int harpa2 = 86;
        public const int harpd3 = 87;
        public const int harpg3 = 88;
        public const int harpb3 = 89;
        public const int harpe4 = 90;

        public const int harpsichordf0 = 100;
        public const int harpsichorda0 = 101;
        public const int harpsichordd1 = 102;
        public const int harpsichordg1 = 103;
        public const int harpsichordb1 = 104;
        public const int harpsichorde2 = 105;
        public const int harpsichorda2 = 106;
        public const int harpsichordd3 = 107;
        public const int harpsichordg3 = 108;
        public const int harpsichordb3 = 109;
        public const int harpsichorde4 = 110;

        public const int pianoe0 = 120;
        public const int pianoa0 = 121;
        public const int pianod1 = 122;
        public const int pianog1 = 123;
        public const int pianob1 = 124;
        public const int pianoe2 = 125;
        public const int pianoa2 = 126;
        public const int pianod3 = 127;
        public const int pianog3 = 128;
        public const int pianob3 = 129;
        public const int pianoe4 = 130;

        public const int sitaree2 = 140;
        public const int sitarea2 = 141;
        public const int sitared3 = 142;
        public const int sitareg3 = 143;
        public const int sitareb3 = 144;
        public const int sitaree4 = 145;
        public const int sitarea4 = 146;
        public const int sitared5 = 147;
        public const int sitareg5 = 148;
        public const int sitareb5 = 149;
        public const int sitarecsharp6 = 150;
        public const int sitaree6 = 151;
        public const int sitardroneshortc = 152;
        public const int tampuracsharpshort = 153;

        public const int synthe0 = 160;
        public const int syntha0 = 161;
        public const int synthd1 = 162;
        public const int synthg1 = 163;
        public const int synthb1 = 164;
        public const int synthe2 = 165;
        public const int syntha2 = 166;
        public const int synthd3 = 167;
        public const int synthg3 = 168;
        public const int synthb3 = 169;
        public const int synthe4 = 170;

        public const int violine0 = 180;
        public const int violina0 = 181;
        public const int violind1 = 182;
        public const int violing1 = 183;
        public const int violinb1 = 184;
        public const int violine2 = 185;
        public const int violina2 = 186;
        public const int violind3 = 187;
        public const int violing3 = 188;
        public const int violinb3 = 189;
        public const int violine4 = 190;
    }
}

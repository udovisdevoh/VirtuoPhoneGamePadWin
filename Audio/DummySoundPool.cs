using System;
using System.Collections.Generic;

namespace VirtuoPhone;

// Silent, headless ISoundPool backend (no audio device). The real engine is NAudioSoundPool;
// pick one via AudioBackend. Useful for tests and for running without an audio output.
public class DummySoundPool : ISoundPool
{
    private int nextId = 1;
    public DummySoundPool(int polyphony) { }
    public int Load(int resourceId, int priority) => nextId++;
    public int Play(int soundId, float leftVolume, float rightVolume, int priority, int loop, float rate, float attackSeconds, float releaseSeconds) => nextId++;
    public void Stop(int streamId) { }
    public void SetVolume(int streamId, float left, float right) { }
    public void SetRate(int streamId, float rate) { }
    public void GlideRate(int streamId, float rateFactor, float glideSeconds) { }
    public void GlideToRate(int streamId, float targetRate, float glideSeconds) { }
    public void Release() { }
}
using System;
using System.Collections.Generic;

#warning Todo remove dummy and replace with real code

namespace VirtuoPhone;

public class DummySoundPool // Must be replaced with real soundpool.
{
    private int nextId = 1;
    public DummySoundPool(int polyphony) { }
    public int Load(int resourceId, int priority) => nextId++;
    public int Play(int soundId, float leftVolume, float rightVolume, int priority, int loop, float rate) => nextId++;
    public void Stop(int streamId) { }
    public void SetVolume(int streamId, float left, float right) { }
    public void SetRate(int streamId, float rate) { }
    public void Release() { }
}
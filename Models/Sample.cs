package com.virtuophone.models;

/**
 * @author Guillaume
 */
public class Sample {
	private int originalPitch;
	
	private int resourceId;
	
	private int soundId;
	
	private float volume;
	
	/**
	 * @param note note
	 * @param resourceId resource's id
	 */
	public Sample(Note note, int resourceId, float volume) {
		originalPitch = note.getPitch();
		this.resourceId = resourceId;
		this.volume = volume;
	}

	public int getOriginalPitch() {
		return originalPitch;
	}
	
	public float getPitchMultiplicator(int desiredPitch, float pitchBend) {
		if (desiredPitch == originalPitch && pitchBend == 0f)
			return 1.0f;
		else
			return (float)Math.pow(1.0594632, (double)(desiredPitch - originalPitch) + pitchBend);
	}

	public int getResourceId() {
		return resourceId;
	}

	public void setSoundId(int soundId) {
		this.soundId = soundId;
	}
	
	public int getSoundId() {
		return soundId;
	}
	
	public float getVolume() {
		return volume;
	}
}

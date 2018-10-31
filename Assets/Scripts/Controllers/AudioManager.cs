using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	public AudioSource efxSource;                   //Drag a reference to the audio source which will play the sound effects.    
	public float DefaultLowPitchRange = .95f;              //The lowest a sound effect will be randomly pitched.
	public float DefaultHighPitchRange = 1.05f;            //The highest a sound effect will be randomly pitched.
	private float DefaultVolume;
	private float DefaultMin;
	private float DefaultMax;
	public List<SoundItem> SoundLibrary = new List<SoundItem>();

	void Awake () {
		if (efxSource == null)
			efxSource = GetComponent<AudioSource> ();
	}

	void Start () {
		DefaultVolume = efxSource.volume;
		DefaultMin = efxSource.minDistance;
		DefaultMax = efxSource.maxDistance;
	}

	public void ChangeSettings (float Volume, float MinDistance, float MaxDistance, float LowPitch, float HighPitch, bool Loop) {
		efxSource.volume = Volume;
		efxSource.minDistance = MinDistance;
		efxSource.maxDistance = MaxDistance;
		efxSource.loop = Loop;

		float randomPitch = Random.Range(LowPitch, HighPitch);
		efxSource.pitch = randomPitch;
	}

	//Used to play single sound clips.
	public void PlaySound (string Name) {
		ChangeSettings (DefaultVolume, DefaultMin, DefaultMax, DefaultLowPitchRange, DefaultHighPitchRange, false);
		AudioClip NewSound = SoundLibrary.Find (X => X.name == Name).sound;

		float randomPitch = Random.Range(DefaultLowPitchRange, DefaultHighPitchRange);
		efxSource.pitch = randomPitch;

		if (NewSound != null) {
			efxSource.clip = NewSound;
			efxSource.Play ();
		} else
			Debug.LogError ("Sound Not Found");
	}

	//Used to play single sound clips.
	public void PlaySound (string Name, float Volume, float MinDistance, float MaxDistance, float LowPitch, float HighPitch, bool Loop) {
		ChangeSettings (Volume, MinDistance, MaxDistance, LowPitch, HighPitch, Loop);

		AudioClip NewSound = SoundLibrary.Find (X => X.name == Name).sound;
		if (NewSound != null) {
			efxSource.clip = NewSound;
			efxSource.Play ();
		} else
			Debug.LogError ("Sound Not Found");
	}

	public void PlaySoundNormal (string Name) {
		ChangeSettings (DefaultVolume, DefaultMin, DefaultMax, DefaultLowPitchRange, DefaultHighPitchRange, false);
		AudioClip NewSound = SoundLibrary.Find (X => X.name == Name).sound;
		if (NewSound != null) {
			efxSource.clip = NewSound;
			efxSource.Play ();
		} else
			Debug.LogError ("Sound Not Found");
	}
}

[System.Serializable]
public class SoundItem {
	public string name;
	public AudioClip sound;

	public SoundItem (string Name, AudioClip Sound) {
		name = Name;
		sound = Sound;
	}
}

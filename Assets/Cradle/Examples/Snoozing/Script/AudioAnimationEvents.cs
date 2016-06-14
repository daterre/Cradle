using UnityEngine;
using System.Collections;

public class AudioAnimationEvents : MonoBehaviour {

	public AudioSource AudioSource;

	public void AudioPlay(AudioClip clip)
	{
		AudioSource.Stop();
		AudioSource.loop = false;
		AudioSource.clip = clip;
		AudioSource.Play();
	}

	public void AudioPlayLoop(AudioClip clip)
	{
		if (AudioSource.isPlaying && AudioSource.loop && AudioSource.clip == clip)
			return;

		AudioSource.loop = true;
		AudioSource.clip = clip;
		AudioSource.Play();
	}
}

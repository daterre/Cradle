using UnityEngine;
using System.Collections;

public class SnoozingHooks : MonoBehaviour {

	public SnoozingStory Story { get; private set; }

	public AudioSource sfxAlarm;

	IEnumerator alarm_Enter()
	{
		yield return new WaitForSeconds(1f);
		sfxAlarm.time = 0f;
		sfxAlarm.Play();

		//Debug.Log("Fade in text");
		//Debug.Log("Fade in phone");

		////ShowText();
		////ShowImage(uiPhone);

		//yield return new WaitForSeconds(4f);
		////ShowLinks();
		//Debug.Log("Show links");
	}

	void alarm_Exit()
	{
		sfxAlarm.Stop();
	}

}

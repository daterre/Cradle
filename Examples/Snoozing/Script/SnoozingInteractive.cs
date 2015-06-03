using UnityEngine;
using System.Collections;

public class SnoozingInteractive : MonoBehaviour {

	public AudioSource sfxAlarm;
	public Sprite her_sprite;
	public Sprite sea_sprite;
	public Sprite sidewalk_sprite;
	public Sprite powerpoint_sprite;

	SnoozingStory story;

	void Awake()
	{
		this.story = GetComponent<SnoozingStory>();
	}

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

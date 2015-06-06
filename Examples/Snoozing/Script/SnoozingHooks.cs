using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityTwine;

public class SnoozingHooks : MonoBehaviour {

	public TwineTextPlayer uiTextPlayer;
	public Image uiImage;
	
	SnoozingStory story;

	void Awake()
	{
		this.story = GetComponent<SnoozingStory>();
	}

	// ...........................
	// alarm

	public AudioSource alarm_sfx;
	public Sprite alarm_image;

	IEnumerator alarm_Enter()
	{
		yield return new WaitForSeconds(1f);
		alarm_sfx.time = 0f;
		alarm_sfx.Play();

		yield return new WaitForSeconds(3f);
		uiImage.sprite = alarm_image;
		FadeImage(0f,1f, story.again ? 1f : 8f);

	}

	void alarm_Exit()
	{
		alarm_sfx.Stop();
		FadeImage(1f, 0f, 0f);
		
	}

	// ...........................
	// snooze

	void snooze_Exit()
	{
		
	}

	// ...........................
	// her

	public Sprite her_image;

	void her_Enter()
	{
		uiImage.sprite = her_image;
		FadeImage(0f, 1f, 12f);
	}

	void her_Exit()
	{
		FadeImage(1f, 0f, 0.1f);
	}

	// ...........................
	// her

	public Sprite sea_image;
	int sea_current = 0;

	IEnumerator sea_Enter()
	{
		uiImage.sprite = sea_image;
		FadeImage(0f, 1f, 5f);
		uiTextPlayer.Auto = false;

		// End frame so that output finishes
		yield return null;

		for (int i = 0; i < story.Output.Count; i++)
		{
			var text = story.Output[i] as TwineText;
			if (text == null)
				continue;
			
			bool waitForClick = text.String.Trim().Length > 0;

			// If it's a line with actual text, wait for a click to display it
			if (waitForClick) {
				while (!Input.GetMouseButtonDown(0))
					yield return null;
			}

			uiTextPlayer.DisplayOutput(text);
			
			// End this frame so mouse button won't still be down
			if (waitForClick)
				yield return null;
		}

		// Wait for one more click
		while (!Input.GetMouseButtonDown(0))
			yield return null;

		// Advance
		story.Advance("continue");
	}

	void sea_Exit()
	{
		uiTextPlayer.Auto = true;
		FadeImage(1f, 0f, 0.1f);
	}

	// ...........................
	// work

	public Image work_ppCursor;
	public Image work_ppTemplate;
	public Sprite[] work_ppImages;
	List<GameObject> work_objects = new List<GameObject>();
	int work_maxObjects;

	void work_Enter()
	{
		work_ppCursor.gameObject.SetActive(true);
		Screen.showCursor = false;
		work_objects.Clear();
		work_maxObjects = Random.Range(10, 30);
	}

	void work_Update()
	{
		work_ppCursor.transform.position = Input.mousePosition;
		if (Input.GetMouseButtonDown(0))
		{
			if (work_objects.Count == work_maxObjects)
			{
				for (int i = 0; i < work_objects.Count; i++)
					GameObject.Destroy(work_objects[i]);

				story.Advance("powerpoint");
			}
			else
			{
				var obj = (GameObject)Instantiate(work_ppTemplate.gameObject);
				obj.SetActive(true);
				work_objects.Add(obj);

				var img = obj.GetComponent<Image>();
				img.rectTransform.SetParent(work_ppTemplate.rectTransform.parent);
				img.sprite = work_ppImages[Random.Range(0, work_ppImages.Length)];
				img.transform.position = Input.mousePosition;
			}
		}
	}

	void work_Exit()
	{
		work_ppCursor.gameObject.SetActive(false);
		Screen.showCursor = true;
	}

	// ======================================
	// Helpers

	Coroutine _fadeCurrent = null;
	
	void FadeImage(float from, float to, float time, float flickerMin = 0f, float flickerMax = 0f)
	{
		if (_fadeCurrent != null)
			StopCoroutine(_fadeCurrent);

		_fadeCurrent = StartCoroutine(FadeImageAnim(from, to, time, flickerMin, flickerMax));

	}

	IEnumerator FadeImageAnim(float from, float to, float time,
		float flickerFreqMin = 0,
		float flickerFreqMax = 0,
		float flickerMin = 0f,
		float flickerMax = 0f
		)
	{
		Color color = uiImage.color;
		float state = (color.a - from) / (to - from);
		//float flickerTarget = -1f;

		for (float t = state*time; t <= time; t += Time.deltaTime)
		{
			color = uiImage.color;
			
			//if (flickerFreqMin > 0f) {
			//	if (flickerTarget < 0f) {
			//		flickerTarget = 
			//	}
			//}

			color.a = Mathf.Lerp(from, to, t/time);
			uiImage.color = color;
			yield return null;
		}

		color = uiImage.color;
		color.a = to;
		uiImage.color = color;
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityTwine;

public class SnoozingHooks : MonoBehaviour {

	public TwineTextPlayer uiTextPlayer;
	public Image uiImage;
	public float alarmWakeUpDelay = 0.3f;
	
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
		SoundAlarm();

		uiImage.sprite = alarm_image;
		FadeImage(uiImage, 0f, 1f, story.again ? 2f : 8f);
	}

	void alarm_Exit()
	{
		alarm_sfx.loop = false;
		FadeImage(uiImage, 1f, 0f, 0f);
	}

	// ...........................
	// snooze

	public AudioSource snooze_sfxNoise;
	public AudioSource snooze_sfxUnderwater;

	IEnumerator snooze_Enter()
	{
		snooze_sfxNoise.volume = 0f;
		snooze_sfxUnderwater.volume = 0f;
		snooze_sfxNoise.Play();
		snooze_sfxUnderwater.Play();

		float t = 0;
		while (true)
		{
			yield return null;
			t += Time.deltaTime;
			const float audioFadeIn = 1f; // 1 second fade in
			float fadeIn = Mathf.Clamp(t / audioFadeIn, 0f, 1f);
			float anxiety = Input.mousePosition.y / Screen.height;
			float dreaming = 1f - anxiety;
			float boost = Input.mousePosition.x / Screen.width;

			snooze_sfxNoise.volume = anxiety * boost * fadeIn;
			snooze_sfxUnderwater.volume = dreaming * boost * fadeIn;

			if (story.State == TwineStoryState.Idle && uiTextPlayer.WasClicked())
			{
				yield return null;
				if (boost < 0.2f)
				{
					story.Advance("her");
					StartCoroutine(SnoozeFadeOut(2f));
				}
				else if (anxiety > dreaming)
				{
					story.Advance("anxiety");
				}
				else
				{
					story.Advance("dream");
				}
				yield break;
			}
		}
	}

	IEnumerator SnoozeFadeOut(float time, float noise = 0f, float underwater = 0f)
	{
		float noiseVol = snooze_sfxNoise.volume;
		float underwaterVol = snooze_sfxUnderwater.volume;
		for (float t = 0f; t <= time; t += Time.deltaTime)
		{
			snooze_sfxNoise.volume = Mathf.Lerp(noiseVol, noise, t / time);
			snooze_sfxUnderwater.volume = Mathf.Lerp(underwaterVol, underwater, t / time);
			yield return null;
		}
		if (noise == 0f)
			snooze_sfxNoise.Stop();
		if (underwater == 0f)
			snooze_sfxUnderwater.Stop();

	}

	// ...........................
	// her

	public Sprite her_image;
	public float her_minScreenMove = 0.01f;
	public float her_alphaFactor = 4f;
	public float her_alphaLineTriggerUp = 0.9f;
	public float her_alphaLineTriggerDown = 0.4f;
	public float her_alphaFadeoutTime = 3f;
	public AudioSource her_sfxBreathing;
	int her_outputIndex = 0;
	bool her_lineTriggered = false;
	bool her_done = false;
	Vector3 her_lastMousePos;

	IEnumerator her_Enter()
	{
		uiImage.sprite = her_image;
		uiImage.color = new Color(1f, 1f, 1f, 0f);
		uiTextPlayer.AutoDisplay = false;
		her_outputIndex = 0;
		her_lineTriggered = false;
		her_done = false;
		her_lastMousePos = Input.mousePosition;

		her_sfxBreathing.time = 0f;
		her_sfxBreathing.volume = 0f;
		her_sfxBreathing.Play();
		const float breathFadeInTime = 2f;
		for (float t = 0; t <= breathFadeInTime; t += Time.deltaTime)
		{
			her_sfxBreathing.volume = t / breathFadeInTime;
			yield return null;
		}
	}

	void her_Update()
	{
		if (her_done)
			return;

		float delta = (Input.mousePosition - her_lastMousePos).magnitude;
		float move = Mathf.Clamp(delta / Screen.height, 0f, 1f);
		Color color = uiImage.color;
		if (move < her_minScreenMove)
		{
			if (_fadeCurrent == null && color.a > 0f)
				FadeImage(uiImage, 1f, 0f, her_alphaFadeoutTime);
		}
		else if (!her_lineTriggered)
		{
			if (_fadeCurrent != null)
			{
				StopCoroutine(_fadeCurrent);
				_fadeCurrent = null;
			}
			
			color.a = Mathf.Clamp(color.a + move * her_alphaFactor, 0f, 1f);
			uiImage.color = color;
		}

		// Show another line
		if (!her_lineTriggered && color.a >= her_alphaLineTriggerUp)
		{
			TwineText line = null;
			while (line == null)
			{
				TwineOutput output = story.Output[her_outputIndex];
				if (output is TwineText)
				{
					line = (TwineText)output;
				}
				else if (output is TwineLink && output.Name == "continue")
				{
					her_done = true;
					StartCoroutine(her_alarm(output as TwineLink));
					break;
				}
				her_outputIndex++;
			}

			if (line != null)
			{
				uiTextPlayer.DisplayOutput(line);
				her_lineTriggered = true;
			}
		}
		else if (her_lineTriggered && color.a <= her_alphaLineTriggerDown)
		{
			her_lineTriggered = false;
		}

		her_lastMousePos = Input.mousePosition;
	}

	IEnumerator her_alarm(TwineLink continueLink)
	{
		SoundAlarm();
		for (float t = 0; t <= alarmWakeUpDelay; t += Time.deltaTime)
		{
			her_sfxBreathing.volume = 1f - (t / alarmWakeUpDelay);
			yield return null;
		}
		her_sfxBreathing.Stop();
		story.Advance(continueLink);
	}

	void her_Exit()
	{
		FadeImage(uiImage, 1f, 0f, 0.1f);
		uiTextPlayer.AutoDisplay = true;
	}

	// ...........................
	// sea

	public Sprite sea_image;
	int sea_current = 0;

	void sea_Enter()
	{
		uiImage.sprite = sea_image;
		FadeImage(uiImage, 0f, 1f, 8f);
		StartCoroutine(SnoozeFadeOut(2f, noise: 0f, underwater: 1f));
	}

	IEnumerator sea_Output(TwineOutput output)
	{
		if (output is TwineLink && output.Name == "continue")
		{
			yield return StartCoroutine(ClickForAlarm(output as TwineLink));
		}
	}

	void sea_Exit()
	{
		FadeImage(uiImage, 1f, 0f, 0f);
	}

	IEnumerator anxiety_Exit()
	{
		return SnoozeFadeOut(alarmWakeUpDelay);
	}

	IEnumerator dreaming_Exit()
	{
		return SnoozeFadeOut(alarmWakeUpDelay);
	}

	// ...........................
	// relationship

	public AudioSource relationship_sfxCough;
	public AudioClip[] relationship_sfxCoughSounds;

	IEnumerator relationship_Output(TwineOutput output)
	{
		if (output is TwineText && Random.Range(0,2) == 0)
		{
			if (!relationship_sfxCough.isPlaying)
			{
				relationship_sfxCough.clip = relationship_sfxCoughSounds[Random.Range(0, relationship_sfxCoughSounds.Length)];
				relationship_sfxCough.time = 0f;
				relationship_sfxCough.Play();
			}
		}
		else if (output is TwineLink && output.Name == "continue")
		{
			yield return StartCoroutine(ClickForAlarm(output as TwineLink));
		}
	}

	// ...........................
	// work

	public Image work_ppCursor;
	public Image work_ppTemplate;
	public Sprite[] work_ppImages;
	const int work_minShapes = 3;
	const int work_maxShapes = 12;

	IEnumerator work_Enter()
	{
		StartCoroutine(SnoozeFadeOut(2f, noise: 1f, underwater: 0f));
		uiTextPlayer.AutoDisplay = false;
		work_ppCursor.gameObject.SetActive(true);
		Screen.showCursor = false;

		// Wait a frame for all Twine output from this passage
		yield return null;

		var shapes = new List<GameObject>();

		for (int i = 0; i <= story.Text.Count; i++)
		{
			bool done = i == story.Text.Count;
			int targetClickCount = Random.Range(work_minShapes, work_maxShapes);

			for (int count = 0; count < targetClickCount; count++)
			{
				while (!Input.GetMouseButtonDown(0))
					yield return null;

				var shape = (GameObject)Instantiate(work_ppTemplate.gameObject);
				shape.SetActive(true);
				shapes.Add(shape);

				var img = shape.GetComponent<Image>();
				img.rectTransform.SetParent(work_ppTemplate.rectTransform.parent);
				img.sprite = work_ppImages[Random.Range(0, work_ppImages.Length)];
				img.transform.position = Input.mousePosition;
				yield return null;
			}

			if (done)
			{
				SoundAlarm();
				yield return new WaitForSeconds(alarmWakeUpDelay);
				
				for (int j = 0; j < shapes.Count; j++)
					GameObject.Destroy(shapes[j]);
				
				story.Advance("continue");
			}
			else
			{
				uiTextPlayer.DisplayOutput(story.Text[i]);
			}
		}
	}

	void work_Update()
	{
		work_ppCursor.transform.position = Input.mousePosition;
	}

	IEnumerator work_Exit()
	{
		work_ppCursor.gameObject.SetActive(false);
		Screen.showCursor = true;
		uiTextPlayer.AutoDisplay = true;
		return SnoozeFadeOut(alarmWakeUpDelay);
	}

	// ...........................
	// street

	public Sprite street_image;
	public AudioSource street_sfxStreet;
	public AudioSource street_sfxFootstep1;
	public AudioSource street_sfxFootstep2;
	float street_speed;
	float street_lastClickTime;
	float street_lastClickSpeed;
	float street_lastStepTime;
	bool street_acceptFootsteps;
	bool street_lastIsLeft = false;
	float street_walkTime = 0f;
	const float street_speedBoost = 0.5f;
	const float street_minSpeed = 0f;
	const float street_maxSpeed = 6f;
	const float street_slowTime = 3f;
	const float street_walkTimeTarget = 0.6f;
	const float street_walkTimeSpeed = 4f;

	IEnumerator street_Enter()
	{
		StartCoroutine(SnoozeFadeOut(2f));
		uiImage.sprite = street_image;
		FadeImage(uiImage, 0f, 1f, 3f);

		street_lastStepTime = 0f;
		street_lastClickTime = 0f;
		street_lastClickSpeed = 0f;
		street_speed = street_minSpeed;
		street_acceptFootsteps = false;
		street_walkTime = 0f;

		street_sfxStreet.time = 0f;
		street_sfxStreet.Play();
		const float fadeInTime = 2f;
		for (float t = 0; t <= fadeInTime; t += Time.deltaTime)
		{
			street_sfxStreet.volume = t / fadeInTime;
			yield return null;
		}
	}

	void street_Update()
	{
		if (!street_acceptFootsteps && uiTextPlayer.WasClicked())
		{
			// Boost the street speed
			street_speed = Mathf.Clamp(street_speed + street_speedBoost, 1f, street_maxSpeed);
			street_lastClickSpeed = street_speed;
			street_lastClickTime = Time.time;
		}
		else
			street_speed = Mathf.Lerp(street_lastClickSpeed, 0f, (Time.time - street_lastClickTime) / street_slowTime);

		if (street_speed == 0f)
			return;

		if (Time.time - street_lastStepTime > 1f/street_speed)
		{
			street_lastStepTime = Time.time;
			AudioSource audio = street_lastIsLeft ? street_sfxFootstep1 : street_sfxFootstep2;

			audio.time = 0f;
			audio.Play();

			// switch feet
			street_lastIsLeft = !street_lastIsLeft;
		}

		if (street_acceptFootsteps)
			return;

		if (street_speed >= street_walkTimeSpeed)
			street_walkTime += Time.deltaTime;
		else
			street_walkTime = 0f;

		if (street_walkTime >= street_walkTimeTarget)
		{
			street_acceptFootsteps = true;
			street_walkTime = 0f;

			if (story.CurrentPassageName == "street3")
				StartCoroutine(street_alarm());
			else
				story.Advance("continue");
		}
		
	}
	IEnumerator street2_Enter()
	{
		yield return new WaitForSeconds(1f);
		street_acceptFootsteps = false;
	}

	IEnumerator street3_Enter()
	{
		yield return new WaitForSeconds(1f);
		street_acceptFootsteps = false;
	}

	void street2_Update()
	{
		street_Update();
	}
	void street3_Update()
	{
		street_Update();
	}
	
	IEnumerator street_alarm()
	{
		SoundAlarm();
		for (float t = 0; t <= alarmWakeUpDelay; t += Time.deltaTime)
		{
			street_sfxStreet.volume = 1f - (t / alarmWakeUpDelay);
			yield return null;
		}
		street_sfxStreet.Stop();
		story.Advance("continue");
	}

	void street3_Exit()
	{
		FadeImage(uiImage, 1f, 0f, 0.1f);
	}

	//IEnumerator street3_alarm(TwineLink continueLink)
	//{
	//	SoundAlarm();
	//	for (float t = 0; t <= alarmWakeUpDelay; t += Time.deltaTime)
	//	{
	//		street_sfx.volume = 1f - (t / alarmWakeUpDelay);
	//		yield return null;
	//	}
	//	street_sfx.Stop();
	//	story.Advance(continueLink);
	//}
	
	// ...........................
	// getUp

	public Image getUp_EndImage;
	public AnimationCurve getUp_EndEase;

	IEnumerator getUp_Enter()
	{
		yield return new WaitForSeconds(1f);
		getUp_EndImage.gameObject.SetActive(true);
		FadeImage(getUp_EndImage, 0f, 1f, 5f);
	}

	// ======================================
	// Helpers

	// .............
	// Alarm

	void SoundAlarm()
	{
		if (alarm_sfx.isPlaying)
			return;

		alarm_sfx.loop = true;
		alarm_sfx.time = 0f;
		alarm_sfx.Play();
	}

	IEnumerator ClickForAlarm(TwineLink continueLink)
	{
		// Wait for a click, play alarm and then advance
		do { yield return null; }
		while (!uiTextPlayer.WasClicked());
		SoundAlarm();
		yield return new WaitForSeconds(alarmWakeUpDelay);
		story.Advance(continueLink);
	}

	// .............
	// Image fading

	Coroutine _fadeCurrent = null;
	
	void FadeImage(Image img, float from, float to, float time, float flickerMin = 0f, float flickerMax = 0f)
	{
		if (_fadeCurrent != null)
			StopCoroutine(_fadeCurrent);

		_fadeCurrent = StartCoroutine(FadeImageAnim(img, from, to, time, flickerMin, flickerMax));

	}

	IEnumerator FadeImageAnim(Image img, float from, float to, float time,
		float flickerFreqMin = 0,
		float flickerFreqMax = 0,
		float flickerMin = 0f,
		float flickerMax = 0f
		)
	{
		Color color = img.color;
		float state = (color.a - from) / (to - from);
		//float flickerTarget = -1f;

		for (float t = state*time; t <= time; t += Time.deltaTime)
		{
			color = img.color;
			
			//if (flickerFreqMin > 0f) {
			//	if (flickerTarget < 0f) {
			//		flickerTarget = 
			//	}
			//}

			color.a = Mathf.Lerp(from, to, t/time);
			img.color = color;
			yield return null;
		}

		color = img.color;
		color.a = to;
		uiImage.color = color;
		_fadeCurrent = null;
	}
}

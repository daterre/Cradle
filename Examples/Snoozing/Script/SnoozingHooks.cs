using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityTwine;

public class SnoozingHooks : MonoBehaviour {

	public TwineTextPlayer uiTextPlayer;
	public Image uiImage;
	public Animator uiTitleScreen;
	const float alarmWakeUpDelay = 0.3f;
	Coroutine _beginStory = null;
	
	TwineStory story;

	void Awake()
	{
		this.story = GetComponent<TwineStory>();
		alarm_sfxVolume = alarm_sfx.volume;
	}

	void Update()
	{
		// Detect escape button press and quit
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	// Called by the Play button on the title screen
	public void PlayStory()
	{
		if (_beginStory != null)
			return;

		uiTextPlayer.Clear();
		uiTitleScreen.SetTrigger("play");
		_beginStory = StartCoroutine(BeginStory());
	}

	IEnumerator BeginStory()
	{
		yield return new WaitForSeconds(2f);
		uiTitleScreen.gameObject.SetActive(false);
		
		this.story.Reset();
		this.story.Begin();
		_beginStory = null;
	}

	void EndStory()
	{
		uiTitleScreen.gameObject.SetActive(true);
		uiTitleScreen.SetTrigger("end");
	}

	// ...........................
	// alarm

	public AudioSource alarm_sfx;
	public Sprite alarm_imgPhone;
	public Animator alarm_buttonAnimator;
	public Button alarm_buttonObject;
	public CanvasGroup alarm_buttonGroup;
	bool alarm_buttonClicked = false;
	float alarm_sfxVolume;

	IEnumerator alarm_Enter()
	{
		alarm_buttonClicked = false;
		alarm_buttonGroup.alpha = 0f;
		alarm_buttonObject.gameObject.SetActive(false);

		yield return new WaitForSeconds(1f);
		
		SoundAlarm();

		float textDelay = story["again"] ? 1.5f : 4.5f;
		yield return new WaitForSeconds(textDelay);

		uiImage.sprite = alarm_imgPhone;
		ImageFade.Start(uiImage, 0f, 1f, 4f);

		alarm_buttonAnimator.gameObject.SetActive(true);
		alarm_buttonObject.gameObject.SetActive(true);
		alarm_buttonAnimator.SetBool("alarm", true);
		ImageFade.Start(alarm_buttonGroup, this, 0f, 1f, 0.2f);
	}

	void alarm_Update()
	{
		// Wait for snooze button to be pressed
		if (story.State == TwineStoryState.Idle && alarm_buttonClicked)
			story.Advance("snooze");
	}

	public void alarm_RegisterClick()
	{
		if (story.State == TwineStoryState.Idle)
			alarm_buttonClicked = true;
	}


	void alarm_Exit()
	{
		alarm_sfx.loop = false;

		ImageFade.Start(uiImage, 1f, 0f, 0f);

		alarm_buttonObject.gameObject.SetActive(true);
		alarm_buttonAnimator.SetBool("alarm", false);
		alarm_buttonAnimator.gameObject.SetActive(false);
		ImageFade.Start(alarm_buttonGroup, this, 1f, 0f, 0f);
		
	}

	// ...........................
	// snooze

	public AudioSource snooze_sfxNoise;
	public AudioSource snooze_sfxUnderwater;
	public Image snooze_imgAnxietyEye;
	public Image snooze_imgAnxietyHalo;
	public Image snooze_imgDreamEye;
	public Image snooze_imgDreamHalo;
	public AnimationCurve snooze_xCurve;
	public AnimationCurve snooze_yCurve;

	const float snooze_fadeIn = 4f;
	const float snooze_fadeOut = 2f; 

	IEnumerator snooze_Enter()
	{
		// Fade out alarm
		const float fadeOutTime = 0.1f;
		for (float fade = 0; fade <= fadeOutTime; fade += Time.deltaTime)
		{
			alarm_sfx.volume = Mathf.Lerp(alarm_sfxVolume, 0f, fade / fadeOutTime);
			yield return null;
		}

		alarm_sfx.Stop();

		yield return new WaitForSeconds(2f);

		snooze_sfxNoise.volume = 0f;
		snooze_sfxUnderwater.volume = 0f;
		snooze_sfxNoise.Play();
		snooze_sfxUnderwater.Play();

		snooze_imgAnxietyEye.gameObject.SetActive(true);
		snooze_imgAnxietyHalo.gameObject.SetActive(true);
		snooze_imgDreamEye.gameObject.SetActive(true);
		snooze_imgDreamHalo.gameObject.SetActive(true);

		Color colorAnxiety = new Color(1f, 1f, 1f, 0f);
		Color colorDream = new Color(1f, 1f, 1f, 0f);
		snooze_imgAnxietyEye.color = colorAnxiety;
		snooze_imgAnxietyHalo.color = colorAnxiety;
		snooze_imgDreamEye.color = colorDream;
		snooze_imgDreamHalo.color = colorDream;

		float t = 0;
		while (true)
		{
			yield return null;
			t += Time.deltaTime;
			
			float fadeIn = Mathf.Clamp(t / snooze_fadeIn, 0f, 1f);
			float anxiety = snooze_yCurve.Evaluate(Input.mousePosition.y / Screen.height);
			float dreaming = 1f - anxiety;
			float boost = snooze_xCurve.Evaluate(Input.mousePosition.x / Screen.width);

			snooze_sfxNoise.volume = anxiety * boost * fadeIn;
			snooze_sfxUnderwater.volume = dreaming * boost * fadeIn;

			colorAnxiety.a = anxiety * boost * fadeIn;
			colorDream.a = dreaming * boost * fadeIn;
			snooze_imgAnxietyEye.color = colorAnxiety;
			snooze_imgAnxietyHalo.color = colorAnxiety;
			snooze_imgDreamEye.color = colorDream;
			snooze_imgDreamHalo.color = colorDream;

			if (story.State == TwineStoryState.Idle && fadeIn == 1f && uiTextPlayer.WasClicked())
			{
				yield return null;

				const float imgFadeOutTime = 0.3f;
				ImageFade.Start(snooze_imgDreamEye, 1f, 0f, imgFadeOutTime);
				ImageFade.Start(snooze_imgDreamHalo, 1f, 0f, imgFadeOutTime);
				ImageFade.Start(snooze_imgAnxietyEye, 1f, 0f, imgFadeOutTime);
				ImageFade.Start(snooze_imgAnxietyHalo, 1f, 0f, imgFadeOutTime);

				const float herTrigger = 0.3f;
				if (boost < herTrigger) {
					story.Advance("her");
					StartCoroutine(SnoozeFadeOut(snooze_fadeOut));
				}
				else if (anxiety > dreaming) {
					story.Advance("anxiety");
				}
				else {
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
	public float her_maxScreenMove = 0.01f;
	public float her_alphaFactor = 4f;
	public float her_alphaLineTriggerUp = 0.9f;
	public float her_alphaLineTriggerDown = 0.4f;
	public float her_minFadeoutTime = 3f;
	public float her_maxFadeoutTime = 1f;
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
			if (!ImageFade.IsInProgress(uiImage) && color.a > 0f)
				ImageFade.Start(uiImage, 1f, 0f, move < her_minScreenMove ? her_minFadeoutTime : her_maxFadeoutTime);
		}
		else if (!her_lineTriggered)
		{
			ImageFade.Stop(uiImage);
			
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
		ImageFade.Start(uiImage, 1f, 0f, 0.1f);
		uiTextPlayer.AutoDisplay = true;
	}

	// ...........................
	// sea

	public Sprite sea_image;
	int sea_current = 0;

	void sea_Enter()
	{
		uiImage.sprite = sea_image;
		ImageFade.Start(uiImage, 0f, 1f, 8f);
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
		ImageFade.Start(uiImage, 1f, 0f, 0f);
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
	const int work_minShapes = 2;
	const int work_maxShapes = 8;

	IEnumerator work_Enter()
	{
		StartCoroutine(SnoozeFadeOut(2f, noise: 1f, underwater: 0f));
		uiTextPlayer.AutoDisplay = false;
		work_ppCursor.gameObject.SetActive(true);
		Cursor.visible = false;

		// Wait a frame for all Twine output from this passage
		yield return null;

		var shapes = new List<GameObject>();
		bool[] shown = new bool[work_ppImages.Length];

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

				// every third click, show a shape we haven't shown it yet
				int imgIndex = -1;
				if (count % 3 == 0) {
					for (int s = 0; s < shown.Length; s++) { 
						if (!shown[s]) {
							imgIndex = s;
						}
					}
				}
				if (imgIndex < 0)
					imgIndex = Random.Range(0, work_ppImages.Length);
				shown[imgIndex] = true;

				var img = shape.GetComponent<Image>();
				img.rectTransform.SetParent(work_ppTemplate.rectTransform.parent);
				img.sprite = work_ppImages[imgIndex];
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
		Cursor.visible = true;
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
	bool street_FootstepsPaused;
	bool street_lastIsLeft = false;
	float street_walkTime = 0f;
	const float street_speedBoost = 1f;
	const float street_minSpeed = 0f;
	const float street_maxSpeed = 6f;
	const float street_slowTime = 2.5f;
	const float street_walkTimeTarget = 0.1f;
	const float street_walkTimeSpeed = 5f;
	int street_currentLine = 0;

	IEnumerator street_Enter()
	{
		uiTextPlayer.AutoDisplay = false;
		StartCoroutine(SnoozeFadeOut(snooze_fadeOut));
		uiImage.sprite = street_image;
		ImageFade.Start(uiImage, 0f, 1f, 3f);

		street_lastStepTime = 0f;
		street_lastClickTime = 0f;
		street_lastClickSpeed = 0f;
		street_speed = street_minSpeed;
		street_FootstepsPaused = false;
		street_walkTime = 0f;
		street_currentLine = 0;

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
		if (!street_FootstepsPaused && uiTextPlayer.WasClicked())
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

		if (street_FootstepsPaused)
			return;

		if (street_speed >= street_walkTimeSpeed)
			street_walkTime += Time.deltaTime;
		else
			street_walkTime = 0f;

		if (street_walkTime >= street_walkTimeTarget)
		{
			street_walkTime = 0f;

			if (street_currentLine >= story.Text.Count - 1)
			{
				if (story.CurrentPassageName == "street3")
					StartCoroutine(street_alarm());
				else
					story.Advance("continue");
			}
			else
			{
				uiTextPlayer.DisplayOutput(story.Text[street_currentLine]);
				street_currentLine++;
				StartCoroutine(street_PauseFootSteps());
			}
		}
	}
	IEnumerator street_PauseFootSteps()
	{
		street_FootstepsPaused = true;
		yield return new WaitForSeconds(1f);
		street_FootstepsPaused = false;
	}

	IEnumerator street2_Enter()
	{
		yield return StartCoroutine(street_PauseFootSteps());
		street_currentLine = 0;
	}

	IEnumerator street3_Enter()
	{
		yield return StartCoroutine(street_PauseFootSteps());
		street_currentLine = 0;
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
		street_FootstepsPaused = true;
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
		ImageFade.Start(uiImage, 1f, 0f, 0.1f);
		uiTextPlayer.AutoDisplay = true;
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

	void getUp_Enter()
	{
		EndStory();
	}

	// ======================================
	// Helpers

	// .............
	// Alarm

	void SoundAlarm()
	{
		if (alarm_sfx.isPlaying)
			return;

		alarm_sfx.volume = alarm_sfxVolume;
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
	
}

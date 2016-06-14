using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cradle;
using UnityStandardAssets.ImageEffects;

public class SnoozingCues : MonoBehaviour {

	public Canvas uiCanvas;
	public TwineTextPlayer uiTextPlayer;
	public Image uiImage;
	public RectTransform cursor;
	public Animator uiTitleScreen;
	public AudioSource sfxMorning;
	const float alarmWakeUpDelay = 0.3f;
	Coroutine _beginStory = null;
	
	Story story;

	void Awake()
	{
		this.story = GetComponent<Story>();
		alarm_sfxVolume = alarm_sfx.volume;
	}

	IEnumerator Start()
	{
		// Fade in the audio
		sfxMorning.Play();
		sfxMorning.volume = 0;

		for (float t = 0; t <= 1f; t += Time.deltaTime)
		{
			sfxMorning.volume = t;
			yield return null; // wait a frame
		}

		sfxMorning.volume = 1f;
	}

	void Update()
	{
		// Detect escape button press and quit
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}

		cursor.transform.position = CursorGetPos();
	}

	// Called by the Play button on the title screen
	public void PlayStory()
	{
		if (_beginStory != null)
			return;

		uiTextPlayer.Clear();
		Cursor.visible = false;
		
		this.story.Reset();

		uiTitleScreen.SetTrigger("play");
		_beginStory = StartCoroutine(BeginStory());
	}

	IEnumerator BeginStory()
	{
		//yield return new WaitForSeconds(2f);

		for (float t = 0; t <= 2f; t += Time.deltaTime)
		{
			sfxMorning.volume = 1f-(t/2f);
			yield return null; // wait a frame
		}
		sfxMorning.Stop();

		uiTitleScreen.gameObject.SetActive(false);	
		this.story.Begin();
		_beginStory = null;
	}

	IEnumerator EndStory()
	{
		uiTitleScreen.gameObject.SetActive(true);
		uiTitleScreen.SetTrigger("end");

		sfxMorning.Play();
		for (float t = 0; t <= 5f; t += Time.deltaTime)
		{
			sfxMorning.volume = t / 5f;
			yield return null; // wait a frame
		}

		Cursor.visible = true;
	}

	// ...........................
	// alarm

	public AudioSource alarm_sfx;
	public Sprite alarm_imgPhone;
	public Animator alarm_buttonAnimator;
	public Button alarm_buttonObject;
	public CanvasGroup alarm_buttonGroup;
	public RectTransform alarm_cursor;
	bool alarm_buttonClicked;
	float alarm_sfxVolume;

	IEnumerator alarm_Enter()
	{
		Camera.main.GetComponent<NoiseAndScratches>().enabled = false;

		alarm_buttonClicked = false;
		alarm_buttonGroup.alpha = 0f;
		alarm_buttonObject.gameObject.SetActive(false);

		yield return new WaitForSeconds(1f);
		
		SoundAlarm();

		float textDelay = story.Vars["again"] ? 1.5f : 4.5f;
		yield return new WaitForSeconds(textDelay);

		uiImage.sprite = alarm_imgPhone;
		ImageFade.Start(uiImage, 0f, 1f, 4f);
		
		CursorSet(alarm_cursor, 4f);

		alarm_buttonAnimator.gameObject.SetActive(true);
		alarm_buttonObject.gameObject.SetActive(true);
		alarm_buttonAnimator.SetBool("alarm", true);
		ImageFade.Start(alarm_buttonGroup, this, 0f, 1f, 0.2f);
	}

	void alarm_Done()
	{
		CursorHoverActions(
			cursorAnimator => cursorAnimator.SetBool("hover", true),
			cursorAnimator => cursorAnimator.SetBool("hover", false),
			new StoryLink[] { story.GetCurrentLinks().Where(link => link.PassageName == "getUp").First() },
			new GameObject[] { alarm_buttonObject.gameObject }
		);
	}

	void alarm_Update()
	{
		// Wait for snooze button to be pressed
		if (story.State == StoryState.Idle && alarm_buttonClicked)
			story.DoLink("snooze");
	}

	public void alarm_RegisterClick()
	{
		if (story.State == StoryState.Idle)
			alarm_buttonClicked = true;
	}


	void alarm_Exit()
	{
		alarm_sfx.loop = false;

		const float alarmStopTime = 0.1f;
		ImageFade.Start(uiImage, 1f, 0f, alarmStopTime);
		ImageFade.Start(alarm_buttonGroup, this, 1f, 0f, alarmStopTime);
		CursorHide(alarmStopTime);
		CursorHoverClear(
			new StoryLink[] { story.GetCurrentLinks().Where(link => link.PassageName == "getUp").First() },
			new GameObject[] { alarm_buttonObject.gameObject }
		);

		//yield return new WaitForSeconds(alarmStopTime);

		alarm_buttonObject.gameObject.SetActive(false);
		alarm_buttonAnimator.SetBool("alarm", false);
		alarm_buttonAnimator.gameObject.SetActive(false);
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
	public RectTransform snooze_cursor;
	public float snooze_noiseIntensity = 0.1f;

	const float snooze_fadeIn = 4f;
	const float snooze_fadeOut = 2f;
	const float snooze_herTrigger = 0.3f;

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

		CursorSet(snooze_cursor, snooze_fadeIn);

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

		var cameraNoise = Camera.main.GetComponent<NoiseAndScratches>();
		cameraNoise.grainIntensityMin = 0f;
		cameraNoise.grainIntensityMax = 0f;
		cameraNoise.enabled = true;

		Animator cursorAnimator = cursor.GetComponentInChildren<Animator>();

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

			cameraNoise.grainIntensityMin = snooze_noiseIntensity * fadeIn;
			cameraNoise.grainIntensityMax = snooze_noiseIntensity * fadeIn;

			cursorAnimator.SetInteger("snooze",
				boost < snooze_herTrigger ? 0 :
				anxiety > dreaming ? 1 :
				-1
			);

			colorAnxiety.a = anxiety * boost * fadeIn;
			colorDream.a = dreaming * boost * fadeIn;
			snooze_imgAnxietyEye.color = colorAnxiety;
			snooze_imgAnxietyHalo.color = colorAnxiety;
			snooze_imgDreamEye.color = colorDream;
			snooze_imgDreamHalo.color = colorDream;

			if (story.State == StoryState.Idle && fadeIn == 1f && uiTextPlayer.WasClicked())
			{
				yield return null;

				const float imgFadeOutTime = 0.3f;
				ImageFade.Start(snooze_imgDreamEye, 1f, 0f, imgFadeOutTime);
				ImageFade.Start(snooze_imgDreamHalo, 1f, 0f, imgFadeOutTime);
				ImageFade.Start(snooze_imgAnxietyEye, 1f, 0f, imgFadeOutTime);
				ImageFade.Start(snooze_imgAnxietyHalo, 1f, 0f, imgFadeOutTime);

				CursorHide(snooze_fadeOut);

				
				if (boost < snooze_herTrigger) {
					story.DoLink("her");
					StartCoroutine(SnoozeFadeOut(snooze_fadeOut));
				}
				else if (anxiety > dreaming) {
					story.DoLink("anxiety");
				}
				else {
					story.DoLink("dream");
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
	public RectTransform her_cursorMatch;
	public float her_minScreenMove = 0.01f;
	public float her_maxScreenMove = 0.01f;
	public float her_alphaFactor = 4f;
	public float her_alphaLineTriggerUp = 0.9f;
	public float her_alphaLineTriggerDown = 0.4f;
	public float her_minFadeoutTime = 3f;
	public float her_maxFadeoutTime = 1f;
	public AudioSource her_sfxBreathing;
	public AudioSource her_sfxMatch;
	public AudioSource her_sfxMatchLight;
	int her_outputIndex;
	bool her_lineTriggered;
	bool her_done;
	Vector3 her_lastMousePos;
	Animator her_cursorFlame;

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

		CursorSet(her_cursorMatch, 3f);
		her_cursorFlame = this.cursor.GetComponentInChildren<Animator>();
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

			if (!her_sfxMatch.isPlaying)
				her_sfxMatch.Play();
		}

		// Show another line
		if (!her_lineTriggered && color.a >= her_alphaLineTriggerUp)
		{
			StoryOutput output = null;
			while (her_outputIndex < story.Output.Count && !(output is LineBreak))
			{
				output = story.Output[her_outputIndex];
				if (output is StoryText)
				{
					var line = (StoryText)output;
					uiTextPlayer.DisplayOutput(line);
				}
				else if (output is StoryLink && output.Name == "continue")
				{
					her_done = true;
					StartCoroutine(her_alarm(output as StoryLink));
					break;
				}
				her_outputIndex++;
			}

			// Show the closing line break, if any
			if (output != null)
				uiTextPlayer.DisplayOutput(output);

			// Play the match light sound
			if (!her_done)
			{
				her_sfxMatchLight.Play();
				her_cursorFlame.SetTrigger("light");
			}

			her_lineTriggered = true;
		}
		else if (her_lineTriggered && color.a <= her_alphaLineTriggerDown)
		{
			her_lineTriggered = false;
		}

		her_lastMousePos = Input.mousePosition;
	}

	IEnumerator her_alarm(StoryLink continueLink)
	{
		SoundAlarm();
		for (float t = 0; t <= alarmWakeUpDelay; t += Time.deltaTime)
		{
			her_sfxBreathing.volume = 1f - (t / alarmWakeUpDelay);
			yield return null;
		}
		her_sfxBreathing.Stop();
		story.DoLink(continueLink);
	}

	void her_Exit()
	{
		CursorHide(0.1f);
		ImageFade.Start(uiImage, 1f, 0f, 0.1f);
		uiTextPlayer.AutoDisplay = true;
	}

	// ...........................
	// sea

	public Sprite sea_image;
	public RectTransform sea_cursor;
	public AudioClip[] sea_sfxBubbleSounds;
	public AudioSource sea_sfxBubbles;
	int sea_current;

	void sea_Enter()
	{
		sea_current = 0;

		uiImage.sprite = sea_image;
		ImageFade.Start(uiImage, 0f, 1f, 8f);
		StartCoroutine(SnoozeFadeOut(2f, noise: 0f, underwater: 1f));
		
		CursorSet(sea_cursor, 8f);
	}

	IEnumerator sea_Output(StoryOutput output)
	{
		if (output is StoryLink && output.Name == "continue")
		{
			yield return StartCoroutine(ClickForAlarm(output as StoryLink));
		}
		else if (output is StoryText && output.Text.Trim().Length > 0)
		{
			sea_sfxBubbles.PlayOneShot(sea_sfxBubbleSounds[Random.Range(0, sea_sfxBubbleSounds.Length)], sea_sfxBubbles.volume);
		}
	}

	IEnumerator sea_Exit()
	{
		CursorHide();

		ImageFade.Start(uiImage, 1f, 0f, 0f);
		return SnoozeFadeOut(alarmWakeUpDelay);
	}

	// ...........................
	// relationship

	public AudioSource relationship_sfxCough;
	public AudioClip[] relationship_sfxCoughSounds;
	public RectTransform relationship_cursorEye;
	Animator relationship_cursorAnimator;
	bool relationship_lastClick;

	void relationship_Enter()
	{
		relationship_lastClick = false;

		StartCoroutine(SnoozeFadeOut(2f, noise: 0.3f, underwater: 0f));

		CursorSet(relationship_cursorEye, 2f);

		relationship_cursorAnimator = cursor.GetComponentInChildren<Animator>();
	}

	void relationship_Update()
	{
		if (!relationship_lastClick && Input.GetMouseButtonUp(0))
			relationship_cursorAnimator.SetTrigger("open");
	}

	IEnumerator relationship_Output(StoryOutput output)
	{
		if (output is StoryText && output.Text.Trim().Length > 0)
		{
			relationship_sfxCough.PlayOneShot(
				relationship_sfxCoughSounds[Random.Range(0, relationship_sfxCoughSounds.Length)],
				relationship_sfxCough.volume);
		}
		else if (output is StoryLink && output.Name == "continue")
		{
			relationship_lastClick = true;
			yield return StartCoroutine(ClickForAlarm(output as StoryLink));
		}
	}

	IEnumerator relationship_Exit()
	{
		CursorHide(alarmWakeUpDelay);
		return SnoozeFadeOut(alarmWakeUpDelay);
	}

	// ...........................
	// work

	public Image work_ppTemplate;
	public RectTransform work_cursor;
	public Sprite[] work_ppImages;
	public AudioSource work_sfxOffice;
	public AudioSource work_sfxMouseClick;
	const int work_minShapes = 2;
	const int work_maxShapes = 8;

	IEnumerator work_Enter()
	{
		StartCoroutine(SnoozeFadeOut(2f, noise: 0.3f, underwater: 0f));
		uiTextPlayer.AutoDisplay = false;
		CursorSet(work_cursor);

		// Fade in the ambient. Because there's a yield null in here, all passage output will be 
		// available by the time this is done
		work_sfxOffice.volume = 0f;
		work_sfxOffice.Play();
		for (float t = 0; t <= 1f; t += Time.deltaTime)
		{
			work_sfxOffice.volume = t/1f;
			yield return null;
		}

		var shapes = new List<GameObject>();
		bool[] shown = new bool[work_ppImages.Length];
		bool waitingForLine = true;

		// Handle all output
		for (int i = 0; i < story.Output.Count; i++)
		{
			// Exit When reaching the continue link
			if (story.Output[i] is StoryLink)
			{
				var continueLink = (StoryLink)story.Output[i];

				SoundAlarm();

				for (float t = 0; t <= alarmWakeUpDelay; t += Time.deltaTime)
				{
					work_sfxOffice.volume = 1f - (t / alarmWakeUpDelay);
					yield return null;
				}
				work_sfxOffice.Stop();

				for (int j = 0; j < shapes.Count; j++)
					GameObject.Destroy(shapes[j]);

				story.DoLink(continueLink);
				break;
			}

			// Otherwise, display a line
			if (waitingForLine)
			{
				waitingForLine = false;

				int targetClickCount = Random.Range(work_minShapes, work_maxShapes);
				for (int count = 0; count < targetClickCount; count++)
				{
					while (!Input.GetMouseButtonDown(0))
						yield return null;

					// Play click sound
					work_sfxMouseClick.Play();

					// Add shape
					var shape = (GameObject)Instantiate(work_ppTemplate.gameObject);
					shape.transform.SetParent(work_ppTemplate.rectTransform.parent);
					shape.transform.localScale = work_ppTemplate.transform.localScale;
					shape.transform.position = CursorGetPos();
					shape.SetActive(true);
					shapes.Add(shape);

					// every third click, show a shape we haven't shown it yet
					int imgIndex = -1;
					if (count % 3 == 0)
					{
						for (int s = 0; s < shown.Length; s++)
						{
							if (!shown[s])
							{
								imgIndex = s;
							}
						}
					}
					if (imgIndex < 0)
						imgIndex = Random.Range(0, work_ppImages.Length);
					shown[imgIndex] = true;

					var img = shape.GetComponent<Image>();
					img.sprite = work_ppImages[imgIndex];
					yield return null;
				}
			}

			uiTextPlayer.DisplayOutput(story.Output[i]);

			// Pause when reaching a line break
			waitingForLine = story.Output[i] is LineBreak;
		}
	}

	IEnumerator work_Exit()
	{
		CursorHide();
		uiTextPlayer.AutoDisplay = true;
		return SnoozeFadeOut(alarmWakeUpDelay);
	}

	// ...........................
	// street

	public Sprite street_image;
	public RectTransform street_cursor;
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
	const float street_speedBoost = 2f;
	const float street_minSpeed = 0f;
	const float street_maxSpeed = 6f;
	const float street_slowTime = 2.5f;
	const float street_walkTimeTarget = 0.1f;
	const float street_walkTimeSpeed = 5f;
	int street_currentOutput = 0;
	bool street_lineShown = false;

	IEnumerator street_Enter()
	{
		uiTextPlayer.AutoDisplay = false;
		StartCoroutine(SnoozeFadeOut(snooze_fadeOut));
		uiImage.sprite = street_image;
		ImageFade.Start(uiImage, 0f, 1f, 3f);
		
		CursorSet(street_cursor, 1f);

		street_lastStepTime = 0f;
		street_lastClickTime = 0f;
		street_lastClickSpeed = 0f;
		street_speed = street_minSpeed;
		street_FootstepsPaused = false;
		street_walkTime = 0f;
		street_currentOutput = 0;
		street_lineShown = false;

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

			// shake image
			uiImage.GetComponent<Animator>().SetTrigger("sidewalkShake");

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

			if (street_currentOutput >= story.Output.Count - 1)
			{
				street_lineShown = false;

				if (story.CurrentPassageName == "street3")
					StartCoroutine(street_alarm());
				else
					story.DoLink("continue");
			}
			else
			{
				while (true)
				{
					StoryOutput output = story.Output[street_currentOutput++];
					uiTextPlayer.DisplayOutput(output);
					street_lineShown |= output is StoryText;

					if (street_lineShown && output is LineBreak)
						break;
				}
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
		street_currentOutput = 0;
	}

	IEnumerator street3_Enter()
	{
		yield return StartCoroutine(street_PauseFootSteps());
		street_currentOutput = 0;
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
		story.DoLink("continue");
	}

	IEnumerator street3_Exit()
	{
		CursorHide();

		ImageFade.Start(uiImage, 1f, 0f, 0.1f);
		uiTextPlayer.AutoDisplay = true;
		return SnoozeFadeOut(alarmWakeUpDelay);
	}

	// ...........................
	// alone

	public Animator alone_body;
	public float alone_noiseIntensity = 0.3f;

	IEnumerator alone_Enter()
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

		alone_body.gameObject.SetActive(true);
		alone_body.SetBool("on", true);

		const float fadeInTime = 1.5f;

		var cameraNoise = Camera.main.GetComponent<NoiseAndScratches>();
		cameraNoise.grainIntensityMin = 0f;
		cameraNoise.grainIntensityMax = 0f;
		cameraNoise.enabled = true;
		for (float t = 0f; t <= fadeInTime; t+=Time.deltaTime)
		{
			cameraNoise.grainIntensityMin = Mathf.Clamp01(t / fadeInTime) * alone_noiseIntensity;
			cameraNoise.grainIntensityMax = Mathf.Clamp01(t / fadeInTime) * alone_noiseIntensity;
			yield return null;
		}

		CursorSet(alarm_cursor, 0.5f);
		CursorHoverActions(
			cursorAnimator => alone_body.SetBool("on", false),
			cursorAnimator => alone_body.SetBool("on", true),
			objects: new GameObject[] { alone_body.gameObject }
		);

		yield return new WaitForSeconds(3.5f);

		CursorHoverActions(
			cursorAnimator => cursorAnimator.SetBool("hover", true),
			cursorAnimator => cursorAnimator.SetBool("hover", false),
			new StoryLink[] {story.GetCurrentLinks().Where(link => link.PassageName == "getUp").First()}
		);
	}

	IEnumerator alone_Exit()
	{
		story.Pause();

		CursorHoverClear(objects: new GameObject[] { alone_body.gameObject });
		CursorHide(0.3f);


		yield return new WaitForSeconds(0.5f);
		alone_body.SetBool("on", false);
		uiTextPlayer.Clear();

		const float fadeOutTime = 2f;
		var cameraNoise = Camera.main.GetComponent<NoiseAndScratches>();
		for (float t = 0f; t <= fadeOutTime; t += Time.deltaTime)
		{
			cameraNoise.grainIntensityMin = Mathf.Clamp01(1f - t / fadeOutTime) * alone_noiseIntensity;
			cameraNoise.grainIntensityMax = Mathf.Clamp01(1f - t / fadeOutTime) * alone_noiseIntensity;
			yield return null;
		}
		cameraNoise.enabled = false;

		story.Resume();
	}
	
	// ...........................
	// getUp

	public Image getUp_EndImage;
	public AnimationCurve getUp_EndEase;

	void getUp_Enter()
	{
		StartCoroutine(EndStory());
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

	IEnumerator ClickForAlarm(StoryLink continueLink, System.Action action = null)
	{
		// Wait for a click, play alarm and then advance
		do { yield return null; }
		while (!uiTextPlayer.WasClicked());
		
		if (action != null)
			action.Invoke();

		SoundAlarm();
		yield return new WaitForSeconds(alarmWakeUpDelay);
		story.DoLink(continueLink);
	}

	void CursorSet(RectTransform prefab, float fadeIn = 0f)
	{
		Image img = null;
		CanvasGroup group = null;

		// Stop any cursor fades
		img = this.cursor.GetComponentInChildren<Image>();
		group = this.cursor.GetComponentInChildren<CanvasGroup>();
		
		if (group != null)
			ImageFade.Stop(group, this);
		else if (img != null)
			ImageFade.Stop(img);

		// Remove all cursor content
		for (int i = 0; i < this.cursor.childCount; i++)
			GameObject.Destroy(this.cursor.GetChild(i).gameObject);

		this.cursor.DetachChildren();

		// Create a new cursor
		RectTransform child = Instantiate(prefab);
		child.SetParent(this.cursor);
		child.localPosition = prefab.localPosition;
		child.localScale = prefab.localScale;
		
		if (fadeIn > 0f)
		{
			group = child.GetComponent<CanvasGroup>();
			img = child.GetComponent<Image>();

			if (group != null)
			{
				group.alpha = 0f;
			}
			else if (img != null)
			{
				Color c = img.color;
				c.a = 0f;
				img.color = c;
			}
		}

		CursorShow(fadeIn);
	}

	void CursorShow(float fadeIn = 0f)
	{
		cursor.gameObject.SetActive(true);
		
		// Fade in if necessary
		if (fadeIn > 0f)
		{
			CanvasGroup group = this.cursor.GetComponentInChildren<CanvasGroup>();
			Image img = this.cursor.GetComponentInChildren<Image>();
			if (group != null)
				ImageFade.Start(group, this, 0f, 1f, fadeIn);
			else if (img != null)
				ImageFade.Start(img, 0f, 1f, fadeIn);
		}

	}

	void CursorHide(float fadeOut = 0f)
	{
		// Fade out if necessary
		if (fadeOut > 0f)
		{
			CanvasGroup group = this.cursor.GetComponentInChildren<CanvasGroup>();
			Image img = this.cursor.GetComponentInChildren<Image>();
			if (group = null)
			{
				ImageFade.Start(group, this, 1f, 0f, fadeOut, () => this.cursor.gameObject.SetActive(false));
				return;
			}
			else if (img != null)
			{
				ImageFade.Start(img, 1f, 0f, fadeOut, () => this.cursor.gameObject.SetActive(false));
				return;
			}
		}

		this.cursor.gameObject.SetActive(false);
	}

	Vector3 CursorGetPos()
	{
		Vector2 pos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(uiCanvas.transform as RectTransform, Input.mousePosition, uiCanvas.worldCamera, out pos);
		return uiCanvas.transform.TransformPoint(pos);
	}

	void CursorHoverActions(System.Action<Animator> onEnter, System.Action<Animator> onExit, StoryLink[] links = null, GameObject[] objects = null)
	{
		IEnumerable<GameObject> all = null;
		
		if (links != null)
			all = uiTextPlayer.Container
				.GetComponentsInChildren<TwineTextPlayerElement>()
				.Where(elem => links.Contains(elem.SourceOutput))
				.Select(elem => elem.gameObject);

		if (objects != null)
			all = all != null ? all.Concat(objects) : objects;

		Animator cursorAnimator = cursor.GetComponentInChildren<Animator>();
		foreach(GameObject obj in all)
		{
			PointerHover hover = obj.GetComponent<PointerHover>();
			if (hover == null)
				hover = obj.AddComponent<PointerHover>();

			hover.OnPointerEnter = () => onEnter(cursorAnimator);
			hover.OnPointerExit = () => onExit(cursorAnimator);
		}
	}

	void CursorHoverClear(StoryLink[] links = null, GameObject[] objects = null)
	{
		IEnumerable<GameObject> all = null;

		if (links != null)
			all = uiTextPlayer.Container
				.GetComponentsInChildren<TwineTextPlayerElement>()
				.Where(elem => links.Contains(elem.SourceOutput))
				.Select(elem => elem.gameObject);

		if (objects != null)
			all = all != null ? all.Concat(objects) : objects;

		Animator cursorAnimator = cursor.GetComponentInChildren<Animator>();
		foreach (GameObject obj in all)
		{
			PointerHover hover = obj.GetComponent<PointerHover>();
			if (hover == null)
				continue;

			hover.OnPointerEnter = null;
			hover.OnPointerExit = null;
		}
	}
}

using UnityEngine;
using System.Collections;
using UnityTwine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TwineTextPlayer : MonoBehaviour {

	public TwineStory Story;
	public RectTransform Container;
	public Button LinkTemplate;
	public Text TextTemplate;
	public bool AutoDisplay = true;
	public bool ShowEmptyLines = false;
	public bool ShowNamedLinks = true;

	bool _clicked = false;

	// Use this for initialization
	void Start () {
		LinkTemplate.gameObject.SetActive(false);
		TextTemplate.gameObject.SetActive(false);
		((RectTransform)LinkTemplate.transform).SetParent(null);
		TextTemplate.rectTransform.SetParent(null);

		this.Story.OnStateChanged += Story_OnStateChanged;
		this.Story.OnOutput += Story_OnOutput;
		this.Story.Begin();
	}

	void OnDestroy()
	{
		if (this.Story != null)
		{
			this.Story.OnStateChanged -= Story_OnStateChanged;
			this.Story.OnOutput -= Story_OnOutput;
		}
	}

	// .....................
	// Clicks

	void LateUpdate()
	{
		_clicked = false;
	}

	public void RegisterClick()
	{
		_clicked = true;
	}

	public bool WasClicked()
	{
		bool clicked = _clicked;
		//if (consume)
			//_clicked = false;
		return clicked;
	}

	void Story_OnStateChanged(TwineStoryState state)
	{
		if (state == TwineStoryState.Playing && this.Story.Output.Count == 0)
		{
			// Clear previous output
			for (int i = 0; i < Container.childCount; i++)
				GameObject.Destroy(Container.GetChild(i).gameObject);
		}
	}

	void Story_OnOutput(TwineOutput output)
	{
		if (!this.AutoDisplay)
			return;

		// Check if a wait is needed
		float wait = 0f;
		try { wait = (float) Story["wait"].ToDouble(); }
		catch (KeyNotFoundException) { }

		// Check if a click in needed (only for links and non-empty text lines)
		bool click = false;
		if ((output is TwineLink || output is TwineText) && output.Text.Length > 0)
		{
			try { click = Story["click"].ToBool(); }
			catch (KeyNotFoundException) { }
		}

		if (click || wait > 0f)
		{
			Story.Pause();
			StartCoroutine(Wait(wait, click, output));
		}
		else
			DisplayOutput(output);
	}

	IEnumerator Wait(float wait, bool click, TwineOutput output)
	{
		if (wait > 0f)
		yield return new WaitForSeconds(wait);

		if (click)
		{
			while (!this.WasClicked())
				yield return null;
		}

		DisplayOutput(output);

		yield return null;
		Story["wait"] = 0.0;
		Story.Resume();
	}

	public void DisplayOutput(TwineOutput output)
	{
		RectTransform child = null;
		if (output is TwineText)
		{
			var text = (TwineText)output;
			if (!ShowEmptyLines && text.Text.Trim().Length < 1)
				return;

			Text uiText = (Text)Instantiate(TextTemplate);
			uiText.gameObject.SetActive(true);
			uiText.text = text.Text;
			child = uiText.rectTransform;
		}
		else if (output is TwineLink)
		{
			var link = (TwineLink)output;
			if (!ShowNamedLinks && link.Name != link.Text)
				return;

			Button uiLink = (Button)Instantiate(LinkTemplate);
			uiLink.gameObject.SetActive(true);
			Text uiLinkText = uiLink.GetComponentInChildren<Text>();
			uiLinkText.text = link.Text;
			uiLink.onClick.AddListener(() => this.Story.Advance(link));
			child = (RectTransform)uiLink.transform;
		}
		else
			return;

		if (child != null)
			child.SetParent(Container);
	}

}

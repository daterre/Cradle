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
	public bool Auto = true;
	public bool ShowEmptyLines = false;
	public bool ShowNamedLinks = true;

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
		if (!this.Auto)
			return;

		float wait;
		try { wait = (float) Story["wait"].ToDouble(); }
		catch (KeyNotFoundException)
		{
			return;
		}
		if (wait > 0f)
		{
			Story.Pause();
			StartCoroutine(Wait(wait, output));
		}
		else
			DisplayOutput(output);
	}

	IEnumerator Wait(float time, TwineOutput output)
	{
		yield return new WaitForSeconds(time);

		DisplayOutput(output);

		Story["wait"] = 0.0;
		Story.Resume();
	}

	public void DisplayOutput(TwineOutput output)
	{
		RectTransform child = null;
		if (output is TwineText)
		{
			var text = (TwineText)output;
			if (!ShowEmptyLines && text.String.Trim().Length < 1)
				return;

			Text uiText = (Text)Instantiate(TextTemplate);
			uiText.gameObject.SetActive(true);
			uiText.text = text.String;
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

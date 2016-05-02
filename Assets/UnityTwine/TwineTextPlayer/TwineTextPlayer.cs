﻿using UnityEngine;
using System.Collections;
using UnityTwine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityTwine.StoryFormats.Harlowe;

public class TwineTextPlayer : MonoBehaviour {

	public TwineStory Story;
	public RectTransform Container;
	public Button LinkTemplate;
	public Text WordTemplate;
	public RectTransform LineBreakTemplate;
	public bool StartStory = true;
	public bool AutoDisplay = true;
	public bool ShowNamedLinks = true;

	bool _clicked = false;
	int _insertIndex = -1;

	// Use this for initialization
	void Start () {
		LinkTemplate.gameObject.SetActive(false);
		((RectTransform)LinkTemplate.transform).SetParent(null);
		LinkTemplate.transform.hideFlags = HideFlags.HideInHierarchy;
		
		WordTemplate.gameObject.SetActive(false);
		WordTemplate.rectTransform.SetParent(null);
		WordTemplate.rectTransform.hideFlags = HideFlags.HideInHierarchy;
		
		LineBreakTemplate.gameObject.SetActive(false);
		LineBreakTemplate.SetParent(null);
		LineBreakTemplate.hideFlags = HideFlags.HideInHierarchy;

		if (this.Story == null)
			this.Story = this.GetComponent<TwineStory>();
		if (this.Story == null)
		{
			Debug.LogError("Text player does not have a story to play. Add a story script to the text player game object, or assign the Story variable of the text player.");
			return;
		}

		this.Story.OnStateChanged += Story_OnStateChanged;
		this.Story.OnOutput += Story_OnOutput;

		if (StartStory)
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
		return clicked;
	}

	public void Clear()
	{
		for (int i = 0; i < Container.childCount; i++)
			GameObject.Destroy(Container.GetChild(i).gameObject);

		_insertIndex = -1;
	}

	void Story_OnStateChanged(TwineStoryState state)
	{
		if (state == TwineStoryState.Playing && this.Story.Output.Count == 1)
		{
			// Clear previous output
			Clear();
		}
	}

	void Story_OnOutput(TwineOutput output)
	{
		if (!this.AutoDisplay)
			return;

		// Check if a wait is needed
		float wait = 0f;
		//try { wait = (float) Story.Vars["wait"].ToDouble(); }
		//catch (KeyNotFoundException) { }

		// Check if a click in needed (only for links and non-empty text lines)
		bool click = false;
		if ((output is TwineLink || output is TwineText) && output.Text.Length > 0)
		{
			//try { click = Story.Vars["click"].ToBool(); }
			//catch (KeyNotFoundException) { }
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
		Story.Vars["wait"] = 0.0;
		Story.Resume();
	}

	public void DisplayOutput(TwineOutput output)
	{
		if (output is TwineText)
		{
			var text = (TwineText)output;
			string[] words = text.Text.Split(' ');
			for (int i = 0; i < words.Length; i++)
			{
				string word = words[i];
				Text uiWord = (Text)Instantiate(WordTemplate);
				uiWord.gameObject.SetActive(true);
				uiWord.text = word;
				uiWord.name = word.Trim().Length == 0 ? "(sp)" : word;
				AddToPlayer(uiWord.rectTransform, output, _insertIndex);
				if (_insertIndex >= 0)
					_insertIndex++;
			}
		}
		else if (output is TwineLink)
		{
			var link = (TwineLink)output;
			if (!ShowNamedLinks && link.Name != link.Text)
				return;

			Button uiLink = (Button)Instantiate(LinkTemplate);
			uiLink.gameObject.SetActive(true);
			uiLink.name = "[[" + link.Text + "]]";

			Text uiLinkText = uiLink.GetComponentInChildren<Text>();
			uiLinkText.text = link.Text;
			uiLink.onClick.AddListener(() =>
			{
				this.Story.DoLink(link);
			});
			AddToPlayer((RectTransform)uiLink.transform, output, _insertIndex);
		}
		else if (output is TwineLineBreak)
		{
			var br = (RectTransform)Instantiate(LineBreakTemplate);
			br.gameObject.SetActive(true);
			br.gameObject.name = "(br)";
			AddToPlayer(br, output, _insertIndex);
		}
		else if (output is HarloweEnchantment)
		{
			var enchant = (HarloweEnchantment)output;
			ProcessHarloweEnchantment(enchant);
		}
	}

	void AddToPlayer(RectTransform rect, TwineOutput output, int childIndex = -1)
	{
		rect.SetParent(Container);
		if (childIndex >= 0)
			rect.SetSiblingIndex(childIndex);

		rect.gameObject.AddComponent<TwineTextPlayerElement>().SourceOutput = output;
	}

	void ProcessHarloweEnchantment(HarloweEnchantment enchant)
	{
		foreach (HarloweEnchantmentTarget target in enchant.Targets)
		{
			int childCount = Container.childCount;

			// Remove all elements related to this output
			int firstChildIndex = -1;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = Container.GetChild(i);
				var element = child.GetComponent<TwineTextPlayerElement>();
				if (element.SourceOutput != target.Output)
					continue;

				// found!
				if (firstChildIndex < 0)
					firstChildIndex = i;

				GameObject.Destroy(child.gameObject);
			}

			_insertIndex = firstChildIndex;

			// Create a link for this action and copy the context
			var link = new TwineLink(target.Output.Text, enchant.Action)
			{
				ContextInfo = target.Output.ContextInfo
			};

			string enchantType = enchant.ContextInfo.Get<string>(HarloweContextOptions.EnchantType);
			if (enchantType == "replace")
			{
				// Simulate the link
				this.Story.DoLink(link);
			}
			else
			{
				// Add the link to the UI
				DisplayOutput(link);
			}
		}
	}
}

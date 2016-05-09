using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityTwine;
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
	Stack<int> _enchantIndex = new Stack<int>();

	static Regex rx_splitText = new Regex(@"(\s+|[^\s]+)");

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

		this.Story.OnPassageEnter += Story_OnPassageEnter;
		this.Story.OnOutput += Story_OnOutput;

		if (StartStory)
			this.Story.Begin();
	}

	void OnDestroy()
	{
		if (this.Story != null)
		{
			this.Story.OnPassageEnter -= Story_OnPassageEnter;
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
		Container.DetachChildren();

		_enchantIndex.Clear();
	}

	void Story_OnPassageEnter(TwinePassage passage)
	{
		Clear();
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
			foreach (Match m in rx_splitText.Matches(text.Text))
			{
				string word = m.Value;
				Text uiWord = (Text)Instantiate(WordTemplate);
				uiWord.gameObject.SetActive(true);
				uiWord.text = word;
				uiWord.name = word.Length == 0 ? "(sp)" : word;
				AddToUI(uiWord.rectTransform, output);
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
			AddToUI((RectTransform)uiLink.transform, output);
		}
		else if (output is TwineLineBreak)
		{
			var br = (RectTransform)Instantiate(LineBreakTemplate);
			br.gameObject.SetActive(true);
			br.gameObject.name = "(br)";
			AddToUI(br, output);
		}
		else if (output is TwineStyleTag)
		{
			var styleTag = (TwineStyleTag)output;
			HarloweEnchantment enchant = styleTag.InnerStyle.GetValues<HarloweEnchantment>(HarloweStyleSettings.Enchantment).LastOrDefault();
			if (enchant != null)
			{
				if (styleTag.TagType == TwineStyleTagType.Opener)
					BeginHarloweEnchantment(enchant);
				else
					EndHarloweEnchantment(enchant);
			}
		}
	}

	void AddToUI(RectTransform rect, TwineOutput output)
	{
		rect.SetParent(Container);
		if (_enchantIndex.Count > 0)
		{
			int index = _enchantIndex.Pop();
			rect.SetSiblingIndex(index);
			_enchantIndex.Push(index + 1);
		}

		var elem = rect.gameObject.AddComponent<TwineTextPlayerElement>();
		elem.SourceOutput = output;
	}

	void BeginHarloweEnchantment(HarloweEnchantment enchant)
	{
		int firstChildIndex = -1;
		int lastChildIndex = -1;

		foreach (TwineOutput affectedOutput in enchant.Affected)
		{
			// Remove all elements related to this output	
			for (int i = 0; i < Container.childCount; i++)
			{
				TwineTextPlayerElement elem = Container.GetChild(i).GetComponent<TwineTextPlayerElement>();
				if (elem.SourceOutput != affectedOutput)
					continue;

				// found!
				if (firstChildIndex < 0)
					firstChildIndex = i;

				lastChildIndex = i;
			}
		}

		if (enchant.Command == HarloweEnchantCommand.Append)
			_enchantIndex.Push(lastChildIndex+1);
		else
			_enchantIndex.Push(firstChildIndex);
	}

	void EndHarloweEnchantment(HarloweEnchantment enchant)
	{
		_enchantIndex.Pop();

		if (enchant.Command != HarloweEnchantCommand.Replace)
			return;

		// Remove all elements in the "replace" enchantment
		foreach (var elem in Container.GetComponentsInChildren<TwineTextPlayerElement>()
			.Where(e => enchant.Affected.Contains(e.SourceOutput)))
		{
			elem.transform.SetParent(null);
			GameObject.Destroy(elem.gameObject);
		}
	}
}

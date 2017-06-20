using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Cradle;
using Cradle.StoryFormats.Harlowe;

[ExecuteInEditMode]
public class TwineTextPlayer : MonoBehaviour {

	public Story Story;
	public RectTransform Container;
	public Button LinkTemplate;
	public Text WordTemplate;
	public RectTransform LineBreakTemplate;
	public bool StartStory = true;
	public bool AutoDisplay = true;
	public bool ShowNamedLinks = true;

	static Regex rx_splitText = new Regex(@"(\s+|[^\s]+)");

	void Start () {
		if (!Application.isPlaying)
			return;

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
			this.Story = this.GetComponent<Story>();
		if (this.Story == null)
		{
			Debug.LogError("Text player does not have a story to play. Add a story script to the text player game object, or assign the Story variable of the text player.");
			return;
		}

		this.Story.OnPassageEnter += Story_OnPassageEnter;
		this.Story.OnOutput += Story_OnOutput;
		this.Story.OnOutputRemoved += Story_OnOutputRemoved;

		if (StartStory)
			this.Story.Begin();
	}

	void OnDestroy()
	{
		if (!Application.isPlaying)
			return;

		if (this.Story != null)
		{
			this.Story.OnPassageEnter -= Story_OnPassageEnter;
			this.Story.OnOutput -= Story_OnOutput;
		}
	}

	// .....................
	// Clicks

	#if UNITY_EDITOR
	void Update()
	{
		if (Application.isPlaying)
			return;

		// In edit mode, disable autoplay on the story if the text player will be starting the story
		if (this.StartStory)
		{
			foreach (Story story in this.GetComponents<Story>())
				story.AutoPlay = false;
		}
	}
	#endif
	
	public void Clear()
	{
		for (int i = 0; i < Container.childCount; i++)
			GameObject.Destroy(Container.GetChild(i).gameObject);
		Container.DetachChildren();
	}

	void Story_OnPassageEnter(StoryPassage passage)
	{
		Clear();
	}

	void Story_OnOutput(StoryOutput output)
	{
		if (!this.AutoDisplay)
			return;

		DisplayOutput(output);
	}

	void Story_OnOutputRemoved(StoryOutput outputThatWasRemoved)
	{
		// Remove all elements related to this output
		foreach (var elem in Container.GetComponentsInChildren<TwineTextPlayerElement>()
			.Where(e => e.SourceOutput == outputThatWasRemoved))
		{
			elem.transform.SetParent(null);
			GameObject.Destroy(elem.gameObject);
		}
	}

	public void DisplayOutput(StoryOutput output)
	{
		// Deternine where to place this output in the hierarchy - right after the last UI element associated with the previous output, if exists
		TwineTextPlayerElement last = Container.GetComponentsInChildren<TwineTextPlayerElement>()
			.Where(elem => elem.SourceOutput.Index < output.Index)
			.OrderBy(elem => elem.SourceOutput.Index)
			.LastOrDefault();
		int uiInsertIndex = last == null ? -1 : last.transform.GetSiblingIndex() + 1;

		// Temporary hack to allow other scripts to change the templates based on the output's Style property
		SendMessage("Twine_BeforeDisplayOutput", output, SendMessageOptions.DontRequireReceiver);

		if (output is StoryText)
		{
			var text = (StoryText)output;
			if (!string.IsNullOrEmpty(text.Text))
			{
				foreach (Match m in rx_splitText.Matches(text.Text))
				{
					string word = m.Value;
					Text uiWord = (Text)Instantiate(WordTemplate);
					uiWord.gameObject.SetActive(true);
					uiWord.text = word;
					uiWord.name = word;
					AddToUI(uiWord.rectTransform, output, uiInsertIndex);
					if (uiInsertIndex >= 0)
						uiInsertIndex++;
				}
			}
		}
		else if (output is StoryLink)
		{
			var link = (StoryLink)output;
			if (!ShowNamedLinks && link.IsNamed)
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
			AddToUI((RectTransform)uiLink.transform, output, uiInsertIndex);
		}
		else if (output is LineBreak)
		{
			var br = (RectTransform)Instantiate(LineBreakTemplate);
			br.gameObject.SetActive(true);
			br.gameObject.name = "(br)";
			AddToUI(br, output, uiInsertIndex);
		}
		else if (output is OutputGroup)
		{
			// Add an empty indicator to later positioning
			var groupMarker = new GameObject();
			groupMarker.name = output.ToString();
			AddToUI(groupMarker.AddComponent<RectTransform>(), output, uiInsertIndex);
		}
	}

	void AddToUI(RectTransform rect, StoryOutput output, int index)
	{
		rect.SetParent(Container);
		if (index >= 0)
			rect.SetSiblingIndex(index);

		var elem = rect.gameObject.AddComponent<TwineTextPlayerElement>();
		elem.SourceOutput = output;
	}
}

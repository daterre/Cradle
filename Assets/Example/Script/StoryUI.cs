using UnityEngine;
using System.Collections;
using UnityTwine;
using UnityEngine.UI;

[RequireComponent(typeof(TwineStory))]
public class StoryUI : MonoBehaviour {

	public TwineStory Story { get; private set; }
	public RectTransform Container;
	public Button LinkTemplate;
	public Text TextTemplate;

	// Use this for initialization
	void Start () {
		LinkTemplate.gameObject.SetActive(false);
		TextTemplate.gameObject.SetActive(false);
		((RectTransform)LinkTemplate.transform).SetParent(null);
		TextTemplate.rectTransform.SetParent(null);

		this.Story = GetComponent<TwineStory>();
		this.Story.OnStateChanged += Story_OnStateChanged;
		this.Story.Begin();
	}

	void Story_OnStateChanged(TwineStoryState state)
	{
		if (state == TwineStoryState.Idle || state == TwineStoryState.Complete)
		{
			// Clear previous output
			for(int i = 0; i < Container.childCount; i++)
			{
				GameObject.Destroy(Container.GetChild(i).gameObject);
			}

			float y = 0f;
			foreach(TwineOutput output in Story.Output)
			{
				RectTransform child = null;
				if (output is TwineText)
				{
					var text = (TwineText)output;
					Text uiText = (Text)Instantiate(TextTemplate);
					uiText.gameObject.SetActive(true);
					uiText.text = text.String;
					child = uiText.rectTransform;
				}
				else if (output is TwineLink)
				{
					var link = (TwineLink)output;
					Button uiLink = (Button)Instantiate(LinkTemplate);
					uiLink.gameObject.SetActive(true);
					Text uiLinkText = uiLink.GetComponentInChildren<Text>();
					uiLinkText.text = link.Text;
					uiLink.onClick.AddListener(() => this.Story.Advance(link));
					child = (RectTransform)uiLink.transform;
				}
				else
					continue;

				
				child.SetParent(Container);
				child.anchoredPosition = new Vector2(0f, y);
				y -= 40f;
			}
		}
	}
	
}

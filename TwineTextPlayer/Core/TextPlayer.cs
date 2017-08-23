using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Cradle;
using Cradle.StoryFormats.Harlowe;
using System.Text;

namespace Cradle.Players.Text
{
	[ExecuteInEditMode]
	public class TextPlayer : MonoBehaviour
	{
		public Story Story;
		public TMPro.TextMeshProUGUI Container;
		public TextPlayerStyle Style;
		public bool StartStory = true;
		public bool ShowNamedLinks = true;

		class PosInfo
		{
			public StoryOutput output;
			public int index;
			public string length;
		}

		void Start()
		{
			if (!Application.isPlaying)
				return;
			if (this.Story == null)
				this.Story = this.GetComponent<Story>();

			if (this.Story == null)
			{
				Debug.LogError("Text player does not have a story to play. Add a story script to the text player game object, or assign the Story variable of the text player.");
				this.enabled = false;
				return;
			}

			if (this.Container == null)
				this.Container = GetComponent<TMPro.TextMeshProUGUI>();

			this.Story.OnStateChanged += Story_OnStateChanged;

			if (StartStory)
				this.Story.Begin();
		}

		void OnDestroy()
		{
			if (!Application.isPlaying)
				return;

			if (this.Story != null)
			{
				this.Story.OnStateChanged -= Story_OnStateChanged;
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
			Container.SetText(string.Empty);
		}

		void Story_OnStateChanged(StoryState state)
		{
			if (!state.In(StoryState.Idle, StoryState.Paused))
				return;

			RefreshText();
		}

		public void RefreshText()
		{
			StringBuilder _builder = new StringBuilder();

			for (int i = 0; i < Story.Output.Count; i++)
			{
				StoryOutput output = Story.Output[i];

				if (output is StoryText)
				{
					_builder.AppendFormat(this.Style.Text, output.Text);
				}
				else if (output is StoryLink)
				{
					var link = (StoryLink)output;
					_builder.AppendFormat(this.Style.Link, link.Name, link.Text);
				}
				else if (output is LineBreak)
				{
					_builder.Append(this.Style.LineBreak);
				}
				else if (output is OutputGroup)
			}
		}
	}
}
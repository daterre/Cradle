using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Cradle;
using Cradle.StoryFormats.Harlowe;
using System.Text;
using TMPro;

namespace Cradle.Players.TextMeshPro
{
	[ExecuteInEditMode]
	public class TMProCradlePlayer : MonoBehaviour
	{
		public Story Story;
		public TextMeshProUGUI TextUI;
		public TMProFormatSettings FormatSettings;
		public bool StartStory = true;
		public bool ShowNamedLinks = true;
		public bool CollapseLineBreaks = true;

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

			if (this.TextUI == null)
				this.TextUI = GetComponent<TMPro.TextMeshProUGUI>();
			if (this.TextUI == null)
			{
				Debug.LogError("Text player is missing a reference to TextUI.");
				this.enabled = false;
				return;
			}

			if (this.FormatSettings == null)
			{
				Debug.LogError("Text player is missing a reference to a format settings asset. Create one using the Asset Create menu.");
				this.enabled = false;
				return;
			}

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
			TextUI.SetText(string.Empty);
		}

		void Story_OnStateChanged(StoryState state)
		{
			if (!state.In(StoryState.Idle, StoryState.Paused))
				return;

			RefreshText();
		}

		public void RefreshText()
		{
			StringBuilder builder = new StringBuilder();
			Stack<StyleGroup> _styleGroups = new Stack<StyleGroup>();

			int lineBreaks = 0;

			for (int i = 0; i < Story.Output.Count; i++)
			{
				StoryOutput output = Story.Output[i];

				if (output is StoryText)
				{
					builder.AppendFormat(Unescape(this.FormatSettings.Text), output.Text);
				}
				else if (output is StoryLink)
				{
					var link = (StoryLink)output;
					if (this.ShowNamedLinks || !link.IsNamed)
						builder.AppendFormat(Unescape(this.FormatSettings.Link), TMProLinkHandler.Escape(link.Name), link.Text);
				}

				if (output is LineBreak)
				{
					if (!CollapseLineBreaks || lineBreaks < 2)
					{
						builder.Append(Unescape(this.FormatSettings.LineBreak));
						lineBreaks++;
					}
				}
				else
					lineBreaks = 0;

				if (output is StyleGroup)
				{
					var styleGroup = (StyleGroup)output;
					FormatStyle(styleGroup.Style, StyleFormatType.Prefix, builder);

					_styleGroups.Push(styleGroup);
				}
				else
				{
					while (_styleGroups.Count > 0 && output.StyleGroup != _styleGroups.Peek())
					{
						FormatStyle(_styleGroups.Pop().Style, StyleFormatType.Suffix, builder);
					}
				}
			}

			TextUI.SetText(builder);
		}

		void FormatStyle(Style style, StyleFormatType formatType, StringBuilder builder)
		{
			foreach(var entry in style)
			{
				StyleFormat styleFormat = FormatSettings.Styles
					.Where(f =>
						f.MatchingKeys.Contains(entry.Key) &&
						(
							string.IsNullOrEmpty(f.MatchingValuesRegex) ||
							Regex.IsMatch(System.Convert.ToString(entry.Value), f.MatchingValuesRegex)
						)
					)
					.FirstOrDefault();

				if (styleFormat == null)
					continue;

				string format = formatType == StyleFormatType.Prefix ?
					styleFormat.Prefix :
					styleFormat.Suffix;

				if (format == null)
					continue;

				builder.AppendFormat(Unescape(format), entry.Value);
			}
		}

		enum StyleFormatType
		{
			Prefix,
			Suffix
		}

		string Unescape(string format)
		{
			return Regex.Unescape(format);
		}

		public void DoLink(string linkName)
		{
			this.Story.DoLink(linkName);
		}
	}
}
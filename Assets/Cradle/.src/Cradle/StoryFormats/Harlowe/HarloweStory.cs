using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cradle;
using IStoryThread = System.Collections.Generic.IEnumerable<Cradle.StoryOutput>;

namespace Cradle.StoryFormats.Harlowe
{
	public abstract class HarloweStory: Story
	{
		public bool DebugMode = false;
		Stack<HarloweEnchantment> _activeEnchantments = new Stack<HarloweEnchantment>();

		public HarloweStory()
		{
			StoryVar.RegisterTypeService<string>(new HarloweStringService()); 
		}

		protected override Func<IStoryThread> GetPassageThread(StoryPassage passage)
		{
			return () => GetPassageThreadWithHeaderAndFooter(passage.MainThread, this.PassageHistory.Count == 1);
		}

		IStoryThread GetPassageThreadWithHeaderAndFooter(Func<IStoryThread> mainThread, bool startup)
		{
			if (startup)
				foreach (string headerPassage in GetPassagesWithTag("startup"))
					yield return passage(headerPassage);

			if (DebugMode)
			{
				if (startup)
					foreach (string headerPassage in GetPassagesWithTag("debug-startup"))
						yield return passage(headerPassage);

				foreach (string headerPassage in GetPassagesWithTag("debug-header"))
					yield return passage(headerPassage);
			}

			foreach (string headerPassage in GetPassagesWithTag("header"))
				yield return passage(headerPassage);

			yield return fragment(mainThread);

			foreach (string footerPassage in GetPassagesWithTag("footer"))
				yield return passage(footerPassage);

			if (DebugMode)
				foreach (string footerPassage in GetPassagesWithTag("debug-footer"))
					yield return passage(footerPassage);
		}

		/*
		protected HarloweHook hook(StoryVar hookName)
		{
			return new HarloweHook() { HookName = hookName };
		}
		*/

		protected StoryVar hookRef(StoryVar hookName)
		{
			return new HarloweHookRef(hookName);
		}

		protected EmbedFragment enchant(StoryVar reference, HarloweEnchantCommand command, Func<IStoryThread> fragment)
		{
            bool isHookRef = reference.Value is HarloweHookRef;
            string str = isHookRef ? ((HarloweHookRef)reference.Value).HookName : reference.ToString();
            List<HarloweEnchantment> enchantments = new List<HarloweEnchantment>();

			HarloweEnchantment lastHookEnchantment = null;

            for (int i = 0; i < this.Output.Count; i++)
            {
				StoryOutput output = this.Output[i];

                if (isHookRef)
                {
					// Check if matching hook found in the current group, otherwise skip
					if (!(output is OutputGroup))
						continue;

					var group = output as OutputGroup;
					if (group.Style.Get<string>(HarloweStyleSettings.Hook) != str)
						continue;

					// Matching hook was found, but enchantment metadata is not up to date
					if (lastHookEnchantment == null || lastHookEnchantment.HookGroup != group)
					{
						lastHookEnchantment = new HarloweEnchantment() {
							ReferenceType = HarloweEnchantReferenceType.Hook,
							Command = command,
							HookGroup = group,
							Affected = new List<StoryOutput>()
						};
						enchantments.Add(lastHookEnchantment);
					}

					// Add all outputs associated with this group
					i++;
					while (i < this.Output.Count && this.Output[i].BelongsToGroup(group))
					{
						lastHookEnchantment.Affected.Add(this.Output[i]);
						i++;
					}
                }
                else if (output is StoryText)
                {
                    var occurences = new Regex(Regex.Escape(str));
                    if (occurences.IsMatch(output.Text))
                        enchantments.Add(new HarloweEnchantment {
							ReferenceType = HarloweEnchantReferenceType.Text,
							Command = command,
							Affected = new List<StoryOutput>(){output},
							Text = str,
							Occurences = occurences
						});
                }
            }

			return new EmbedFragment(() => EnchantExecute(enchantments, fragment));
		}

		protected IStoryThread wrapFragmentWithHook(string hookName, Func<IStoryThread> fragment)
		{
			using (Group("hook", hookName))
				yield return this.fragment(fragment);
		}

		protected IStoryThread prefixFragmentWithLinkText(Func<IStoryThread> fragment)
		{
			yield return this.text(this.CurrentLinkInAction.Text);
			yield return this.fragment(fragment);
		}

		protected IStoryThread enchantHook(string hookName, HarloweEnchantCommand enchantCommand, Func<IStoryThread> fragment, bool linkTextPrefix = false)
		{
			// Special fragment features, if necessary
			Func<IStoryThread> f2 = !linkTextPrefix ? fragment : () => prefixFragmentWithLinkText(fragment);
			yield return enchant(hookRef(hookName), enchantCommand, f2);
		}

		protected EmbedFragment enchantIntoLink(StoryVar reference, Func<IStoryThread> linkAction)
		{
			return enchant(reference, HarloweEnchantCommand.Replace, () => EnchantIntoLink(linkAction));
		}

		IStoryThread EnchantExecute(IEnumerable<HarloweEnchantment> enchantments, Func<IStoryThread> fragment)
		{
			foreach(HarloweEnchantment enchantment in enchantments)
			{
				bool isHookRef = enchantment.ReferenceType == HarloweEnchantReferenceType.Hook;
				int index = isHookRef && enchantment.Command != HarloweEnchantCommand.Append ?
					enchantment.HookGroup.Index + 1 :
					-1;

				if (enchantment.Affected.Count > 0)
				{
					if (index == -1)
					{
						index =
							enchantment.Command == HarloweEnchantCommand.Append ? enchantment.Affected.Last().Index + 1 :
							enchantment.Affected[0].Index;
					}

					// Remove affected outputs to be replaced
					if (enchantment.Command == HarloweEnchantCommand.Replace)
					{
						for (int i = 0; i < enchantment.Affected.Count; i++)
							OutputRemove(enchantment.Affected[i]);
					}
				}

				this.InsertStack.Push(index);
				_activeEnchantments.Push(enchantment);

				GroupScope scope = isHookRef ? scope = Group(enchantment.HookGroup) : null;
				using (scope)
				{
					// Execute the enchantment thread
					yield return this.fragment(fragment);
				}

				_activeEnchantments.Pop();

				// Reset the index
				this.InsertStack.Pop();
			}
		}

		IStoryThread EnchantIntoLink(Func<IStoryThread> linkAction)
		{
			HarloweEnchantment enchantment = _activeEnchantments.Peek();
			foreach(StoryOutput affected in enchantment.Affected)
			{
				if (!(affected is StoryText))
					continue;
					//yield return affected;

				if (enchantment.ReferenceType == HarloweEnchantReferenceType.Text)
				{
					MatchCollection matches = enchantment.Occurences.Matches(affected.Text);
					int startCharIndex = 0;
					foreach (Match m in matches)
					{
						// Return text till here
						if (m.Index > startCharIndex)
							yield return new StoryText(affected.Text.Substring(startCharIndex, m.Index - startCharIndex));

						// Return text of this match
						yield return new StoryLink(m.Value, linkAction);
						startCharIndex = m.Index + m.Length;
					}

					// Return remaining text
					if (startCharIndex < affected.Text.Length)
						yield return new StoryText(affected.Text.Substring(startCharIndex));
				}
				else
				{
					// See commented out EnchantIntoLinkUndo
					//yield return new TwineLink(affected.Text, () => EnchantIntoLinkUndo(linkAction));
					yield return new StoryLink(affected.Text, linkAction);
				}
			}
		}

		// Need to implement this
		//IStoryThread EnchantIntoLinkUndo(Func<IStoryThread> linkAction)
		//{
		//	var enchant = this.CurrentLinkInAction.Style.GetValues<HarloweEnchantment>(HarloweStyleSettings.Enchantment).Last();
		//
		//  // Need to mark replaced positions somehow
		//	var undoEnchant = new HarloweEnchantment()
		//	{
		//		ReferenceType = HarloweEnchantReferenceType.Other,
		//		Command = HarloweEnchantCommand.Replace,
		//		Affected = new List<TwineOutput>()
		//	};

		//	yield return fragment(linkAction);
		//}
	}

	public class HarloweEnchantment
	{
		public HarloweEnchantReferenceType ReferenceType;
		public HarloweEnchantCommand Command;
		public List<StoryOutput> Affected;
		public OutputGroup HookGroup;
		public string Text;
		public Regex Occurences;

		public override string ToString()
		{
			return string.Format("{0}: {1}", Command, ReferenceType == HarloweEnchantReferenceType.Hook ? 
				string.Format("hook({0})", HookGroup.Style[HarloweStyleSettings.Hook]) :
				string.Format("\"{0}\"", this.Text)
			);
		}
	}

	public enum HarloweEnchantReferenceType
	{
		Text,
		Hook,
		Other
	}

	public enum HarloweEnchantCommand
	{
		None,
		Replace,
		Append,
		Prepend
	}

	public static class HarloweStyleSettings
	{
		public const string Hook = "hook";
		public const string EnchantCommand = "enchant-command";
		public const string EnchantSource = "enchant-source";
		public const string Enchantment = "enchantment";
		public const string LinkType = "link-type";
	}
}

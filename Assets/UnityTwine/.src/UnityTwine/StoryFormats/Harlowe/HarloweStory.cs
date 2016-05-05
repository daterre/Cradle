using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityTwine;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine.StoryFormats.Harlowe
{
	public abstract class HarloweStory: TwineStory
	{
		public HarloweStory()
		{
			TwineVar.RegisterTypeService<string>(new HarloweStringService()); 
		}

		protected HarloweHook hook(TwineVar hookName)
		{
			return new HarloweHook() { HookName = hookName };
		}

		protected TwineVar hookRef(TwineVar hookName)
		{
			return new HarloweHookRef(hookName);
		}

		protected TwineEmbedFragment replaceWithLink(TwineVar reference, Func<ITwineThread> linkAction)
		{
			return enchant(reference, HarloweEnchantCommand.Replace, () => EnchantToLink(linkAction));
		}

		protected TwineEmbedFragment enchant(TwineVar reference, HarloweEnchantCommand command, Func<ITwineThread> fragment)
		{
            bool isHookRef = reference.Value is HarloweHookRef;
            string str = isHookRef ? ((HarloweHookRef)reference.Value).HookName : reference.ToString();
            List<HarloweEnchantment> enchantments = new List<HarloweEnchantment>();

			HarloweEnchantment lastHookEnchantment = null;

            foreach(TwineOutput output in this.Output)
            {
                if (isHookRef)
                {
					HarloweHook hook = output.Style
						.GetValues<HarloweHook>(HarloweStyleSettings.Hook)
						.Where(h => h.HookName == str)
						.FirstOrDefault();
					
					// Check if matching hook found in the current context, otherwise skip
					if (hook == null)
						continue;

					// Matching hook was found, but enchantment metadata is not up to date
					if (lastHookEnchantment == null || lastHookEnchantment.Hook != hook)
					{
						lastHookEnchantment = new HarloweEnchantment() {
							ReferenceType = HarloweEnchantReferenceType.Hook,
							Command = command,
							Hook = hook,
							Affected = new List<TwineOutput>()
						};
						enchantments.Add(lastHookEnchantment);
					}

					lastHookEnchantment.Affected.Add(output);
                }
                else if (output is TwineText)
                {
                    var occurences = new Regex(Regex.Escape(str));
                    if (occurences.IsMatch(output.Text))
                        enchantments.Add(new HarloweEnchantment {
							ReferenceType = HarloweEnchantReferenceType.Text,
							Command = command,
							Affected = new List<TwineOutput>(){output},
							Occurences = occurences
						});
                }
            }

			return new TwineEmbedFragment(() => EnchantExecute(enchantments, fragment));
		}

		ITwineThread EnchantExecute(IEnumerable<HarloweEnchantment> enchantments, Func<ITwineThread> fragment)
		{
			foreach(HarloweEnchantment enchantment in enchantments)
			{
				using (ApplyStyle(HarloweStyleSettings.Enchantment, enchantment))
				{
					foreach (TwineOutput output in fragment.Invoke())
						yield return output;
				}
			}
		}

		ITwineThread EnchantToLink(Func<ITwineThread> linkAction)
		{
			var enchantment = this.Style.GetValues<HarloweEnchantment>(HarloweStyleSettings.Enchantment).Last();
			foreach(TwineOutput affected in enchantment.Affected)
			{
				if (!(affected is TwineText))
					continue;
					//yield return affected;

				using (ApplyStyle(affected.Style))
				{
					if (enchantment.ReferenceType == HarloweEnchantReferenceType.Text)
					{
						MatchCollection matches = enchantment.Occurences.Matches(affected.Text);
						int startCharIndex = 0;
						foreach (Match m in matches)
						{
							// Return text till here
							if (m.Index > startCharIndex)
								yield return new TwineText(affected.Text.Substring(startCharIndex, m.Index - startCharIndex));

							// Return text of this match
							yield return new TwineLink(m.Value, linkAction);
							startCharIndex = m.Index + m.Length;
						}

						// Return remaining text
						if (startCharIndex < affected.Text.Length - 1)
							yield return new TwineText(affected.Text.Substring(startCharIndex));
					}
					else
					{
						yield return new TwineLink(affected.Text, linkAction);
					}
				}
			}
		}
	}

	public class HarloweEnchantment
	{
		public HarloweEnchantReferenceType ReferenceType;
		public HarloweEnchantCommand Command;
		public List<TwineOutput> Affected;
		public HarloweHook Hook;
		public Regex Occurences;

		public override string ToString()
		{
			return string.Format("{0} {1} (affects {2})", Command, ReferenceType, Affected.Count);
		}
	}

	public enum HarloweEnchantReferenceType
	{
		Text,
		Hook
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

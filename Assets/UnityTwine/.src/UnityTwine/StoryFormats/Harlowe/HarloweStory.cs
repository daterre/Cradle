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

		protected TwineEmbedFragment replaceWithLink(TwineVar reference, Func<ITwineThread> linkAction, TwineContext contextInfo = null)
		{
			return enchant(reference, HarloweEnchantCommand.Replace, () => EnchantToLink(linkAction), contextInfo);
		}

		protected TwineEmbedFragment enchant(TwineVar reference, HarloweEnchantCommand command, Func<ITwineThread> fragment, TwineContext contextInfo = null)
		{
            bool isHookRef = reference.Value is HarloweHookRef;
            string str = isHookRef ? ((HarloweHookRef)reference.Value).HookName : reference.ToString();
            List<HarloweEnchantment> enchantments = new List<HarloweEnchantment>();

			HarloweEnchantment lastHookEnchantment = null;

            foreach(TwineOutput output in this.Output)
            {
                if (isHookRef)
                {
					HarloweHook hook = output.ContextInfo
						.GetValues<HarloweHook>(HarloweContext.Hook)
						.Where(h => h.HookName == str)
						.FirstOrDefault();
					
					// Check if matching hook found in the current context, otherwise skip
					if (hook == null)
					{
						// Nullify the last hook enchantment if it is present, because if it is we just exited it
						lastHookEnchantment = null;
						continue;
					}

					// Matching hook was found, but no enchantment created yet
					if (lastHookEnchantment == null)
					{
						lastHookEnchantment = new HarloweEnchantment() {
							EnchantType = HarloweEnchantType.Hook,
							EnchantCommand = command,
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
							EnchantType = HarloweEnchantType.Text,
							EnchantCommand = command,
							Affected = new List<TwineOutput>(){output},
							Occurences = occurences
						});
                }
            }

			return new TwineEmbedFragment(() => EnchantExecute(enchantments, fragment)) { ContextInfo = contextInfo };
		}

		ITwineThread EnchantExecute(IEnumerable<HarloweEnchantment> enchantments, Func<ITwineThread> fragment)
		{
			foreach(HarloweEnchantment enchantment in enchantments)
			{
				using (this.Context.Apply(HarloweContext.Enchantment, enchantment))
				{
					foreach (TwineOutput output in fragment.Invoke())
						yield return output;
				}
			}
		}

		ITwineThread EnchantToLink(Func<ITwineThread> linkAction)
		{
			var enchantment = this.Context.GetValues<HarloweEnchantment>(HarloweContext.Enchantment).Last();
			foreach(TwineOutput affected in enchantment.Affected)
			{
				if (!(affected is TwineText))
					continue;
					//yield return affected;

				using (Context.Apply(affected.ContextInfo))
				//using (Context.Apply(HarloweContext.EnchantSource, affected))
				{
					if (enchantment.EnchantType == HarloweEnchantType.Text)
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
		public HarloweEnchantType EnchantType;
		public HarloweEnchantCommand EnchantCommand;
		public List<TwineOutput> Affected;
		public Regex Occurences;

		public override string ToString()
		{
			return string.Format("{0} {1} (affects {2})", EnchantCommand, EnchantType, Affected.Count);
		}
	}

	public enum HarloweEnchantType
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityTwine;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweStory: TwineStory
	{
		public HarloweStory()
		{
			TwineVar.RegisterTypeService<string>(new HarloweStringService()); 
		}

		protected TwineVar hookRef(TwineVar hookName)
		{
			return new HarloweHookRef(hookName);
		}

        protected Enchantment enchant(TwineVar reference, Func<ITwineThread> action, TwineContext contextInfo = null)
		{
            bool isHookRef = reference.Value is HarloweHookRef;
            string str = isHookRef ? ((HarloweHookRef)reference.Value).HookName : reference.ToString();
            List<EnchantmentTarget> targets = new List<EnchantmentTarget>();

            foreach(TwineOutput output in this.Output)
            {
                EnchantmentTarget target = null;
                if (isHookRef)
                {
                    if (output.ContextInfo.Get<string>("hook") == str)
                        target = new EnchantmentTarget() { Output = output };
                }
                else if (output is TwineText)
                {
                    var occurences = new Regex(Regex.Escape(str));
                    if (occurences.IsMatch(output.Text))
                        target = new EnchantmentTarget { Output = output, Occurences = occurences };
                }
                if (target != null)
                    targets.Add(target);
            }

            using(contextInfo != null ? Context.Apply(contextInfo) : null)
                return new Enchantment(this, targets.ToArray(), action);
		}
	}
}

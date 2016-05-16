using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IStoryThread = System.Collections.Generic.IEnumerable<Cradle.StoryOutput>;
using System.Text.RegularExpressions;

namespace Cradle
{
    public class StoryLink: StoryOutput
    {
        public string PassageName;
		public Dictionary<string,object> Parameters;
		public Func<IStoryThread> Action;

		static Regex rx_Name = new Regex(@"^((?<name>[^=]+?)\s*=\s*)", RegexOptions.ExplicitCapture);

		public StoryLink(string text, string passageName, Func<IStoryThread> action)
        {
            this.PassageName = passageName;
			this.Action = action;

			Match m = rx_Name.Match(text);
			if (m.Success)
			{
				this.Name = m.Success ? m.Groups["name"].Value : text;
				this.Text = text.Substring(m.Index + m.Length);
			}
			else
				this.Text = text;
        }

		public StoryLink(string text, string passageName) :
			this( text, passageName, null)
		{
		}

		public StoryLink(string text, Func<IStoryThread> action) :
			this( text, null, action)
		{
		}

		public static bool operator==(StoryLink a, StoryLink b)
		{
			bool anull = object.ReferenceEquals(a,null);
			bool bnull = object.ReferenceEquals(b,null);

			if (anull && bnull)
				return true;
			if ((anull && !bnull) || (!anull && bnull))
				return false;

			return a.Text == b.Text && a.PassageName == b.PassageName && a.Action == b.Action;
		}

		public static bool operator!=(StoryLink a, StoryLink b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is StoryLink) {
				return ((StoryLink)obj) == this;
			}
			else
				return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return string.Format("[[{0}]]{1}{2}",
				this.Text,
				this.Action != null ? " --> (fragment)" : null,
				this.PassageName != null ? " --> " + PassageName : null
			);
		}
    }
}


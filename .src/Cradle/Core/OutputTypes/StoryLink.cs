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
		public bool IsNamed;

		static Regex rx_Name = new Regex(@"^((?<name>[^=]+?)\s*=\s*)", RegexOptions.ExplicitCapture);

		public StoryLink(string text, string passageName, Func<IStoryThread> action, bool useNameSyntax = true)
        {
            this.PassageName = passageName;
			this.Action = action;

			// If the name syntax extension is valid, use it
			if (useNameSyntax)
			{
				Match m = rx_Name.Match(text);
				if (m.Success)
				{
					this.Name = m.Success ? m.Groups["name"].Value : text;
					this.Text = text.Substring(m.Index + m.Length);
					this.IsNamed = true;
					return;
				}
			}
			
			// Name syntax didn't do anything so use the text as-is
			this.Name = text;
			this.Text = text;
			this.IsNamed = false;
        }

		public StoryLink(string text, string passageName, bool useNameSyntax = true) :
			this( text, passageName, null, useNameSyntax)
		{
		}

		public StoryLink(string text, Func<IStoryThread> action, bool useNameSyntax = true) :
			this(text, null, action, useNameSyntax)
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
			return string.Format("[[{0}{1}]]{2}{3}",
				this.Name != this.Text ? this.Name + " = " : null,
				this.Text,
				this.Action != null ? " --> (fragment)" : null,
				this.PassageName != null ? " --> " + PassageName : null
			);
		}
    }
}


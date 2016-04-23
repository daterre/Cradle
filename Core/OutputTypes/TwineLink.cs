using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine
{
    public class TwineLink: TwineOutput
    {
        public string PassageName;
		public Dictionary<string,object> Parameters;
		public Func<ITwineThread> Action;

		public TwineLink(string text, string passageName, Func<ITwineThread> action, IDictionary<string,object> parameters = null)
        {
            this.Text = text;
            this.PassageName = passageName;
			this.Action = action;
			this.Parameters = parameters == null ? null : new Dictionary<string, object>(parameters);
        }


		public TwineLink(string text, string passageName) :
			this( text, passageName, null,null)
		{
		}

		public TwineLink(string text, Func<ITwineThread> action) :
			this( text, null, action,null)
		{
		}

		public static bool operator==(TwineLink a, TwineLink b)
		{
			bool anull = object.ReferenceEquals(a,null);
			bool bnull = object.ReferenceEquals(b,null);

			if (anull && bnull)
				return true;
			if ((anull && !bnull) || (!anull && bnull))
				return false;

			return a.Text == b.Text && a.PassageName == b.PassageName && a.Action == b.Action;
		}

		public static bool operator!=(TwineLink a, TwineLink b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is TwineLink) {
				return ((TwineLink)obj) == this;
			}
			else
				return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}
    }
}


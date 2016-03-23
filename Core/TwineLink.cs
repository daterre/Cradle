using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityTwine
{
    public class TwineLink: TwineOutput
    {
        public string PassageName;
        public Action Action;

		[System.Obsolete]
		public TwineLink(string name, string text, string passageID, Action action, string unused):
			this(name,text, passageID, action)
		{
		}

		public TwineLink(string name, string text, string passageID, Action action)
        {
			this.Name = name;
            this.Text = text;
            this.PassageName = passageID;
			this.Action = action;
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


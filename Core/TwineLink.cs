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
		public Func<ITwineThread> Action;

		[System.Obsolete]
		public TwineLink(string name, string text, string passageID, Func<ITwineThread> action, string unused) :
			this(name,text, passageID, action)
		{
		}

		public TwineLink(string name, string text, string passageID, Func<ITwineThread> action)
        {
			this.Name = name;
            this.Text = text;
            this.PassageName = passageID;
			this.Action = action;
        }

		public TwineLink(string name, string text, string passageID):
			this(name, text, passageID, null)
		{
		}

		public TwineLink(string text, string passageID) :
			this(text, text, passageID, null)
		{
		}

		public TwineLink(string name, string text, Func<ITwineThread> action) :
			this (name, text, null, action)
		{
		}

		public TwineLink(string text, Func<ITwineThread> action) :
			this(text, text, null, action)
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


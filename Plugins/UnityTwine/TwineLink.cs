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
        public Action Setters;
		string _settersHash;

		public TwineLink(string name, string text, string passageID, Action setters, string settersHash)
        {
			this.Name = name;
            this.Text = text;
            this.PassageName = passageID;
            this.Setters = setters;
			_settersHash = settersHash;
        }

		public static bool operator==(TwineLink a, TwineLink b)
		{
			bool anull = object.ReferenceEquals(a,null);
			bool bnull = object.ReferenceEquals(b,null);

			if (anull && bnull)
				return true;
			if ((anull && !bnull) || (!anull && bnull))
				return false;

			return a.Text == b.Text && a.PassageName == b.PassageName && a._settersHash == b._settersHash;
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


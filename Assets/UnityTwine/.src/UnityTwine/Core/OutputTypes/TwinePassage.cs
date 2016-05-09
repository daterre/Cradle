using System;
using System.Collections;
using System.Collections.Generic;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine
{
    public class TwinePassage: TwineOutput
    {
        public string[] Tags;
		internal Func<ITwineThread> GetMainThread;

		public TwinePassage(string name, string[] tags, Func<ITwineThread> mainThread)
        {
            this.Name = name;
            this.Tags = tags;
            this.GetMainThread = mainThread;
        }

		public override string ToString()
		{
			return string.Format("{0} (passage)", this.Name);
		}
    }
}


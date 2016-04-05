using System;
using System.Collections;
using System.Collections.Generic;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine
{
    public class TwineNamedFragment: TwineOutput
    {
		internal Func<ITwineThread> GetThread;

		public TwineNamedFragment(string name, Func<ITwineThread> thread)
        {
            this.Name = name;
			this.GetThread = thread;
        }
    }
}


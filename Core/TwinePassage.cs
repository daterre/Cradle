using System;
using System.Collections;
using System.Collections.Generic;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

namespace UnityTwine
{
    public class TwinePassage: TwineOutput
    {
        public string[] Tags;
		internal Func<ITwineThread> Execute;

		public TwinePassage(string name, string[] tags, Func<ITwineThread> execute)
        {
            this.Name = name;
            this.Tags = tags;
            this.Execute = execute;
        }
    }
}


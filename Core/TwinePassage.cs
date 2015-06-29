using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine
{
    public class TwinePassage: TwineOutput
    {
        public string[] Tags;
		internal Func<IEnumerable<TwineOutput>> Execute;

		public TwinePassage(string name, string[] tags, Func<IEnumerable<TwineOutput>> execute)
        {
            this.Name = name;
            this.Tags = tags;
            this.Execute = execute;
        }
    }
}


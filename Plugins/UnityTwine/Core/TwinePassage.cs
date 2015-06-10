using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine
{
    public class TwinePassage: TwineOutput
    {
        public Dictionary<string,string> Tags;
		internal Func<IEnumerable<TwineOutput>> Execute;

		public TwinePassage(string name, Dictionary<string,string> tags, Func<IEnumerable<TwineOutput>> execute)
        {
            this.Name = name;
            this.Tags = tags;
            this.Execute = execute;
        }
    }
}


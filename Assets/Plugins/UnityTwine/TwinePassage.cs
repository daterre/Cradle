using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine
{
    public class TwinePassage: TwineOutput
    {
        public string ID;
        public Dictionary<string,string> Tags;
		internal Func<IEnumerable<TwineOutput>> Execute;

		public TwinePassage(string id, Dictionary<string,string> tags, Func<IEnumerable<TwineOutput>> execute)
        {
            this.ID = id;
            this.Tags = tags;
            this.Execute = execute;
        }
    }
}


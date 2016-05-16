using System;
using System.Collections;
using System.Collections.Generic;
using IStoryThread = System.Collections.Generic.IEnumerable<Cradle.StoryOutput>;

namespace Cradle
{
    public class StoryPassage: StoryOutput
    {
        public string[] Tags;
		internal Func<IStoryThread> GetMainThread;

		public StoryPassage(string name, string[] tags, Func<IStoryThread> mainThread)
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


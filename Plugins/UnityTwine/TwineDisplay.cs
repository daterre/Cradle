using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine
{
    public class TwineDisplay:TwineOutput
    {
        public string PassageName;
		public TwineVar[] Parameters;

        public TwineDisplay(string passageID, params TwineVar[] parameters)
        {
            this.PassageName = passageID;
			this.Parameters = parameters;
        }
    }
}
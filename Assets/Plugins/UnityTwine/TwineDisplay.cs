using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine
{
    public class TwineDisplay:TwineOutput
    {
        public string PassageID;
		public TwineVar[] Parameters;

        public TwineDisplay(string passageID, params TwineVar[] parameters)
        {
            this.PassageID = passageID;
			this.Parameters = parameters;
        }
    }
}
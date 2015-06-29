using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityTwine
{
    public class TwineDisplay:TwineOutput
    {
        public string PassageName;
		public TwineVar[] Parameters;

        public TwineDisplay(string passageName, params TwineVar[] parameters)
        {
            this.PassageName = passageName;
			this.Parameters = parameters;
        }
    }
}
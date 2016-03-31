using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UnityTwine
{
	public interface ITwineVarType
	{
		bool Contains(TwineVar val);

		TwineVar this[string propertyName]
		{
			get;
			set;
		}
	}

	public abstract class TwineVarTypeService
	{
		public abstract TwineVar GetProperty(object container, string propertyName);
		public abstract void SetProperty(object container, string propertyName, TwineVar value);
		public abstract bool Contains(object container, object containee);
	}
}

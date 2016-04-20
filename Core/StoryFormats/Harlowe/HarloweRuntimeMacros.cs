using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using UnityTwine;

namespace UnityTwine.StoryFormats.Harlowe
{
	[TwineMacroLibrary(typeof(HarloweStory))]
	public class HarloweRuntimeMacros: TwineRuntimeMacros
	{
		// ------------------------------------

		[TwineRuntimeMacro]
		public void click(TwineVar hookRef)
		{
			throw new System.NotImplementedException();
		}
		
		// ------------------------------------

		[TwineRuntimeMacro]
		public TwineVar a(params TwineVar[] vals)
		{
			return new HarloweArray(vals);
		}

		[TwineRuntimeMacro]
		public TwineVar count(TwineVar array, TwineVar item)
		{
			if (array.GetInnerType() == typeof(HarloweArray))
			{
				var harray = (HarloweArray)array.Value;
				return harray.Values.Where(elem => elem == item).Count();
			}
			else
				throw new TwineMacroException("count macro only supports arrays");
		}

		[TwineRuntimeMacro]
		public TwineVar range(int start, int end)
		{
			TwineVar[] values = new TwineVar[end - start + 1];
			for (int i = 0; i < values.Length; i++)
				values[i] = start + i;
			return new HarloweArray(values);
		}

		[TwineRuntimeMacro]
		public TwineVar rotated(int steps, params TwineVar[] vals)
		{
			//var array = new HarloweArray(vals);
			//List< values = new TwineVar[array.Values.Count];
			//Array.Copy(array.Values, array.Values, 
			//array.Values.sh
			throw new NotImplementedException();
		}

		[TwineRuntimeMacro]
		public TwineVar dataset(params TwineVar[] vals)
		{
			return new HarloweDataset(vals);
		}

		[TwineRuntimeMacro]
		public TwineVar datamap(params TwineVar[] vals)
		{
			return new HarloweDatamap(vals);
		}

		// ------------------------------------

		[TwineRuntimeMacro]
		public TwineVar ceil(double num)
		{
			return Mathf.CeilToInt((float)num);
		}

		[TwineRuntimeMacro]
		public TwineVar round(double num)
		{
			return Mathf.RoundToInt((float)num);
		}

		[TwineRuntimeMacro]
		public TwineVar round(double num, int precision)
		{
			return Math.Round(num, precision);
		}

		[TwineRuntimeMacro]
		public TwineVar either(params TwineVar[] vars)
		{
			return vars[UnityEngine.Random.Range(0, vars.Length)];
		}
	}
}


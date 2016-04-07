using UnityEngine;
using System.Collections;
using System.Linq;
using System;

namespace UnityTwine.StoryFormats.Harlowe
{
	public class HarloweRuntimeMacros: TwineRuntimeMacros
	{
		// ------------------------------------

		public void click(TwineVar hookRef)
		{
			throw new System.NotImplementedException();
		}
		
		// ------------------------------------

		public TwineVar a(params TwineVar[] vals)
		{
			return new HarloweArray(vals);
		}

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

		public TwineVar range(int start, int end)
		{
			TwineVar[] values = new TwineVar[end - start + 1];
			for (int i = 0; i < values.Length; i++)
				values[i] = start + i;
			return new HarloweArray(values);
		}

		public TwineVar rotated(int steps, params TwineVar[] vals)
		{
			//var array = new HarloweArray(vals);
			//List< values = new TwineVar[array.Values.Count];
			//Array.Copy(array.Values, array.Values, 
			//array.Values.sh
			throw new NotImplementedException();
		}


		public TwineVar dataset(params TwineVar[] vals)
		{
			return new HarloweDataset(vals);
		}

		public TwineVar datamap(params TwineVar[] vals)
		{
			return new HarloweDatamap(vals);
		}

		// ------------------------------------

		public TwineVar ceil(double num)
		{
			return Mathf.CeilToInt((float)num);
		}

		public TwineVar round(double num)
		{
			return Mathf.RoundToInt((float)num);
		}

		public TwineVar round(double num, int precision)
		{
			return Math.Round(num, precision);
		}

		public TwineVar either(params TwineVar[] vars)
		{
			return vars[UnityEngine.Random.Range(0, vars.Length)];
		}
	}
}


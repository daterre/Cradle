using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Cradle;
using System.Text;

namespace Cradle.StoryFormats.Harlowe
{
	[MacroLibrary(typeof(HarloweStory))]
	public class HarloweRuntimeMacros: RuntimeMacros
	{
		// ------------------------------------
        // Basic

        [RuntimeMacro]
        public StoryVar either(params StoryVar[] vals)
        {
            return vals[UnityEngine.Random.Range(0, vals.Length)];
        }
		
		// ------------------------------------
        // Data structures

        // ..........
        // Array

		[RuntimeMacro]
		public StoryVar a(params StoryVar[] vals)
		{
			return new HarloweArray(vals);
		}

		[RuntimeMacro]
		public StoryVar count(StoryVar array, StoryVar item)
		{
			return array.ConvertValueTo<HarloweArray>().Values.Where(elem => elem == item).Count();
		}

		[RuntimeMacro]
		public StoryVar range(int start, int end)
		{
			int temp = start;
			start = Math.Min(start, end);
			end = Math.Max(temp, end);

			StoryVar[] values = new StoryVar[end - start + 1];
			for (int i = 0; i < values.Length; i++)
				values[i] = start + i;
			return new HarloweArray(values);
		}

		[RuntimeMacro]
		public StoryVar rotated(int shift, params StoryVar[] vals)
		{
            var original = new HarloweArray(vals);
            var copy = new HarloweArray(original.Values);

            for (int i = 0; i < original.Length; i++)
            {
                int j = i + shift;
				if (j < 0)
					j += original.Length;
				else if (j > original.Length - 1)
					j -= original.Length;
                copy.Values[j] = original.Values[i];
            }

            return copy;
		}

        // Used by the shuffled macro
        System.Random shuffleRandomizer = new System.Random();

        [RuntimeMacro]
        public StoryVar shuffled(params StoryVar[] vals)
        {
            // http://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp
            var array = new HarloweArray(vals);
            int n = array.Length;  
            while (n > 1) {  
                n--;  
                int k = shuffleRandomizer.Next(n + 1);  
                StoryVar value = array.Values[k];  
                array.Values[k] = array.Values[n];  
                array.Values[n] = value;  
            } 

            return array;
        }

		 [RuntimeMacro]
        public StoryVar sorted(params string[] values)
        {
            return new HarloweArray(values
                .OrderBy(v => v, StringComparer.InvariantCulture)
                .Select(v => new StoryVar(v))
            );
        }

		 [RuntimeMacro]
		 public StoryVar subarray(StoryVar array, int from, int to)
		{
			return array[range(from, to)];
		}

        // ..........
        // Dataset

		[RuntimeMacro]
		public StoryVar dataset(params StoryVar[] vals)
		{
			return new HarloweDataset(vals);
		}

		[RuntimeMacro]
		public StoryVar ds(params StoryVar[] vals)
		{
			return dataset(vals);
		}

        // ..........
        // Datamap

		[RuntimeMacro]
		public StoryVar datamap(params StoryVar[] vals)
		{
			return new HarloweDatamap(vals);
		}

		[RuntimeMacro]
		public StoryVar dm(params StoryVar[] vals)
		{
			return datamap(vals);
		}

        [RuntimeMacro]
        public StoryVar datanames(StoryVar datamap)
        {
			return new HarloweArray(datamap.ConvertValueTo<HarloweDatamap>().Dictionary.Keys
                .OrderBy(key => key, StringComparer.InvariantCulture)
                .Select(key => new StoryVar(key))
            );
        }

        [RuntimeMacro]
		public StoryVar datavalues(StoryVar datamap)
        {
			return new HarloweArray(datamap.ConvertValueTo<HarloweDatamap>().Dictionary
                .OrderBy(pair => pair.Key, StringComparer.InvariantCulture)
                .Select(pair => pair.Value)
            );
        }

        // ------------------------------------
        // Date and time

        [RuntimeMacro]
        public StoryVar currentDate()
        {
            return DateTime.Today.ToShortDateString();
        }

        [RuntimeMacro]
        public StoryVar currentTime()
        {
            return DateTime.Now.ToShortTimeString();
        }

        [RuntimeMacro]
        public StoryVar monthday()
        {
            return DateTime.Today.Month;
        }

        [RuntimeMacro]
        public StoryVar weekday()
        {
            return DateTime.Today.DayOfWeek.ToString();
        }

        // ------------------------------------
        // Game state

		[RuntimeMacro]
        public StoryVar history()
        {
            return new HarloweArray(Story.PassageHistory.Select(passageName => new StoryVar(passageName)));
        }

		 [RuntimeMacro]
        public StoryVar passage(string passageName)
        {
            StoryPassage passage;
            if (!Story.Passages.TryGetValue(passageName, out passage))
                return default(StoryVar);
            else
                return new HarloweDatamap(
                    "source", "UnityTwine can't show the source of the passage.",
                    "name", passageName,
                    "tags", sorted(passage.Tags)
                );
        }

		// ------------------------------------
		// Math

		[RuntimeMacro]
		public StoryVar abs(double num)
		{
			return Math.Abs(num);
		}

		[RuntimeMacro]
		public StoryVar cos(double num)
		{
			return Math.Cos(num);
		}

		[RuntimeMacro]
		public StoryVar exp(double num)
		{
			return Math.Exp(num);
		}

		[RuntimeMacro]
		public StoryVar log(double num)
		{
			return Math.Log(num);
		}

		[RuntimeMacro]
		public StoryVar log10(double num)
		{
			return Math.Log10(num);
		}

		[RuntimeMacro]
		public StoryVar log2(double num)
		{
			return Math.Log(num, 2);
		}

		[RuntimeMacro]
		public StoryVar max(params StoryVar[] numbers)
		{
			double max = double.NaN;
			foreach (StoryVar num in HarloweSpread.Flatten(numbers))
				if (num > max)
					max = num;

			return max;
		}

		[RuntimeMacro]
		public StoryVar min(params StoryVar[] numbers)
		{
			double min = double.NaN;
			foreach (StoryVar num in HarloweSpread.Flatten(numbers))
				if (num < min)
					min = num;

			return min;
		}

		[RuntimeMacro]
		public StoryVar pow(double num, double power)
		{
			return Math.Pow(num, power);
		}

		[RuntimeMacro]
		public StoryVar sign(double num)
		{
			return Math.Sign(num);
		}

		[RuntimeMacro]
		public StoryVar sin(double num)
		{
			return Math.Sin(num);
		}

		[RuntimeMacro]
		public StoryVar sqrt(double num)
		{
			return Math.Sqrt(num);
		}

		[RuntimeMacro]
		public StoryVar tan(double num)
		{
			return Math.Tan(num);
		}

		// ------------------------------------
		// Number

		[RuntimeMacro]
		public StoryVar ceil(double num)
		{
			return Mathf.CeilToInt((float)num);
		}

		[RuntimeMacro]
		public StoryVar floor(double num)
		{
			return Mathf.FloorToInt((float)num);
		}

		[RuntimeMacro]
		public StoryVar number(StoryVar val)
		{
			return StoryVar.ConvertTo<double>(val, false);
		}

		[RuntimeMacro]
		public StoryVar num(StoryVar val)
		{
			return number(val);
		}

		[RuntimeMacro]
		public StoryVar random(double from, double to = 0)
		{
			int a = Mathf.CeilToInt((float)from);
			int b = Mathf.CeilToInt((float)to);
			return UnityEngine.Random.Range(a < b ? a : b, (b > a ? b : a) + 1);
		}

		[RuntimeMacro]
		public StoryVar round(double num)
		{
			return Mathf.RoundToInt((float)num);
		}

		[RuntimeMacro]
		public StoryVar round(double num, int precision)
		{
			return Math.Round(num, precision);
		}

		// ------------------------------------
		// String

		[RuntimeMacro]
		public StoryVar substring(string str, int from, int to)
		{
			return new StoryVar(str).GetMember(range(from, to));
		}

		[RuntimeMacro]
		public StoryVar text(params StoryVar[] vals)
		{
			var buffer = new StringBuilder();
			foreach (StoryVar val in HarloweSpread.Flatten(vals))
				buffer.Append(StoryVar.ConvertTo<string>(val, false));
			return buffer.ToString();
		}

		[RuntimeMacro]
		public StoryVar @string(params StoryVar[] vals)
		{
			return text(vals);
		}
	}
}


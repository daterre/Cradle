using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityTwine;
using System.Text;

namespace UnityTwine.StoryFormats.Harlowe
{
	[TwineMacroLibrary(typeof(HarloweStory))]
	public class HarloweRuntimeMacros: TwineRuntimeMacros
	{
		// ------------------------------------
        // Basic

        [TwineRuntimeMacro]
        public TwineVar either(params TwineVar[] vals)
        {
            return vals[UnityEngine.Random.Range(0, vals.Length)];
        }
		
		// ------------------------------------
        // Data structures

        // ..........
        // Array

		[TwineRuntimeMacro]
		public TwineVar a(params TwineVar[] vals)
		{
			return new HarloweArray(vals);
		}

		[TwineRuntimeMacro]
		public TwineVar count(TwineVar array, TwineVar item)
		{
			return array.ConvertValueTo<HarloweArray>().Values.Where(elem => elem == item).Count();
		}

		[TwineRuntimeMacro]
		public TwineVar range(int start, int end)
		{
			int temp = start;
			start = Math.Min(start, end);
			end = Math.Max(temp, end);

			TwineVar[] values = new TwineVar[end - start + 1];
			for (int i = 0; i < values.Length; i++)
				values[i] = start + i;
			return new HarloweArray(values);
		}

		[TwineRuntimeMacro]
		public TwineVar rotated(int shift, params TwineVar[] vals)
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

        [TwineRuntimeMacro]
        public TwineVar shuffled(params TwineVar[] vals)
        {
            // http://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp
            var array = new HarloweArray(vals);
            int n = array.Length;  
            while (n > 1) {  
                n--;  
                int k = shuffleRandomizer.Next(n + 1);  
                TwineVar value = array.Values[k];  
                array.Values[k] = array.Values[n];  
                array.Values[n] = value;  
            } 

            return array;
        }

		 [TwineRuntimeMacro]
        public TwineVar sorted(params string[] values)
        {
            return new HarloweArray(values
                .OrderBy(v => v, StringComparer.InvariantCulture)
                .Select(v => new TwineVar(v))
            );
        }

		 [TwineRuntimeMacro]
		 public TwineVar subarray(TwineVar array, int from, int to)
		{
			return array[range(from, to)];
		}

        // ..........
        // Dataset

		[TwineRuntimeMacro]
		public TwineVar dataset(params TwineVar[] vals)
		{
			return new HarloweDataset(vals);
		}

		[TwineRuntimeMacro]
		public TwineVar ds(params TwineVar[] vals)
		{
			return dataset(vals);
		}

        // ..........
        // Datamap

		[TwineRuntimeMacro]
		public TwineVar datamap(params TwineVar[] vals)
		{
			return new HarloweDatamap(vals);
		}

		[TwineRuntimeMacro]
		public TwineVar dm(params TwineVar[] vals)
		{
			return datamap(vals);
		}

        [TwineRuntimeMacro]
        public TwineVar datanames(TwineVar datamap)
        {
			return new HarloweArray(datamap.ConvertValueTo<HarloweDatamap>().Dictionary.Keys
                .OrderBy(key => key, StringComparer.InvariantCulture)
                .Select(key => new TwineVar(key))
            );
        }

        [TwineRuntimeMacro]
		public TwineVar datavalues(TwineVar datamap)
        {
			return new HarloweArray(datamap.ConvertValueTo<HarloweDatamap>().Dictionary
                .OrderBy(pair => pair.Key, StringComparer.InvariantCulture)
                .Select(pair => pair.Value)
            );
        }

        // ------------------------------------
        // Date and time

        [TwineRuntimeMacro]
        public TwineVar currentDate()
        {
            return DateTime.Today.ToShortDateString();
        }

        [TwineRuntimeMacro]
        public TwineVar currentTime()
        {
            return DateTime.Now.ToShortTimeString();
        }

        [TwineRuntimeMacro]
        public TwineVar monthday()
        {
            return DateTime.Today.Month;
        }

        [TwineRuntimeMacro]
        public TwineVar weekday()
        {
            return DateTime.Today.DayOfWeek.ToString();
        }

        // ------------------------------------
        // Game state

        public TwineVar history()
        {
            return new HarloweArray(Story.PassageHistory.Select(passageName => new TwineVar(passageName)));
        }

        public TwineVar passage(string passageName)
        {
            TwinePassage passage;
            if (!Story.Passages.TryGetValue(passageName, out passage))
                return default(TwineVar);
            else
                return new HarloweDatamap(
                    "source", "UnityTwine can't show the source of the passage.",
                    "name", passageName,
                    "tags", sorted(passage.Tags)
                );
        }

		// ------------------------------------
		// Math

		[TwineRuntimeMacro]
		public TwineVar abs(double num)
		{
			return Math.Abs(num);
		}

		[TwineRuntimeMacro]
		public TwineVar cos(double num)
		{
			return Math.Cos(num);
		}

		[TwineRuntimeMacro]
		public TwineVar exp(double num)
		{
			return Math.Exp(num);
		}

		[TwineRuntimeMacro]
		public TwineVar log(double num)
		{
			return Math.Log(num);
		}

		[TwineRuntimeMacro]
		public TwineVar log10(double num)
		{
			return Math.Log10(num);
		}

		[TwineRuntimeMacro]
		public TwineVar log2(double num)
		{
			return Math.Log(num, 2);
		}

		[TwineRuntimeMacro]
		public TwineVar max(params TwineVar[] numbers)
		{
			double max = double.NaN;
			foreach (TwineVar num in HarloweSpread.Flatten(numbers))
				if (num > max)
					max = num;

			return max;
		}

		[TwineRuntimeMacro]
		public TwineVar min(params TwineVar[] numbers)
		{
			double min = double.NaN;
			foreach (TwineVar num in HarloweSpread.Flatten(numbers))
				if (num < min)
					min = num;

			return min;
		}

		[TwineRuntimeMacro]
		public TwineVar pow(double num, double power)
		{
			return Math.Pow(num, power);
		}

		[TwineRuntimeMacro]
		public TwineVar sign(double num)
		{
			return Math.Sign(num);
		}

		[TwineRuntimeMacro]
		public TwineVar sin(double num)
		{
			return Math.Sin(num);
		}

		[TwineRuntimeMacro]
		public TwineVar sqrt(double num)
		{
			return Math.Sqrt(num);
		}

		[TwineRuntimeMacro]
		public TwineVar tan(double num)
		{
			return Math.Tan(num);
		}

		// ------------------------------------
		// Number

		[TwineRuntimeMacro]
		public TwineVar ceil(double num)
		{
			return Mathf.CeilToInt((float)num);
		}

		[TwineRuntimeMacro]
		public TwineVar floor(double num)
		{
			return Mathf.FloorToInt((float)num);
		}

		[TwineRuntimeMacro]
		public TwineVar number(TwineVar val)
		{
			return TwineVar.ConvertTo<double>(val, false);
		}

		[TwineRuntimeMacro]
		public TwineVar num(TwineVar val)
		{
			return number(val);
		}

		[TwineRuntimeMacro]
		public TwineVar random(double from, double to = 0)
		{
			int a = Mathf.CeilToInt((float)from);
			int b = Mathf.CeilToInt((float)to);
			return UnityEngine.Random.Range(a < b ? a : b, (b > a ? b : a) + 1);
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

		// ------------------------------------
		// String

		[TwineRuntimeMacro]
		public TwineVar substring(string str, int from, int to)
		{
			return new TwineVar(str).GetMember(range(from, to));
		}

		[TwineRuntimeMacro]
		public TwineVar text(params TwineVar[] vals)
		{
			var buffer = new StringBuilder();
			foreach (TwineVar val in HarloweSpread.Flatten(vals))
				buffer.Append(TwineVar.ConvertTo<string>(val, false));
			return buffer.ToString();
		}

		[TwineRuntimeMacro]
		public TwineVar @string(params TwineVar[] vals)
		{
			return text(vals);
		}
	}
}


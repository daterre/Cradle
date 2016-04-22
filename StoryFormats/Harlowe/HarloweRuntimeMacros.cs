using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityTwine;

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

		[TwineRuntimeMacro]
		public void click(TwineVar hookRef)
		{
			throw new System.NotImplementedException();
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
		public TwineVar count(HarloweArray array, TwineVar item)
		{
			return array.Values.Where(elem => elem == item).Count();
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
		public TwineVar rotated(int shift, params TwineVar[] vals)
		{
            var original = new HarloweArray(vals);
            var copy = new HarloweArray(original.Values);

            for (int i = 0; i < original.Length; i++)
            {
                int j = i + shift;
                if (j < 0)
                    j = original.Length - j;
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

        public TwineVar sorted(params string[] values)
        {
            return new HarloweArray(values
                .OrderBy(v => v, StringComparer.InvariantCulture)
                .Select(v => new TwineVar(v))
            );
        }

        // ..........
        // Dataset

		[TwineRuntimeMacro]
		public TwineVar dataset(params TwineVar[] vals)
		{
			return new HarloweDataset(vals);
		}

        // ..........
        // Datamap

		[TwineRuntimeMacro]
		public TwineVar datamap(params TwineVar[] vals)
		{
			return new HarloweDatamap(vals);
		}

        [TwineRuntimeMacro]
        public TwineVar datanames(HarloweDatamap datamap)
        {
            return new HarloweArray(datamap.Dictionary.Keys
                .OrderBy(key => key, StringComparer.InvariantCulture)
                .Select(key => new TwineVar(key))
            );
        }

        [TwineRuntimeMacro]
        public TwineVar datavalues(HarloweDatamap datamap)
        {
            return new HarloweArray(datamap.Dictionary.Keys
                .OrderBy(key => key, StringComparer.InvariantCulture)
                .Select(key => datamap.Dictionary[key])
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
                    "source", default(TwineVar),
                    "name", passageName,
                    "tags", sorted(passage.Tags)
                );
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
	}
}


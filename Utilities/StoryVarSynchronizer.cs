using System.Collections.Generic;
using UnityEngine;
using StoryVarSet = System.Collections.Generic.Dictionary<string, Cradle.StoryVar>;

namespace Cradle.Utilities
{
	public class StoryVarSynchronizer: MonoBehaviour
	{
		[Tooltip("The stories to synchronize. The first story gets the highest priority in case of a conflict.")]
		public Story[] Stories;

		Dictionary<Story, StoryVarSet> _previousValues = new Dictionary<Story, StoryVarSet>();
		StoryVarSet _changes = new StoryVarSet();

		void Update()
		{
			// Clean all previous recorded changes
			_changes.Clear();

			// Go over every story and check for changed variables
			for (int i = 0; i < Stories.Length; i++)
			{
				Story story = Stories[i];
				StoryVarSet previousValues;

				// If no previous values were stored for this story, create a new set to store them in
				if (!_previousValues.TryGetValue(story, out previousValues))
					_previousValues.Add(story, previousValues = new StoryVarSet());

				// Go over every variable and compare with its previous value
				foreach (var variable in story.Vars)
				{
					// Get current and previous values for this story
					StoryVar currentValue = variable.Value;
					StoryVar previousValue = default(StoryVar);

					// Compare to previous
					if (previousValues.TryGetValue(variable.Key, out previousValue))
					{
						// A change has been detected in this variable, and no other story has changed it in this frame
						if (previousValue != currentValue && !_changes.ContainsKey(variable.Key))
							_changes.Add(variable.Key, currentValue);
					}

					// Keep track of the previous value
					previousValues[variable.Key] = currentValue;
				}
			}

			// Go over every story again, this time applying the changed variables
			for (int i = 0; i < Stories.Length; i++)
			{
				Story story = Stories[i];
				foreach (var variable in _changes)
				{
					if (story.Vars.ContainsKey(variable.Key))
						story.Vars[variable.Key] = variable.Value;
				}
			}
		}
	}
}
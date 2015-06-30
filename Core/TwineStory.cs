using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace UnityTwine
{
    public abstract class TwineStory: MonoBehaviour
    {
		public bool AutoPlay = true;
        public string StartPassage = "Start";
		public GameObject[] AdditionalHooks;

		public event Action<TwineStoryState> OnStateChanged;
		public event Action<TwineOutput> OnOutput;
		
		public List<TwineText> Text { get; private set; }
		public List<TwineLink> Links { get; private set; }
		public List<TwineOutput> Output { get; private set; }
		public TwineVar[] PassageParameters { get; private set; }
		public string[] Tags { get; private set; }
		public string CurrentPassageName { get; private set; }
		public string PreviousPassageName { get; private set; }

		protected Dictionary<string, TwinePassage> Passages { get; private set; }
		TwineStoryState _state = TwineStoryState.Idle;
		IEnumerator<TwineOutput> _passageExecutor = null;
		Hook[] _passageUpdateHooks = null;
		string _passageWaitingToEnter = null;
		Dictionary<string, List<Hook>> _hookCache = new Dictionary<string, List<Hook>>();
		MonoBehaviour[] _hookTargets = null;

		int _turns;
		Dictionary<string, int> _visitedCountPassages = new Dictionary<string,int>();
		Dictionary<string, int> _visitedCountTags = new Dictionary<string,int>();

		private class Hook
		{
			public MonoBehaviour target;
			public MethodInfo method;
		}

		public TwineStory()
		{
			this.Passages = new Dictionary<string, TwinePassage>();
		}

		protected void Init()
		{
			_state = TwineStoryState.Idle;
			this.Output = new List<TwineOutput>();
			this.Text = new List<TwineText>();
			this.Links = new List<TwineLink>();
			this.Tags = new string[0];
			this.PassageParameters = null;

			_turns = 0;
			_visitedCountPassages.Clear();
			_visitedCountTags.Clear();
			
			PreviousPassageName = null;
			CurrentPassageName = null;
		}

		void Start()
		{
			if (AutoPlay)
				this.Begin();
		}

		// ---------------------------------
		// State control

		public TwineStoryState State
		{
			get { return _state; }
			private set
			{
				TwineStoryState prev = _state;
				_state = value;
				if (prev != value && OnStateChanged != null)
					OnStateChanged(value);
			}
		}

		public void Reset()
		{
			if (this.State != TwineStoryState.Idle && this.State != TwineStoryState.Complete)
				throw new InvalidOperationException("Can only reset a story that is Idle or Complete.");

			// Reset twine vars
			// TODO: don't use reflection, dummy. We need to code generate a dictionary of variables and use that
			FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
			for (int i = 0; i < fields.Length; i++)
				if (fields[i].FieldType == typeof(TwineVar))
					fields[i].SetValue(this, default(TwineVar));

			this.Init();
		}

		public void Begin()
		{
			GoTo(StartPassage);
		}

		public void GoTo(string passageName)
		{
			if (this.State != TwineStoryState.Idle)
			{
				throw new InvalidOperationException(
					// Paused
					this.State == TwineStoryState.Paused ?
						"The story is currently paused. Resume() must be called before advancing to a different passage." :
					// Playing
					this.State == TwineStoryState.Playing || this.State == TwineStoryState.Exiting ?
						"The story can only be advanced when it is in the Idle state." :
					// Complete
						"The story is complete. Reset() must be called before it can be played again."
					);
			}
			
			// Indicate specified passage as next
			_passageWaitingToEnter = passageName;

			if (CurrentPassageName != null)
			{
				this.State = TwineStoryState.Exiting;

				// invoke exit hooks
				HooksInvoke(HooksFind("Exit", reverse: true));
			}

			if (this.State != TwineStoryState.Paused)
				Enter(passageName);
		}

		void Enter(string passageName)
		{
			_passageWaitingToEnter = null;

			this.Output.Clear();
			this.Text.Clear();
			this.Links.Clear();
			this.PassageParameters = null;
			_passageUpdateHooks = null;

			TwinePassage passage = GetPassage(passageName);
			this.Tags = (string[]) passage.Tags.Clone();
			this.PreviousPassageName = this.CurrentPassageName;
			this.CurrentPassageName = passage.Name;
			
			// Update visited counters for passages and tags
			int visitedPassage;
			if (!_visitedCountPassages.TryGetValue(passageName, out visitedPassage))
				visitedPassage = 0;
			_visitedCountPassages[passageName] = visitedPassage+1;

			for (int i = 0; i < passage.Tags.Length; i++)
			{
				string tag = passage.Tags[i];
				int visitedTag;
				if (!_visitedCountTags.TryGetValue(tag, out visitedTag))
					visitedTag = 0;
				_visitedCountTags[tag] = visitedTag+1;
			}

			// Add output (and trigger hooks)
			this.Output.Add(passage);

			// Prepare the enumerator
			_passageExecutor = ExecutePassage(passage).GetEnumerator();
			
			// Get update hooks for calling during update
			_passageUpdateHooks = HooksFind("Update", reverse: false, allowCoroutines: false).ToArray();

			this.State = TwineStoryState.Playing;
			SendOutput(passage);
			HooksInvoke(HooksFind("Enter", maxLevels: 1));

			// Story was paused, wait for it to resume
			if (this.State == TwineStoryState.Paused)
				return;
			else
				Execute();
		}

		/// <summary>
		/// While the story is playing, pauses the execution of the current passage.
		/// </summary>
		public void Pause()
		{
			if (this.State != TwineStoryState.Playing && this.State != TwineStoryState.Exiting)
				throw new InvalidOperationException("Pause can only be called while a passage is playing or exiting.");

			this.State = TwineStoryState.Paused;
		}

		public void Resume()
		{
			if (this.State != TwineStoryState.Paused)
			{
				throw new InvalidOperationException(
					// Paused
					this.State == TwineStoryState.Idle ?
						"The story is currently idle. Call Begin, Advance or GoTo to play." :
					// Playing
					this.State == TwineStoryState.Playing || this.State == TwineStoryState.Exiting?
						"Resume() should be called only when the story is paused." :
					// Complete
						"The story is complete. Reset() must be called before it can be played again."
					);
			}
						
			// Either enter the next passage, or Execute if it was already entered
			if (_passageWaitingToEnter != null) {
				Enter(_passageWaitingToEnter);
			}
			else {
				this.State = TwineStoryState.Playing;
				Execute();
			}
		}

		/// <summary>
		/// Resumes a story that was paused.
		/// </summary>
		void Execute()
		{
			while (_passageExecutor.MoveNext())
			{
				TwineOutput output = _passageExecutor.Current;
				this.Output.Add(output);

				if (output is TwineLink)
				{
					// Add links to dedicated list
					var link = (TwineLink)output;
					this.Links.Add(link);
				}
				else if (output is TwineText)
				{
					// Add all text to the Text property for easy access
					var text = (TwineText)output;
					this.Text.Add(text);
				}

				// Let the handlers and hooks kick in
				if (output is TwinePassage)
				{
					HooksInvoke(HooksFind("Enter", reverse: true, maxLevels: 1));

					// Refresh the update hooks
					_passageUpdateHooks = HooksFind("Update", reverse: false, allowCoroutines: false).ToArray();
				}
				else
				{
					SendOutput(output);
					HooksInvoke(HooksFind("Output"), output);
				}

				// Story was paused, wait for it to resume
				if (this.State == TwineStoryState.Paused)
					return;
			}

			_passageExecutor.Dispose();
			_passageExecutor = null;

			this.State = this.Links.Count > 0 ?
				TwineStoryState.Idle :
				TwineStoryState.Complete;

			HooksInvoke(HooksFind("Done"));
		}

		TwinePassage GetPassage(string passageName)
		{
			string pid = passageName;
			TwinePassage passage;
			if (!Passages.TryGetValue(pid, out passage))
				throw new Exception(String.Format("Passage '{0}' does not exist.", pid));
			return passage;
		}
			
		IEnumerable<TwineOutput> ExecutePassage(TwinePassage passage)
        {
			foreach(TwineOutput output in passage.Execute()) {
                if (output is TwineDisplay) {
                    var display = (TwineDisplay) output;
					var displayParams = (TwineVar[])display.Parameters.Clone();
					this.PassageParameters = displayParams;
					
					TwinePassage displayPassage = GetPassage(display.PassageName);
					yield return displayPassage;
					foreach (TwineOutput innerOutput in ExecutePassage(displayPassage))
					{
						yield return innerOutput;
						this.PassageParameters = displayParams; // do this again because inner display macros can override this
					}
					PassageParameters = null;
                }
                else
                    yield return output;
            }
        }

		void SendOutput(TwineOutput output)
		{
			if (OnOutput != null)
				OnOutput(output);

		}
		
		// ---------------------------------
		// Links

		public void Advance(TwineLink link)
		{
			if (link.Setters != null)
				link.Setters.Invoke();

			_turns++;

			GoTo(link.PassageName);
		}

		public void Advance(int linkIndex)
		{
			Advance(this.Links[linkIndex]);
		}
		
		public void Advance(string linkName)
		{
			TwineLink link = this.Links
				.Where(lnk => string.Equals(lnk.Name, linkName, System.StringComparison.OrdinalIgnoreCase))
				.FirstOrDefault();

			if (link == null)
				throw new KeyNotFoundException(string.Format("There is no available link with the name '{0}'.", linkName));

			Advance(link);
		}

		public bool HasLink(string linkName)
		{
			TwineLink link = this.Links
				.Where(lnk => string.Equals(lnk.Name, linkName, System.StringComparison.OrdinalIgnoreCase))
				.FirstOrDefault();

			return link != null;
		}


		// ---------------------------------
		// Hooks

		static Regex _validPassageNameRegex = new Regex("^[a-z_][a-z0-9]*$", RegexOptions.IgnoreCase);

		void Update()
		{
			HooksInvoke(_passageUpdateHooks);
		}

		void HooksInvoke(IEnumerable<Hook> hooks, params object[] args)
		{
			if (hooks == null)
				return;

			if (hooks is Hook[])
			{
				var ar = (Hook[]) hooks;
				for (int i = 0; i < ar.Length; i++)
					HookInvoke(ar[i], args);
			}
			else
			{
				foreach (Hook hook in hooks)
					HookInvoke(hook, args);
			}
		}

		IEnumerable<Hook> HooksFind(string hookName, int maxLevels = 0, bool reverse = false, bool allowCoroutines = true)
		{
			int c = 0;
			for(
				int i = reverse ? this.Output.Count - 1 : 0;
				reverse ? i >= 0 : i < this.Output.Count;
				c++, i = i + (reverse ? -1 : 1)
				)
			{
				if (!(this.Output[i] is TwinePassage))
					continue;

				var passage = (TwinePassage)this.Output[i];
				
				// Ensure hookable passage
				if (!_validPassageNameRegex.IsMatch(passage.Name))
				{
					//Debug.LogWarning(string.Format("Passage \"{0}\" is not hookable because it does not follow C# variable naming rules.", passage.Name));
					continue;
				}

				List<Hook> hooks = HookGetMethods(passage.Name + '_' + hookName, allowCoroutines);
				if (hooks != null)
				{
					for (int h = 0; h < hooks.Count; h++)
						yield return hooks[h];
					c++;
					if (maxLevels > 0 && c == maxLevels)
						yield break;
				}
			}
		}

		void HookInvoke(Hook hook, object[] args)
		{
			var result = hook.method.Invoke(hook.target, args);
			if (result is IEnumerator)
				StartCoroutine(((IEnumerator)result));
		}

		MonoBehaviour[] HookGetTargets()
		{
			if (_hookTargets == null)
			{
				// ...................
				// Get all hook targets
				GameObject[] hookTargets;
				if (this.AdditionalHooks != null)
				{
					hookTargets = new GameObject[this.AdditionalHooks.Length + 1];
					hookTargets[0] = this.gameObject;
					this.AdditionalHooks.CopyTo(hookTargets, 1);
				}
				else
					hookTargets = new GameObject[] { this.gameObject };

				// Get all types
				var sources = new List<MonoBehaviour>();
				for (int i = 0; i < hookTargets.Length; i++)
					sources.AddRange(hookTargets[i].GetComponents<MonoBehaviour>());

				_hookTargets = sources.ToArray();
			}

			return _hookTargets;
		}

		List<Hook> HookGetMethods(string methodName, bool allowCoroutines = true)
		{
			List<Hook> hooks = null;
			if (!_hookCache.TryGetValue(methodName, out hooks))
			{
				MonoBehaviour[] targets = HookGetTargets();
				for (int i = 0; i < targets.Length; i++)
				{
					Type targetType = targets[i].GetType();
					MethodInfo method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

					// No method found on this source type
					if (method == null)
						continue;
					
					// Validate the found method
					if (allowCoroutines)
					{
						if (method.ReturnType != typeof(void) && !typeof(IEnumerator).IsAssignableFrom(method.ReturnType))
						{
							Debug.LogError(targetType.Name + "." + methodName + " must return void or IEnumerator in order to hook this story event.");
							method = null;
						}
					}
					else
					{
						if (method.ReturnType != typeof(void))
						{
							Debug.LogError(targetType.Name + "." + methodName + " must return void in order to hook to this story event.");
							method = null;
						}
					}

					// The found method wasn't valid
					if (method == null)
						continue;

					// Init the method list
					if (hooks == null)
						hooks = new List<Hook>();

					hooks.Add(new Hook() { method = method, target = targets[i] } );
				}

				// Cache the method list even if it's null so we don't do another lookup next time around (lazy load)
				_hookCache.Add(methodName, hooks);
			}

			return hooks;
		}

		// ---------------------------------
		// Variables

		public abstract TwineVar this[string name]
		{
			get;
			set;
		}

		// ---------------------------------
		// Functions

		protected TwineVar either(params TwineVar[] vars)
		{
			return vars[UnityEngine.Random.Range(0, vars.Length)];
		}

		protected int random(int min, int max)
		{
			return UnityEngine.Random.Range(min, max + 1);
		}

		protected string passage()
		{
			return this.CurrentPassageName;
		}

		protected string previous()
		{
			return this.PreviousPassageName;
		}

		protected TwineVar visited(params string[] passageNames)
		{
			if (passageNames == null || passageNames.Length == 0)
				passageNames = new string[] { this.CurrentPassageName };

			int min = int.MaxValue;
			for(int i = 0; i < passageNames.Length; i++)
			{
				string passage = passageNames[i];
				int count;
				if (!_visitedCountPassages.TryGetValue(passage, out count))
					count = 0;

				if (count < min)
					min = count;
			}

			if (min == int.MaxValue)
				min = 0;

			return min;
		}

		protected TwineVar visitedTag(params string[] tags)
		{
			if (tags == null || tags.Length == 0)
				return 0;

			int min = int.MaxValue;
			for (int i = 0; i < tags.Length; i++)
			{
				string tag = tags[i];
				int count;
				if (!_visitedCountTags.TryGetValue(tag, out count))
					count = 0;

				if (count < min)
					min = count;
			}

			if (min == int.MaxValue)
				min = 0;

			return min;
		}

		protected int turns()
		{
			return _turns;
		}
		
		protected string[] tags()
		{
			return this.Tags;
		}

		protected TwineVar parameter(int index)
		{
			return this.PassageParameters == null || this.PassageParameters.Length-1 < index ? new TwineVar(index) : this.PassageParameters[index];
		}
		
	}
}

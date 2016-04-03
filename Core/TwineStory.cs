using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ITwineThread = System.Collections.Generic.IEnumerable<UnityTwine.TwineOutput>;

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
		public TwineRuntimeVars Vars { get; protected set; }
		public string CurrentPassageName { get; private set; }
		
		public string PreviousPassageName { get; private set; }

		protected Dictionary<string, TwinePassage> Passages { get; private set; }
		TwineStoryState _state = TwineStoryState.Idle;
		IEnumerator<TwineOutput> _currentThread = null;
		TwineLink _currentLinkInAction = null;
		ThreadResult _lastThreadResult = ThreadResult.Done;
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

		private enum ThreadResult
		{
			InProgress = 0,
			Done = 1,
			Aborted = 2
		}

		public TwineStory()
		{
			TwineVar.RegisterTypeService<bool>(new BoolService());
			TwineVar.RegisterTypeService<int>(new IntService());
			TwineVar.RegisterTypeService<double>(new DoubleService());
			TwineVar.RegisterTypeService<string>(new StringService());

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
			if (this.State != TwineStoryState.Idle)
				throw new InvalidOperationException("Can only reset a story that is Idle.");

			// Reset twine vars
			if (Vars != null)
				Vars.Reset();

			this.Init();
		}

		/// <summary>
		/// Begins the story by calling GoTo(StartPassage).
		/// </summary>
		public void Begin()
		{
			GoTo(StartPassage);
		}

		/// <summary>
		/// 
		/// </summary>
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

		/// <summary>
		/// While the story is playing, pauses the execution of the current story thread.
		/// </summary>
		public void Pause()
		{
			if (this.State != TwineStoryState.Playing && this.State != TwineStoryState.Exiting)
				throw new InvalidOperationException("Pause can only be called while a passage is playing or exiting.");

			this.State = TwineStoryState.Paused;
		}

		/// <summary>
		/// When the story is paused, resumes execution of the current story thread.
		/// </summary>
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
				ExecuteCurrentThread();
			}
		}

		TwinePassage GetPassage(string passageName)
		{
			TwinePassage passage;
			if (!Passages.TryGetValue(passageName, out passage))
				throw new TwineException(String.Format("Passage '{0}' does not exist.", passageName));
			return passage;
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
			this.Tags = (string[])passage.Tags.Clone();
			this.PreviousPassageName = this.CurrentPassageName;
			this.CurrentPassageName = passage.Name;

			// Update visited counters for passages and tags
			int visitedPassage;
			if (!_visitedCountPassages.TryGetValue(passageName, out visitedPassage))
				visitedPassage = 0;
			_visitedCountPassages[passageName] = visitedPassage + 1;

			for (int i = 0; i < passage.Tags.Length; i++)
			{
				string tag = passage.Tags[i];
				int visitedTag;
				if (!_visitedCountTags.TryGetValue(tag, out visitedTag))
					visitedTag = 0;
				_visitedCountTags[tag] = visitedTag + 1;
			}

			// Add output (and trigger hooks)
			this.Output.Add(passage);

			// Get update hooks for calling during update
			_passageUpdateHooks = HooksFind("Update", reverse: false, allowCoroutines: false).ToArray();

			// Prepare the thread enumerator
			_currentThread = CollapseThread(passage.GetMainThread()).GetEnumerator();
			_currentLinkInAction = null;

			this.State = TwineStoryState.Playing;
			SendOutput(passage);
			HooksInvoke(HooksFind("Enter", maxLevels: 1));

			// Story was paused, wait for it to resume
			if (this.State == TwineStoryState.Paused)
				return;
			else
			{
				ExecuteCurrentThread();
			}
		}

		/// <summary>
		/// Executes the current thread by advancing its enumerator, sending its output and invoking hooks.
		/// </summary>
		void ExecuteCurrentThread()
		{
			TwineAbort aborted = null;

			while (_currentThread.MoveNext())
			{
				TwineOutput output = _currentThread.Current;

				// Abort this thread
				if (output is TwineAbort)
				{
					aborted = (TwineAbort) output;
					break;
				}

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
				else if (output is TwinePassage)
				{
					HooksInvoke(HooksFind("Enter", reverse: true, maxLevels: 1));

					// Refresh the update hooks
					_passageUpdateHooks = HooksFind("Update", reverse: false, allowCoroutines: false).ToArray();
				}

				// Send output
				SendOutput(output);
				HooksInvoke(HooksFind("Output"), output);

				// Story was paused, wait for it to resume
				if (this.State == TwineStoryState.Paused)
				{
					_lastThreadResult = ThreadResult.InProgress;
					return;
				}
			}

			_currentThread.Dispose();
			_currentThread = null;

			this.State = TwineStoryState.Idle;

			// Return the appropriate result
			if (aborted != null)
			{
				HooksInvoke(HooksFind("Aborted"));
				if (aborted.GoToPassage != null)
					this.GoTo(aborted.GoToPassage);

				_lastThreadResult = ThreadResult.Aborted;
			}
			else
			{
				// Invoke the done hook - either for main or for a link
				if (_currentLinkInAction == null)
					HooksInvoke(HooksFind("Done"));
				else
					HooksInvoke(HooksFind("ActionDone"), _currentLinkInAction);

				_lastThreadResult = ThreadResult.Done;
			}

			_currentLinkInAction = null;
		}

		/// <summary>
		/// Invokes and bubbles up output of inner passages (display).
		/// </summary>
		ITwineThread CollapseThread(ITwineThread thread)
		{
			foreach (TwineOutput output in thread)
			{
				if (output is TwineDisplay)
				{
					yield return output;
					var display = (TwineDisplay)output;
					var displayParams = (TwineVar[])display.Parameters.Clone();
					this.PassageParameters = displayParams;

					TwinePassage displayPassage = GetPassage(display.PassageName);
					yield return displayPassage;
					foreach (TwineOutput innerOutput in CollapseThread(displayPassage.GetMainThread()))
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

		public void DoLink(TwineLink link)
		{
			if (this.State != TwineStoryState.Idle)
			{
				throw new InvalidOperationException(
					// Paused
					this.State == TwineStoryState.Paused ?
						"The story is currently paused. Resume() must be called before a link can be used." :
					// Playing
					this.State == TwineStoryState.Playing || this.State == TwineStoryState.Exiting ?
						"A link can be used only when the story is in the Idle state." :
					// Complete
						"The story is complete. Reset() must be called before it can be played again."
					);
			}

			// Process the link action before continuing
			if (link.Action != null)
			{
				// Action might invoke a fragment method, in which case we need to process it with hooks etc.
				ITwineThread linkActionThread = link.Action.Invoke();
				if (linkActionThread != null)
				{
					// Prepare the fragment thread enumerator
					_currentThread = CollapseThread(linkActionThread).GetEnumerator();
					_currentLinkInAction = link;

					// Resume story, this time with the actoin thread
					this.State = TwineStoryState.Playing;

					ExecuteCurrentThread();
				}
			}

			// Continue to the link passage only if a fragment thread (opened by the action) isn't in progress
			if (link.PassageName != null && _lastThreadResult == ThreadResult.Done)
			{
				_turns++;
				GoTo(link.PassageName);
			}
		}

		public void DoLink(int linkIndex)
		{
			DoLink(this.Links[linkIndex]);
		}

		public void DoLink(string linkName)
		{
			TwineLink link = GetLink(linkName, true);
			DoLink(link);
		}

		public bool HasLink(string linkName)
		{
			return GetLink(linkName) != null;
		}

		public TwineLink GetLink(string linkName, bool throwException = false)
		{
			TwineLink link = this.Links
				.Where(lnk => string.Equals(lnk.Name, linkName, System.StringComparison.OrdinalIgnoreCase))
				.FirstOrDefault();

			if (link == null && throwException)
				throw new TwineException(string.Format("There is no available link with the name '{0}'.", linkName));

			return link;
		}

		[Obsolete("Use DoLink instead.")]
		public void Advance(TwineLink link)
		{
			DoLink(link);
		}

		[Obsolete("Use DoLink instead.")]
		public void Advance(int linkIndex)
		{
			DoLink(linkIndex);
		}

		[Obsolete("Use DoLink instead.")]
		public void Advance(string linkName)
		{
			DoLink(linkName);
		}

		// ---------------------------------
		// Hooks

		static Regex _validPassageNameRegex = new Regex("^[a-z_][a-z0-9_]*$", RegexOptions.IgnoreCase);

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
					Debug.LogWarning(string.Format("Passage \"{0}\" is not hookable because it does not follow C# variable naming rules.", passage.Name));
					continue;
				}

				List<Hook> hooks = HookGetMethods(passage.Name + '_' + hookName, allowCoroutines);
				if (hooks != null)
				{
					for (int h = 0; h < hooks.Count; h++)
						yield return hooks[h];
					
					if (maxLevels > 0 && c == maxLevels-1)
						yield break;
				}
			}
		}

		void HookInvoke(Hook hook, object[] args)
		{
			object result = null;
			try { result = hook.method.Invoke(hook.target, args); }
			catch(TargetParameterCountException)
			{
				Debug.LogErrorFormat("The hook {0} doesn't have the right parameters so it is being ignored.",
					hook.method.Name
				);
				return;
			}

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

		public void HooksClear()
		{
			_hookCache.Clear();
			_hookTargets = null;
		}


		// ---------------------------------
		// Functions

		protected TwineVar v(string val)
		{
			return new TwineVar(val);
		}

		protected TwineVar v(double val)
		{
			return new TwineVar(val);
		}

		protected TwineVar v(int val)
		{
			return new TwineVar(val);
		}

		protected TwineVar v(bool val)
		{
			return new TwineVar(val);
		}

		protected TwineVar v(object val)
		{
			return new TwineVar(val);
		}

		// OBSOLETE: //

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
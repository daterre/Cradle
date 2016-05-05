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
	public enum TwineStoryState
	{
		Idle = 0,
		Playing = 1,
		Paused = 2,
		Exiting = 3
	}

    public abstract class TwineStory: MonoBehaviour
    {
		public bool AutoPlay = true;
        public string StartPassage = "Start";
		public GameObject[] AdditionalCues;

		public event Action<TwinePassage> OnPassageEnter;
		public event Action<TwineStoryState> OnStateChanged;
		public event Action<TwineOutput> OnOutput;
		
		public List<TwineText> Text { get; private set; }
		public List<TwineLink> Links { get; private set; }
		public List<TwineOutput> Output { get; private set; }
		public string[] Tags { get; private set; }
		public TwineRuntimeVars Vars { get; protected set; }
		public string CurrentPassageName { get; private set; }
		public string PreviousPassageName { get; private set; }
		public TwineStyle Style { get; private set; }

        public Dictionary<string, TwinePassage> Passages { get; private set; }
		TwineStoryState _state = TwineStoryState.Idle;
		IEnumerator<TwineOutput> _currentThread = null;
		TwineLink _currentLinkInAction = null;
		ThreadResult _lastThreadResult = ThreadResult.Done;
		Cue[] _passageUpdateCues = null;
		string _passageWaitingToEnter = null;
		Dictionary<string, List<Cue>> _cueCache = new Dictionary<string, List<Cue>>();
		MonoBehaviour[] _cueTargets = null;
		List<TwineStyleScope> _scopes = new List<TwineStyleScope>();
		TwineStyle _scopeStyle;

		public int NumberOfLinksDone { get; private set; }
        public List<string> PassageHistory {get; private set; }

		private class Cue
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

            this.PassageHistory = new List<string>();
		}

		protected void Init()
		{
			_state = TwineStoryState.Idle;
			this.Output = new List<TwineOutput>();
			this.Text = new List<TwineText>();
			this.Links = new List<TwineLink>();
			this.Tags = new string[0];
			this.Style = new TwineStyle();

			NumberOfLinksDone = 0;
			PassageHistory.Clear();
			
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

				// invoke exit cues
				CuesInvoke(CuesFind("Exit", reverse: true));
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
			this.Style = new TwineStyle();
			_passageUpdateCues = null;

			TwinePassage passage = GetPassage(passageName);
			this.Tags = (string[])passage.Tags.Clone();
			this.PreviousPassageName = this.CurrentPassageName;
			this.CurrentPassageName = passage.Name;

            PassageHistory.Add(passageName);

			// Invoke the general passage enter event
			if (this.OnPassageEnter != null)
				this.OnPassageEnter(passage);

			// Add output (and trigger cues)
			this.Output.Add(passage);

			// Get update cues for calling during update
			_passageUpdateCues = CuesFind("Update", reverse: false, allowCoroutines: false).ToArray();

			// Prepare the thread enumerator
			_currentThread = CollapseThread(passage.GetMainThread()).GetEnumerator();
			_currentLinkInAction = null;

			this.State = TwineStoryState.Playing;
			SendOutput(passage);
			CuesInvoke(CuesFind("Enter", maxLevels: 1));

			// Story was paused, wait for it to resume
			if (this.State == TwineStoryState.Paused)
				return;
			else
			{
				ExecuteCurrentThread();
			}
		}

		/// <summary>
		/// Executes the current thread by advancing its enumerator, sending its output and invoking cues.
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
				// Let the handlers and cues kick in
				else if (output is TwinePassage)
				{
					CuesInvoke(CuesFind("Enter", reverse: true, maxLevels: 1));

					// Refresh the update cues
					_passageUpdateCues = CuesFind("Update", reverse: false, allowCoroutines: false).ToArray();
				}

				// Send output
				SendOutput(output);
				CuesInvoke(CuesFind("Output"), output);

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
				CuesInvoke(CuesFind("Aborted"));
				if (aborted.GoToPassage != null)
					this.GoTo(aborted.GoToPassage);

				_lastThreadResult = ThreadResult.Aborted;
			}
			else
			{
				// Invoke the done cue - either for main or for a link
				if (_currentLinkInAction == null)
					CuesInvoke(CuesFind("Done"));
				else
					CuesInvoke(CuesFind("ActionDone"), _currentLinkInAction);

				_lastThreadResult = ThreadResult.Done;
			}

			_currentLinkInAction = null;
		}

		/// <summary>
		/// Invokes and bubbles up output of embedded fragments and passages.
		/// </summary>
		ITwineThread CollapseThread(ITwineThread thread)
		{
			foreach (TwineOutput output in thread)
			{
				foreach (TwineOutput scopeTag in ScopeOutputOpeners())
					yield return scopeTag;

				if (output is TwineEmbed)
				{
					var embed = (TwineEmbed) output;
					ITwineThread embeddedThread;
					if (embed is TwineEmbedPassage)
					{
						var embedInfo = (TwineEmbedPassage)embed;
						TwinePassage passage = GetPassage(embedInfo.Name);
						embeddedThread = passage.GetMainThread();
					}
					else if (embed is TwineEmbedFragment)
					{
						var embedInfo = (TwineEmbedFragment)embed;
                        embeddedThread = embedInfo.GetThread();
					}
					else
						continue;

					// First yield the embed
					yield return embed;

					// Output the content
					foreach (TwineOutput innerOutput in CollapseThread(embeddedThread))
					{
						innerOutput.EmbedInfo = embed;
						yield return innerOutput;
					}
				}
				else
					yield return output;

				foreach (TwineOutput scopeTag in ScopeOutputClosers())
					yield return scopeTag;
			}

			foreach (TwineOutput scopeTag in ScopeOutputClosers())
				yield return scopeTag;
		}

		void SendOutput(TwineOutput output)
		{
			output.Style = this.Style;

			if (OnOutput != null)
				OnOutput(output);
		}

		// ---------------------------------
		// Scope control

		protected TwineStyleScope ApplyStyle(string setting, object value)
		{
			return ApplyStyle(new TwineStyle(setting, value));
		}

		protected TwineStyleScope ApplyStyle(TwineStyle style)
		{
			return ScopeOpen(style);
		}

		/// <summary>
		/// Helper method to create a new style scope.
		/// </summary>
		TwineStyleScope ScopeOpen(TwineStyle style)
		{
			TwineStyleScope scope = new TwineStyleScope()
			{
				State = TwineStyleScopeState.PendingOpen,
				Style = style
			};
			scope.OnDisposed += ScopeWasDisposed;
			_scopes.Add(scope);

			ScopeBuildStyle();

			return scope;
		}

		void ScopeWasDisposed(TwineStyleScope scope)
		{
			scope.OnDisposed -= ScopeWasDisposed;
			_scopes.Remove(scope);
			
			ScopeBuildStyle();
			scope.State = TwineStyleScopeState.PendingClose;
		}

		ITwineThread ScopeOutputOpeners()
		{
			for (int i = 0; i < _scopes.Count; i++)
			{
				if (_scopes[i].State == TwineStyleScopeState.PendingOpen)
				{
					_scopes[i].State = TwineStyleScopeState.Open;
					yield return new TwineStyleTag(TwineStyleTagType.Opener, _scopes[i].Style);
				}
			}
		}

		ITwineThread ScopeOutputClosers()
		{
			for (int i = _scopes.Count - 1; i >= 0; i--)
			{
				if (_scopes[i].State == TwineStyleScopeState.PendingClose)
				{
					_scopes[i].State = TwineStyleScopeState.Closed;
					yield return new TwineStyleTag(TwineStyleTagType.Closer, _scopes[i].Style);
				}
			}
		}

		void ScopeBuildStyle()
		{
			TwineStyle style = new TwineStyle();
			for (int i = 0; i < _scopes.Count; i++)
				style += _scopes[i].Style;

			this.Style = style;
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
				// Action might invoke a fragment method, in which case we need to process it with cues etc.
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
				NumberOfLinksDone++;
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
		// Cues

		static Regex _validPassageNameRegex = new Regex("^[a-z_][a-z0-9_]*$", RegexOptions.IgnoreCase);

		void Update()
		{
			CuesInvoke(_passageUpdateCues);
		}

		void CuesInvoke(IEnumerable<Cue> cues, params object[] args)
		{
			if (cues == null)
				return;

			if (cues is Cue[])
			{
				var ar = (Cue[]) cues;
				for (int i = 0; i < ar.Length; i++)
					CueInvoke(ar[i], args);
			}
			else
			{
				foreach (Cue cue in cues)
					CueInvoke(cue, args);
			}
		}

		IEnumerable<Cue> CuesFind(string cueName, int maxLevels = 0, bool reverse = false, bool allowCoroutines = true)
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
				// TODO: use attribute when passage is named this way
				if (!_validPassageNameRegex.IsMatch(passage.Name))
				{
					Debug.LogWarning(string.Format("Passage \"{0}\" cannot use cues because it does not follow C# variable naming rules.", passage.Name));
					continue;
				}

				List<Cue> cues = CueGetMethods(passage.Name + '_' + cueName, allowCoroutines);
				if (cues != null)
				{
					for (int h = 0; h < cues.Count; h++)
						yield return cues[h];
					
					if (maxLevels > 0 && c == maxLevels-1)
						yield break;
				}
			}
		}

		void CueInvoke(Cue cue, object[] args)
		{
			object result = null;
			try { result = cue.method.Invoke(cue.target, args); }
			catch(TargetParameterCountException)
			{
				Debug.LogWarningFormat("The cue {0} doesn't have the right parameters so it is being ignored.",
					cue.method.Name
				);
				return;
			}

			if (result is IEnumerator)
				StartCoroutine(((IEnumerator)result));
		}

		MonoBehaviour[] CueGetTargets()
		{
			if (_cueTargets == null)
			{
				// ...................
				// Get all hook targets
				GameObject[] cueTargets;
				if (this.AdditionalCues != null)
				{
					cueTargets = new GameObject[this.AdditionalCues.Length + 1];
					cueTargets[0] = this.gameObject;
					this.AdditionalCues.CopyTo(cueTargets, 1);
				}
				else
					cueTargets = new GameObject[] { this.gameObject };

				// Get all types
				var sources = new List<MonoBehaviour>();
				for (int i = 0; i < cueTargets.Length; i++)
					sources.AddRange(cueTargets[i].GetComponents<MonoBehaviour>());

				_cueTargets = sources.ToArray();
			}

			return _cueTargets;
		}

		List<Cue> CueGetMethods(string methodName, bool allowCoroutines = true)
		{
			List<Cue> cues = null;
			if (!_cueCache.TryGetValue(methodName, out cues))
			{
				MonoBehaviour[] targets = CueGetTargets();
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
							Debug.LogError(targetType.Name + "." + methodName + " must return void or IEnumerator in order to be used as a cue.");
							method = null;
						}
					}
					else
					{
						if (method.ReturnType != typeof(void))
						{
							Debug.LogError(targetType.Name + "." + methodName + " must return void in order to be used as a cue.");
							method = null;
						}
					}

					// The found method wasn't valid
					if (method == null)
						continue;

					// Init the method list
					if (cues == null)
						cues = new List<Cue>();

					cues.Add(new Cue() { method = method, target = targets[i] } );
				}

				// Cache the method list even if it's null so we don't do another lookup next time around (lazy load)
				_cueCache.Add(methodName, cues);
			}

			return cues;
		}

		public void CuesClear()
		{
			_cueCache.Clear();
			_cueTargets = null;
		}


		// ---------------------------------
		// Shorthand functions

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

		protected TwineText text(TwineVar text)
		{
			return new TwineText(TwineVar.ConvertTo<string>(text.Value, strict: false));
		}

		protected TwineLineBreak lineBreak()
		{
			return new TwineLineBreak();
		}

		protected TwineLink link(string text, string passageName, Func<ITwineThread> action)
		{
			return new TwineLink(text, passageName, action);
		}

		protected TwineAbort abort(string goToPassage)
		{
			return new TwineAbort(goToPassage);
		}

		protected TwineEmbedFragment fragment(Func<ITwineThread> action)
		{
			return new TwineEmbedFragment(action);
		}

		protected TwineEmbedPassage passage(string passageName, params TwineVar[] parameters)
		{
			return new TwineEmbedPassage(passageName, parameters);
		}

		protected TwineStyle style(string setting, object value)
		{
			return new TwineStyle(setting, value);
		}

		protected TwineStyle style(TwineVar expression)
		{
			return new TwineStyle(expression);
		}
	}
}
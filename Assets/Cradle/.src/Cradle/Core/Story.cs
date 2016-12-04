using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using IStoryThread = System.Collections.Generic.IEnumerable<Cradle.StoryOutput>;

namespace Cradle
{
	public enum StoryState
	{
		Idle = 0,
		Playing = 1,
		Paused = 2,
		Exiting = 3
	}

    public abstract class Story: MonoBehaviour
    {
		public bool AutoPlay = true;
        public string StartPassage = "Start";
		public GameObject[] AdditionalCues;

		public event Action<StoryPassage> OnPassageEnter;
		public event Action<StoryPassage> OnPassageDone;
		public event Action<StoryState> OnStateChanged;
		public event Action<StoryOutput> OnOutput;
		public event Action<StoryOutput> OnOutputRemoved;
		
        public Dictionary<string, StoryPassage> Passages { get; private set; }
		public List<StoryOutput> Output { get; private set; }
		public string[] Tags { get; private set; }
		public RuntimeVars Vars { get; protected set; }
		public string CurrentPassageName { get; private set; }
		public StoryLink CurrentLinkInAction { get; private set; }
		public int NumberOfLinksDone { get; private set; }
        public List<string> PassageHistory {get; private set; }
		public float PassageTime { get { return _timeAccumulated + (Time.time - _timeChangedToPlay); } }
		public StorySaveData SaveData { get; private set; }

		StoryState _state = StoryState.Idle;
		IEnumerator<StoryOutput> _currentThread = null;
		ThreadResult _lastThreadResult = ThreadResult.Done;
		Cue[] _passageUpdateCues = null;
		string _passageWaitingToEnter = null;
		bool _passageEnterCueInvoked = false;
		Dictionary<string, List<Cue>> _cueCache = new Dictionary<string, List<Cue>>();
		MonoBehaviour[] _cueTargets = null;
		float _timeChangedToPlay = 0f;
		float _timeAccumulated;
		Stack<OutputGroup> _groupStack = new Stack<OutputGroup>();

		protected Stack<int> InsertStack = new Stack<int>();

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

		public Story()
		{
			StoryVar.RegisterTypeService<bool>(new BoolService());
			StoryVar.RegisterTypeService<int>(new IntService()); 
			StoryVar.RegisterTypeService<double>(new DoubleService());
			StoryVar.RegisterTypeService<string>(new StringService());

			this.Passages = new Dictionary<string, StoryPassage>();

            this.PassageHistory = new List<string>();
		}

		protected void Init()
		{
			_state = StoryState.Idle;
			this.Output = new List<StoryOutput>();
			this.Tags = new string[0];

			NumberOfLinksDone = 0;
			PassageHistory.Clear();
			InsertStack.Clear();
			
			CurrentPassageName = null;
		}

		void Start()
		{
			if (AutoPlay)
				this.Begin();
		}

		// ---------------------------------
		// State control

		public StoryState State
		{
			get { return _state; }
			private set
			{
				StoryState prev = _state;
				_state = value;
				if (prev != value && OnStateChanged != null)
					OnStateChanged(value);
			}
		}

		public void Reset()
		{
			if (this.State != StoryState.Idle)
				throw new InvalidOperationException("Can only reset a story that is Idle.");

			// Reset twine vars
			if (Vars != null)
				Vars.Reset();

			this.Init();
		}

		protected virtual StorySaveData Save()
		{
			var saveData = new StorySaveData()
			{
				PassageHistory = this.PassageHistory.ToList(),
				PassageToResume = this.CurrentPassageName,
				Variables = new Dictionary<string, StoryVar>()
			};

			foreach(var pair in this.Vars)
				saveData.Variables[pair.Key] = pair.Value;

			return saveData;
		}

		public virtual void Load(StorySaveData saveData)
		{
			if (this.State != StoryState.Idle)
				throw new InvalidOperationException("Can only load a story that is Idle.");

			this.Reset();
			this.PassageHistory.AddRange(saveData.PassageHistory);
			foreach (var pair in saveData.Variables)
				this.Vars[pair.Key] = pair.Value;

			this.GoTo(saveData.PassageToResume);
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
			if (this.State != StoryState.Idle)
			{
				throw new InvalidOperationException(
					// Paused
					this.State == StoryState.Paused ?
						"The story is currently paused. Resume() must be called before advancing to a different passage." :
					// Playing
					this.State == StoryState.Playing || this.State == StoryState.Exiting ?
						"The story can only be advanced when it is in the Idle state." :
					// Complete
						"The story is complete. Reset() must be called before it can be played again."
					);
			}
			
			// Indicate specified passage as next
			_passageWaitingToEnter = passageName;

			if (CurrentPassageName != null)
			{
				this.State = StoryState.Exiting;

				// invoke exit cues
				CuesInvoke(CuesFind("Exit", reverse: true));
			}

			if (this.State != StoryState.Paused)
				Enter(passageName);
		}

		/// <summary>
		/// While the story is playing, pauses the execution of the current story thread.
		/// </summary>
		public void Pause()
		{
			if (this.State != StoryState.Playing && this.State != StoryState.Exiting)
				throw new InvalidOperationException("Pause can only be called while a passage is playing or exiting.");

			this.State = StoryState.Paused;
			_timeAccumulated = Time.time - _timeChangedToPlay;
		}

		/// <summary>
		/// When the story is paused, resumes execution of the current story thread.
		/// </summary>
		public void Resume()
		{
			if (this.State != StoryState.Paused)
			{
				throw new InvalidOperationException(
					// Paused
					this.State == StoryState.Idle ?
						"The story is currently idle. Call Begin, DoLink or GoTo to play." :
					// Playing
					this.State == StoryState.Playing || this.State == StoryState.Exiting?
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
				this.State = StoryState.Playing;
				_timeAccumulated = Time.time - _timeChangedToPlay;

				// The passage enter cue hasn't been invoked yet, probably because of a Pause() call in an OnPassageEnter event.
				if (!_passageEnterCueInvoked)
				{
					CuesInvoke(CuesGet(this.CurrentPassageName, "Enter"));
					_passageEnterCueInvoked = true;
				}

				// Continue un-pausing only if the passage enter cue didn't pause again
				if (this.State == StoryState.Playing)
					ExecuteCurrentThread();
			}
		}

		public StoryPassage GetPassage(string passageName)
		{
			StoryPassage passage;
			if (!Passages.TryGetValue(passageName, out passage))
				throw new StoryException(String.Format("Passage '{0}' does not exist.", passageName));
			return passage;
		}

		public IEnumerable<string> GetPassagesWithTag(string tag)
		{
			return this.Passages
				.Where(pair => pair.Value.Tags.Contains(tag, System.StringComparer.InvariantCultureIgnoreCase))
				.Select(pair => pair.Key)
				.OrderBy(passageName => passageName, StringComparer.InvariantCulture);
		}

		protected virtual Func<IStoryThread> GetPassageThread(StoryPassage passage)
		{
			return passage.MainThread;
		}

		void Enter(string passageName)
		{
			this.SaveData = this.Save();

			_passageWaitingToEnter = null;
			_passageEnterCueInvoked = false;
			_timeAccumulated = 0;
			_timeChangedToPlay = Time.time;

			this.InsertStack.Clear();
			this.Output.Clear();
			_groupStack.Clear();
			_passageUpdateCues = null;

			StoryPassage passage = GetPassage(passageName);
			this.Tags = (string[])passage.Tags.Clone();
			this.CurrentPassageName = passage.Name;

            PassageHistory.Add(passageName);

			// Prepare the thread enumerator
			_currentThread = CollapseThread(GetPassageThread(passage).Invoke()).GetEnumerator();
			CurrentLinkInAction = null;

			this.State = StoryState.Playing;
			
			// Invoke the general passage enter event
			if (this.OnPassageEnter != null)
				this.OnPassageEnter(passage);

			// Story was paused, wait for it to resume
			if (this.State == StoryState.Paused)
				return;
			else
			{
				CuesInvoke(CuesGet(passage.Name, "Enter"));
				_passageEnterCueInvoked = true;
				ExecuteCurrentThread();
			}
		}

		/// <summary>
		/// Executes the current thread by advancing its enumerator, sending its output and invoking cues.
		/// </summary>
		void ExecuteCurrentThread()
		{
			Abort aborted = null;
			UpdateCuesRefresh();

			while (_currentThread.MoveNext())
			{
				StoryOutput output = _currentThread.Current;
				
				// If output is not null, process it. Otherwise, just check if story was paused and continue
				if (output != null)
				{
					// Abort this thread
					if (output is Abort)
					{
						aborted = (Abort)output;
						break;
					}

					OutputAdd(output);

					// Let the handlers and cues kick in
					if (output is EmbedPassage)
					{
						CuesInvoke(CuesGet(output.Name, "Enter"));
						UpdateCuesRefresh();
					}

					// Send output
					OutputSend(output);
					CuesInvoke(CuesFind("Output"), output);
				}

				// Story was paused, wait for it to resume
				if (this.State == StoryState.Paused)
				{
					_lastThreadResult = ThreadResult.InProgress;
					return;
				}
			}

			_currentThread.Dispose();
			_currentThread = null;

			// Return the appropriate result
			if (aborted != null)
			{
				_lastThreadResult = ThreadResult.Aborted;
				_passageWaitingToEnter = aborted.GoToPassage;

				CuesInvoke(CuesFind("Aborted"));

				if (aborted.GoToPassage != null && this.State != StoryState.Paused)
					Enter(aborted.GoToPassage);
				else
					this.State = StoryState.Idle;
			}
			else
			{
				_lastThreadResult = ThreadResult.Done;

				this.State = StoryState.Idle;

				// Invoke the general passage enter event
				if (this.OnPassageDone != null)
					this.OnPassageDone(GetPassage(this.CurrentPassageName));

				// Invoke the done cue - either for main or for a link
				if (CurrentLinkInAction == null)
					CuesInvoke(CuesFind("Done"));
				else
					CuesInvoke(CuesFind("Done", CurrentLinkInAction.Name));
			}

			CurrentLinkInAction = null;
		}

		/// <summary>
		/// Invokes and bubbles up output of embedded fragments and passages.
		/// </summary>
		IStoryThread CollapseThread(IStoryThread thread)
		{
			foreach (StoryOutput output in thread)
			{
				//foreach (TwineOutput scopeTag in ScopeOutputTags())
				//	yield return scopeTag;

				if (output is Embed)
				{
					var embed = (Embed) output;
					IStoryThread embeddedThread;
					if (embed is EmbedPassage)
					{
						var embedInfo = (EmbedPassage)embed;
						StoryPassage passage = GetPassage(embedInfo.Name);
						embeddedThread = passage.MainThread();
					}
					else if (embed is EmbedFragment)
					{
						var embedInfo = (EmbedFragment)embed;
                        embeddedThread = embedInfo.GetThread();
					}
					else
						continue;

					// First yield the embed
					yield return embed;

					// Output the content
					foreach (StoryOutput innerOutput in CollapseThread(embeddedThread))
					{
						if (innerOutput != null)
							innerOutput.EmbedInfo = embed;
						yield return innerOutput;
					}
				}
				else
					yield return output;
			}

			//foreach (TwineOutput scopeTag in ScopeOutputTags())
			//	yield return scopeTag;
		}

		void OutputAdd(StoryOutput output)
		{
			// Insert the output into the right place
			int insertIndex = InsertStack.Count > 0 ? InsertStack.Peek() : -1;

			if (insertIndex < 0)
			{
				output.Index = this.Output.Count;
				this.Output.Add(output);
			}
			else
			{
				// When a valid insert index is specified, update the following outputs' index
				output.Index = insertIndex;
				this.Output.Insert(insertIndex, output);
				OutputUpdateIndexes(insertIndex + 1);
			}

			// Increase the topmost index
			if (InsertStack.Count > 0 && insertIndex >= 0)
				InsertStack.Push(InsertStack.Pop() + 1);
		}

		void OutputSend(StoryOutput output, bool add = false)
		{
			if (_groupStack.Count > 0)
				output.Group = _groupStack.Peek();

			if (add)
				OutputAdd(output);

			if (OnOutput != null)
				OnOutput(output);
		}

		protected void OutputRemove(StoryOutput output)
		{
			if (this.Output.Remove(output))
			{
				if (OnOutputRemoved != null)
					OnOutputRemoved(output);
				OutputUpdateIndexes(output.Index);
			}
		}

		void OutputUpdateIndexes(int startIndex)
		{
			for (int i = startIndex; i < this.Output.Count; i++)
				this.Output[i].Index = i;
		}


		public IEnumerable<StoryLink> GetCurrentLinks()
		{
			return this.Output.Where(o => o is StoryLink).Cast<StoryLink>();
		}

		public IEnumerable<StoryText> GetCurrentText()
		{
			return this.Output.Where(o => o is StoryText).Cast<StoryText>();
		}

		public bool IsFirstVisitToPassage
		{
			get { return PassageHistory.Count(p => p == this.CurrentPassageName) == 1; }
		}

		// ---------------------------------
		// Grouping and style

		protected GroupScope Group(string setting, object value)
		{
			return Group(new StoryStyle(setting, value));
		}

		protected GroupScope Group(StoryStyle style)
		{
			var group = new OutputGroup(style);
			OutputSend(group, add: true);

			return Group(group);
		}

		protected GroupScope Group(OutputGroup group)
		{
			var scope = new GroupScope(group);
			scope.OnDisposed += GroupScopeClose;

			_groupStack.Push(scope.Group);

			return scope;
		}

		void GroupScopeClose(GroupScope scope)
		{
			scope.OnDisposed -= GroupScopeClose;
			if (_groupStack.Pop() != scope.Group)
				throw new System.Exception("Unexpected group was closed.");
		}

		public OutputGroup CurrentGroup
		{
			get { return _groupStack.Peek(); }
		}

		public StoryStyle GetCurrentStyle()
		{
			OutputGroup group = _groupStack.Count > 0 ? _groupStack.Peek() : null;
			if (group != null)
				return group.GetAppliedStyle() + group.Style; // Combine styles
			else
				return new StoryStyle();
		}
		
		// ---------------------------------
		// Links

		public void DoLink(StoryLink link)
		{
			if (this.State != StoryState.Idle)
			{
				throw new InvalidOperationException(
					// Paused
					this.State == StoryState.Paused ?
						"The story is currently paused. Resume() must be called before a link can be used." :
					// Playing
					this.State == StoryState.Playing || this.State == StoryState.Exiting ?
						"A link can be used only when the story is in the Idle state." :
					// Complete
						"The story is complete. Reset() must be called before it can be played again."
					);
			}

			// Process the link action before continuing
			if (link.Action != null)
			{
				CurrentLinkInAction = link;

				// Action might invoke a fragment method, in which case we need to process it with cues etc.
				IStoryThread linkActionThread = link.Action.Invoke();
				if (linkActionThread != null)
				{
					// Prepare the fragment thread enumerator
					_currentThread = CollapseThread(linkActionThread).GetEnumerator();

					// Resume story, this time with the actoin thread
					this.State = StoryState.Playing;

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
			DoLink(this.GetCurrentLinks().ElementAt(linkIndex));
		}

		public void DoLink(string linkName)
		{
			StoryLink link = GetLink(linkName, true);
			DoLink(link);
		}

		public bool HasLink(string linkName)
		{
			return GetLink(linkName) != null;
		}

		public StoryLink GetLink(string linkName, bool throwException = false)
		{
			StoryLink link = this.GetCurrentLinks()
				.Where(lnk => string.Equals(lnk.Name, linkName, System.StringComparison.OrdinalIgnoreCase))
				.FirstOrDefault();

			if (link == null && throwException)
				throw new StoryException(string.Format("There is no available link with the name '{0}' in the passage '{1}'.", linkName, this.CurrentPassageName));

			return link;
		}

		[Obsolete("Use DoLink instead.")]
		public void Advance(StoryLink link)
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
		const BindingFlags _cueMethodFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase;

		void Update()
		{
			CuesInvoke(_passageUpdateCues);
		}

		void UpdateCuesRefresh()
		{
			// Get update cues for calling during update
			_passageUpdateCues = CuesFind("Update", reverse: false, allowCoroutines: false).ToArray();
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


		IEnumerable<Cue> CuesFind(string cueName, string linkName = null, bool reverse = false, bool allowCoroutines = true)
		{
			// Get main passage's cues
			List<Cue> mainCues = CuesGet(this.CurrentPassageName, cueName, linkName, allowCoroutines);

			// Return them here only if not reversing
			if (!reverse && mainCues != null)
			{
				for (int h = 0; h < mainCues.Count; h++)
					yield return mainCues[h];
			}

			// Note: Since 2.0, the Output list can be rearranged by changing the InsertStack.
			// Consequently, later embedded passages' cues can be triggered before earlier embedded
			// passages' cues if they were inserted higher up in the list.
			int c = 0;
			for(
				int i = reverse ? this.Output.Count - 1 : 0;
				reverse ? i >= 0 : i < this.Output.Count;
				c++, i = i + (reverse ? -1 : 1)
				)
			{
				EmbedPassage passageEmbed = this.Output[i] as EmbedPassage;
				if (passageEmbed == null)
					continue;

				List<Cue> cues = CuesGet(passageEmbed.Name, cueName, linkName, allowCoroutines);
				if (cues != null)
				{
					for (int h = 0; h < cues.Count; h++)
						yield return cues[h];
				}
			}

			// Reversing, so return the main cues now
			if (reverse && mainCues != null)
			{
				for (int h = 0; h < mainCues.Count; h++)
					yield return mainCues[h];
			}
		}

		void CueInvoke(Cue cue, object[] args)
		{
			object result = null;
			try { result = cue.method.Invoke(cue.target, args); }
			catch(TargetParameterCountException)
			{
				Debug.LogWarningFormat("The cue '{0}' doesn't have the right parameters so it is being ignored.",
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

		List<Cue> CuesGet(string passageName, string cueName, string linkName = null, bool allowCoroutines = true)
		{
			string methodName = passageName + "_" + (linkName != null ? linkName + "_" : null) + cueName;

			List<Cue> cues = null;

			if (!_cueCache.TryGetValue(methodName, out cues))
			{
				MonoBehaviour[] targets = CueGetTargets();
				var methodsFound = new List<MethodInfo>();

				for (int i = 0; i < targets.Length; i++)
				{
					Type targetType = targets[i].GetType();
					methodsFound.Clear();

					// Get methods with attribute
					methodsFound.AddRange(targetType.GetMethods(_cueMethodFlags)
						.Where(m => m.GetCustomAttributes(typeof(StoryCueAttribute), true)
							.Cast<StoryCueAttribute>()
							.Where(attr => attr.PassageName == passageName && attr.LinkName == linkName && attr.CueName == cueName)
							.Count() > 0
						));

					// Get the method by name (if valid)
					if (_validPassageNameRegex.IsMatch(passageName))
					{
						MethodInfo methodByName = targetType.GetMethod(methodName, _cueMethodFlags);

						// Only add it if doesn't have a StoryCue attribute
						if (methodByName != null && methodByName.GetCustomAttributes(typeof(StoryCueAttribute), true).Length == 0)
							methodsFound.Add(methodByName);
					}

					// Now ensure that all methods are valid and add them as cues
					foreach (MethodInfo method in methodsFound)
					{
						// Validate the found method
						if (allowCoroutines)
						{
							if (method.ReturnType != typeof(void) && !typeof(IEnumerator).IsAssignableFrom(method.ReturnType))
							{
								Debug.LogWarning(targetType.Name + "." + methodName + " must return void or IEnumerator in order to be used as a cue.");
								continue;
							}
						}
						else
						{
							if (method.ReturnType != typeof(void))
							{
								Debug.LogWarning(targetType.Name + "." + methodName + " must return void in order to be used as a cue.");
								continue;
							}
						}

						// Init the method list
						if (cues == null)
							cues = new List<Cue>();

						cues.Add(new Cue() { method = method, target = targets[i] });
					}
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

		protected StoryVar v(string val)
		{
			return new StoryVar(val);
		}

		protected StoryVar v(double val)
		{
			return new StoryVar(val);
		}

		protected StoryVar v(int val)
		{
			return new StoryVar(val);
		}

		protected StoryVar v(bool val)
		{
			return new StoryVar(val);
		}

		protected StoryVar v(object val)
		{
			return new StoryVar(val);
		}

		protected StoryText text(StoryVar text)
		{
			return new StoryText(StoryVar.ConvertTo<string>(text.Value, strict: false));
		}

		protected LineBreak lineBreak()
		{
			return new LineBreak();
		}

		protected StoryLink link(string text, string passageName, Func<IStoryThread> action)
		{
			return new StoryLink(text, passageName, action);
		}

		protected Abort abort(string goToPassage)
		{
			return new Abort(goToPassage);
		}

		protected EmbedFragment fragment(Func<IStoryThread> action)
		{
			return new EmbedFragment(action);
		}

		protected EmbedPassage passage(string passageName, params StoryVar[] parameters)
		{
			return new EmbedPassage(passageName, parameters);
		}

		protected StoryStyle style(string setting, object value)
		{
			return new StoryStyle(setting, value);
		}

		protected StoryStyle style(StoryVar expression)
		{
			return new StoryStyle(expression);
		}
	}

	[Serializable]
	public class StorySaveData
	{
		public string PassageToResume;
		public List<string> PassageHistory;
		public Dictionary<string, StoryVar> Variables;
	}
}
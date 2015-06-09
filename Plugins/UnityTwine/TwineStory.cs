using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityTwine
{
    public abstract class TwineStory: MonoBehaviour
    {
		public bool AutoPlay = true;
        public string StartPassage = "Start";
		public MonoBehaviour HookScript = null;

		public event Action<TwineStoryState> OnStateChanged;
		public event Action<TwineOutput> OnOutput;
		
		public List<TwineText> Text { get; private set; }
		public List<TwineLink> Links { get; private set; }
		public List<TwineOutput> Output { get; private set; }
		public Dictionary<string, string> Tags { get; private set; }
		public string CurrentPassageName { get; private set; }
		public string PreviousPassageName { get; private set; }

		protected Dictionary<string, TwinePassage> Passages { get; private set; }
		TwineStoryState _state = TwineStoryState.Idle;
		IEnumerator<TwineOutput> _passageExecutor = null;
		MethodInfo[] _passageUpdateHooks = null;
		string _passageWaitingToEnter = null;
		Dictionary<string, MethodInfo> _hookCache = new Dictionary<string, MethodInfo>();

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
			this.Tags = new Dictionary<string, string>();
			
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
			this.Tags.Clear();
			_passageUpdateHooks = null;

			TwinePassage passage = GetPassage(passageName);
			this.PreviousPassageName = this.CurrentPassageName;
			this.CurrentPassageName = passage.Name;

			this.Output.Add(passage);

			// Prepare the enumerator
			_passageExecutor = ExecutePassage(passage).GetEnumerator();
			
			// Get update hooks for calling during update
			_passageUpdateHooks = HooksFind("Update", reverse: false, allowCoroutines: false).ToArray();

			this.State = TwineStoryState.Playing;
			SendOutput(passage);
			HooksInvoke(HooksFind("Enter", max: 1));

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

				if (output is TwinePassage)
				{
					// Merge tags with existing passages
					var passage = (TwinePassage)output;
					foreach (var tag in passage.Tags)
						this.Tags[tag.Key] = tag.Value;
				}
				else if (output is TwineLink)
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
					HooksInvoke(HooksFind("Enter", reverse: true, max: 1));

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
					TwinePassage displayPassage = GetPassage(display.PassageName);
					yield return displayPassage;
					foreach(TwineOutput innerOutput in ExecutePassage(displayPassage))
                        yield return innerOutput;
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

		void Update()
		{
			HooksInvoke(_passageUpdateHooks);
		}

		void HooksInvoke(IEnumerable<MethodInfo> hooks, params object[] args)
		{
			if (this.HookScript == null)
				return;

			if (hooks == null)
				return;

			if (hooks is MethodInfo[])
			{
				var ar = (MethodInfo[]) hooks;
				for (int i = 0; i < ar.Length; i++)
					MethodInvoke(ar[i], args);
			}
			else
			{
				foreach (MethodInfo hook in hooks)
					MethodInvoke(hook, args);
			}
		}

		IEnumerable<MethodInfo> HooksFind(string hookName, int max = 0, bool reverse = false, bool allowCoroutines = true)
		{
			if (this.HookScript == null)
				yield break;

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
				MethodInfo hook = MethodFind(passage.Name + '_' + hookName, allowCoroutines);
				if (hook != null)
				{
					yield return hook;
					c++;
					if (max > 0 && c == max)
						yield break;
				}
			}
		}

		void MethodInvoke(MethodInfo method, object[] args)
		{
			var result = method.Invoke(this.HookScript, args);
			if (result is IEnumerator)
				StartCoroutine(((IEnumerator)result));
		}

		MethodInfo MethodFind(string methodName, bool allowCoroutines = true)
		{
			if (this.HookScript == null)
				throw new UnassignedReferenceException("Can't hook because HookScript is not assigned.");

			MethodInfo method = null;
			if (!_hookCache.TryGetValue(methodName, out method))
			{
				method = this.HookScript.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

				if (method != null)
				{
					if (allowCoroutines)
					{
						if (method.ReturnType != typeof(void) && !typeof(IEnumerator).IsAssignableFrom(method.ReturnType))
						{
							Debug.LogError(methodName + " must return void or IEnumerator in order to hook this story event.");
							method = null;
						}
					}
					else
					{
						if (method.ReturnType != typeof(void))
						{
							Debug.LogError(methodName + " must return void in order to hook to this story event.");
							method = null;
						}
					}
				}

				// Cache it even if it's null so we don't look for it again later
				_hookCache.Add(methodName, method);

				//if (method != null)
				//	Debug.Log("Hooking to " + method.Name);
			}

			return method;
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

		// TODO: You plunge into the [[glowing vortex|either("12000 BC","The Future","2AM Yesterday")]].
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

		protected int visited(params string[] passageNames)
		{
			// TODO: add passage counters
			throw new NotImplementedException();
		}

		protected int visitedTag(params string[] passageNames)
		{
			// TODO: add tag counters
			throw new NotImplementedException();
		}

		protected int turns()
		{
			// TODO: return total link follow counter
			throw new NotImplementedException();
		}
		
		protected string[] tags()
		{
			return this.Tags.Keys.ToArray();
		}

		protected TwineVar paramater(int index)
		{
			// TODO: add parameters to passage execution
			throw new NotImplementedException();
			//return this.Paramaters[index];
		}
		
	}
}

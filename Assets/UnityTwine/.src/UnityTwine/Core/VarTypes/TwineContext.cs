using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using List = System.Collections.Generic.List<object>;

namespace UnityTwine
{
	public class TwineContext: TwineType, IDisposable
	{
		public event Action<TwineContext> OnDisposed;
		public bool IsReadOnly { get; private set; }

		public const string EvaluatedValContextOption = "evaluated-expression";

		List<TwineContext> _appliedContexts = new List<TwineContext>();
		Dictionary<string, List> _appliedValues = new Dictionary<string, List>();
		Dictionary<string, List> _calculatedValues = new Dictionary<string, List>();

		public TwineContext()
		{
		}

		public TwineContext(string name, object value)
		{
			Set(name, value);
		}

		public TwineContext(TwineVar val)
		{
			Set(val);
		}

		public static implicit operator TwineContext(TwineVar contextVar)
		{
			return contextVar.ConvertValueTo<TwineContext>();
		}

		public static bool operator true(TwineContext context)
		{
			return context != null && context._calculatedValues.Count > 0;
		}

		public static bool operator false(TwineContext context)
		{
			return context == null || context._calculatedValues.Count == 0;
		}

		public List<T> GetValues<T>(string name)
		{
			List values;
			if (!_calculatedValues.TryGetValue(name, out values))
				return new List<T>();
			else
				return new List<T>(values.Select(obj => (T)obj));
		}

		public List GetValues(string name)
		{
			return GetValues<object>(name);
		}

		public Dictionary<string,List>.KeyCollection Options
		{
			get { return _calculatedValues.Keys; }
		}

		void Set(string name, object value)
		{
			if (IsReadOnly)
				throw new TwineException("This context is a read-only copy and cannot be modified.");

			List values;
			if (!_appliedValues.TryGetValue(name, out values))
				_appliedValues[name] = values = new List();

			values.Add(value);
			Recalculate();
		}

		void Set(TwineVar val)
		{
			if (val.ConvertValueTo<bool>())
				Set(EvaluatedValContextOption, val);
		}

		public TwineContext Apply(TwineVar val)
		{
			if (typeof(TwineContext).IsAssignableFrom(val.GetInnerType()))
				return Apply(val.ConvertValueTo<TwineContext>());

			if (val.ConvertValueTo<bool>())
				return Apply(EvaluatedValContextOption, val);
			else
				return new TwineContext();
		}

		public TwineContext Apply(TwineContext context)
		{
			if (IsReadOnly)
				throw new TwineException("This context is a read-only copy and cannot be modified.");

			if (_appliedContexts.Contains(context))
				throw new InvalidOperationException("This context has already been applied.");

			if (context._appliedContexts.Count > 0)
				throw new InvalidOperationException("Cannot apply a context that is itself composed of other contexts.");

			context.OnDisposed += Unapply;
			_appliedContexts.Add(context);

			Recalculate();
			return context;
		}

		public TwineContext Apply(string option, object value)
		{
			var newContext = new TwineContext();
			newContext.Set(option, value);
			return Apply(newContext);
		}

		public void Unapply(TwineContext context)
		{
			if (!_appliedContexts.Contains(context))
				return;

			context.OnDisposed -= Unapply;
			_appliedContexts.Remove(context);
			
			Recalculate();
		}

		void Recalculate()
		{
			_calculatedValues.Clear();
			for (int i = 0; i < _appliedContexts.Count; i++)
			{
				TwineContext context = _appliedContexts[i];
				foreach (var entry in context._calculatedValues)
					RecalculateEntry(entry);
			}

			foreach (var entry in this._appliedValues)
				RecalculateEntry(entry);
		}

		void RecalculateEntry(KeyValuePair<string, List> entry)
		{
			List values;
			if (!_calculatedValues.TryGetValue(entry.Key, out values))
				_calculatedValues[entry.Key] = values = new List();
			values.AddRange(entry.Value);
		}

		public void Dispose()
		{
			if (this.OnDisposed != null)
				this.OnDisposed(this);
		}

		public TwineContext GetCopy()
		{
			var copy = new TwineContext()
			{
				IsReadOnly = true,
				_calculatedValues = new Dictionary<string, List>()
			};

			foreach (var entry in this._calculatedValues)
				copy._calculatedValues[entry.Key] = new List(entry.Value);

			return copy;
		}

		public override TwineVar GetMember(TwineVar member)
		{
			throw new NotImplementedException();
		}

		public override void SetMember(TwineVar member, TwineVar value)
		{
			throw new NotImplementedException();
		}

		public override void RemoveMember(TwineVar member)
		{
			throw new NotImplementedException();
		}

		public override bool Compare(TwineOperator op, object b, out bool result)
		{
			result = default(TwineVar);
			return false;
		}

		public override bool Combine(TwineOperator op, object b, out TwineVar result)
		{
			result = default(TwineVar);
			if (!(b is TwineContext) || op != TwineOperator.Add)
				return false;
			TwineContext bContext = (TwineContext)b;

			var combinedContext = new TwineContext();
			combinedContext.Apply(this);
			combinedContext.Apply(bContext);
			result = combinedContext.GetCopy();
			return true;
		}

		public override bool Unary(TwineOperator op, out TwineVar result)
		{
			result = default(TwineVar);
			return false;
		}

		public override bool ConvertTo(Type t, out object result, bool strict = false)
		{
			result = null;
			return false;
		}

		public override ITwineType Duplicate()
		{
			return this.GetCopy();
		}
	}
}

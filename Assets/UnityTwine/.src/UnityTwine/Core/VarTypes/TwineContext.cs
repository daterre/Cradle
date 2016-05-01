using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityTwine
{
	public class TwineContext: TwineType, IDisposable
	{
		public event Action<TwineContext> OnDisposed;
		public bool IsReadOnly { get; private set; }

		public const string EvaluatedVaContextOption = "evaluated-expression";

		List<TwineContext> _appliedContexts = new List<TwineContext>();
		Dictionary<string, object> _appliedValues = new Dictionary<string, object>();
		Dictionary<string, object> _calculatedValues = new Dictionary<string, object>();

		public TwineContext()
		{
		}

		public TwineContext(string name, object value)
		{
			_appliedValues[name] = value;
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

		public object this[string name]
		{
			get
			{
				object val;
				if (_calculatedValues.TryGetValue(name, out val))
					return val;
				else
					return null;
			}
			set
			{
				if (IsReadOnly)
					throw new TwineException("This context is a read-only copy and cannot be modified.");

				_appliedValues[name] = value;
				Recalculate();
			}
		}

		public T Get<T>(string name)
		{
			object val = this[name];
			return val == null ? default(T) : (T)val;
		}


		public object GetApplied(string name)
		{
			object val;
			if (_appliedValues.TryGetValue(name, out val))
				return val;
			else
				return null;
		}

		public TwineContext Apply(TwineVar val)
		{
			if (typeof(TwineContext).IsAssignableFrom(val.GetInnerType()))
				return Apply(val.ConvertValueTo<TwineContext>());

			if (val.ConvertValueTo<bool>())
				return Apply(EvaluatedVaContextOption, val);
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
			newContext[option] = value;
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
				foreach (var entry in context._appliedValues)
					_calculatedValues[entry.Key] = entry.Value;
			}

			foreach (var entry in this._appliedValues)
				_calculatedValues[entry.Key] = entry.Value;
		}

		public void Dispose()
		{
			if (this.OnDisposed != null)
				this.OnDisposed(this);
		}

		public TwineContext GetCopy()
		{
			var copy = new TwineContext();
			copy._calculatedValues = new Dictionary<string, object>(this._calculatedValues);
			copy.IsReadOnly = true;
			return copy;
		}

		public override TwineVar GetMember(TwineVar member)
		{
			return new TwineVar(this[member]);
		}

		public override void SetMember(TwineVar member, TwineVar value)
		{
			this[member] = value.Value;
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

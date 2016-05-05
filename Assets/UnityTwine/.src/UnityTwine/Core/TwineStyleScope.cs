using System;

namespace UnityTwine
{
	public class TwineStyleScope : IDisposable
	{
		internal TwineStyleScopeState State;
		internal TwineStyle Style;
		internal event Action<TwineStyleScope> OnDisposed;

		void IDisposable.Dispose()
		{
			if (OnDisposed != null)
				OnDisposed(this);
		}
	}

	internal enum TwineStyleScopeState
	{
		PendingOpen,
		Open,
		PendingClose,
		Closed
	}
}


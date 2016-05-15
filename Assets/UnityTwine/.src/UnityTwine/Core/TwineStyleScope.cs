using System;

namespace UnityTwine
{
	public class TwineStyleScope : IDisposable
	{
		internal TwineStyle Style;
		internal event Action<TwineStyleScope> OnDisposed;

		void IDisposable.Dispose()
		{
			if (OnDisposed != null)
				OnDisposed(this);
		}
	}
}


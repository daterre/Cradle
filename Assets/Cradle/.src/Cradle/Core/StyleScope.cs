using System;

namespace Cradle
{
	public class StyleScope : IDisposable
	{
		internal StoryStyle Style;
		internal event Action<StyleScope> OnDisposed;

		void IDisposable.Dispose()
		{
			if (OnDisposed != null)
				OnDisposed(this);
		}
	}
}


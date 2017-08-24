using System;

namespace Cradle
{
	public class StyleScope : IDisposable
	{
		internal StyleGroup Group;
		internal event Action<StyleScope> OnDisposed;

		public StyleScope(StyleGroup group)
		{
			this.Group = group;
		}

		void IDisposable.Dispose()
		{
			if (OnDisposed != null)
				OnDisposed(this);
		}
	}
}


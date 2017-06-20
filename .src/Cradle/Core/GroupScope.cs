using System;

namespace Cradle
{
	public class GroupScope : IDisposable
	{
		internal OutputGroup Group;
		internal event Action<GroupScope> OnDisposed;

		public GroupScope(OutputGroup group)
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


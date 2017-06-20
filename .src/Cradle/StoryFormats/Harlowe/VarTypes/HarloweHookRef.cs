using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cradle.StoryFormats.Harlowe
{
	public class HarloweHookRef: VarType
	{
		public string HookName;

		public HarloweHookRef(StoryVar hookName)
		{
			this.HookName = hookName;
		}

		public override StoryVar GetMember(StoryVar member)
		{
			throw new System.NotSupportedException();
		}

		public override void SetMember(StoryVar member, StoryVar value)
		{
			throw new System.NotSupportedException();
		}

		public override void RemoveMember(StoryVar member)
		{
			throw new System.NotSupportedException();
		}

		public override bool Compare(Operator op, object b, out bool result)
		{
			throw new System.NotSupportedException();
		}

		public override bool Combine(Operator op, object b, out StoryVar result)
		{
			throw new System.NotSupportedException();
		}

		public override bool Unary(Operator op, out StoryVar result)
		{
			throw new System.NotSupportedException();
		}

		public override bool ConvertTo(System.Type t, out object result, bool strict = false)
		{
			throw new System.NotSupportedException();
		}

		public override IVarType Duplicate()
		{
			return new HarloweHookRef(this.HookName);
		}
	}
}
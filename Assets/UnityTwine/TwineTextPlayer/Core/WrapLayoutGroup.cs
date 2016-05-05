using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///   <para>Abstract base class for HorizontalLayoutGroup and VerticalLayoutGroup.</para>
/// </summary>
public class WrapLayoutGroup : LayoutGroup
{
	[SerializeField]
	protected float m_SpacingInline;

	[SerializeField]
	protected float m_SpacingLine;

	[SerializeField]
	protected bool m_IsVertical;

	/// <summary>
	///   <para>The spacing to use between layout elements in the layout group.</para>
	/// </summary>
	public float spacingInline
	{
		get
		{
			return this.m_SpacingInline;
		}
		set
		{
			base.SetProperty<float>(ref this.m_SpacingInline, value);
		}
	}

	/// <summary>
	///   <para>The spacing to use between layout elements in the layout group.</para>
	/// </summary>
	public float spacingLine
	{
		get
		{
			return this.m_SpacingLine;
		}
		set
		{
			base.SetProperty<float>(ref this.m_SpacingLine, value);
		}
	}

	public bool isVertical
	{
		get
		{
			return this.m_IsVertical;
		}
		set
		{
			base.SetProperty<bool>(ref this.m_IsVertical, value);
		}
	}

	/// <summary>
	///   <para>Calculate the layout element properties for this layout element along the given axis.</para>
	/// </summary>
	/// <param name="axis">The axis to calculate for. 0 is horizontal and 1 is vertical.</param>
	/// <param name="isVertical">Is this group a vertical group?</param>
	protected void CalcAlongAxis(int axis, bool isVertical)
	{
		float num = (float)((axis != 0) ? base.padding.vertical : base.padding.horizontal);
		float num2 = num;
		float num3 = num;
		float num4 = 0f;
		bool flag = isVertical ^ axis == 1;
		for (int i = 0; i < base.rectChildren.Count; i++)
		{
			RectTransform rect = base.rectChildren[i];
			float minSize = LayoutUtility.GetMinSize(rect, axis);
			float preferredSize = LayoutUtility.GetPreferredSize(rect, axis);
			float num5 = LayoutUtility.GetFlexibleSize(rect, axis);
			if (flag)
			{
				num2 = Mathf.Max(minSize + num, num2);
				num3 = Mathf.Max(preferredSize + num, num3);
				num4 = Mathf.Max(num5, num4);
			}
			else
			{
				num2 += minSize + this.spacingInline;
				num3 += preferredSize + this.spacingInline;
				num4 += num5;
			}
		}
		if (!flag && base.rectChildren.Count > 0)
		{
			num2 -= this.spacingInline;
			num3 -= this.spacingInline;
		}
		num3 = Mathf.Max(num2, num3);
		base.SetLayoutInputForAxis(num2, num3, num4, axis);
	}

	/// <summary>
	///   <para>Set the positions and sizes of the child layout elements for the given axis.</para>
	/// </summary>
	/// <param name="axis">The axis to handle. 0 is horizontal and 1 is vertical.</param>
	/// <param name="isVertical">Is this group a vertical group?</param>
	protected void SetChildrenAlongAxis(int axis, bool isVertical)
	{
		int inlineAxis = isVertical ? 1 : 0;
		int wrapAxis = isVertical ? 0 : 1;
		
		float lineSize = base.rectTransform.rect.size[inlineAxis];

		bool calculatingWrapAxis = isVertical ^ axis == 1;

		float inlineStartOffset = (float)((inlineAxis != 0) ? base.padding.top : base.padding.left);
		float inlineOffset = inlineStartOffset;

		float wrapOffset = (float)((wrapAxis != 0) ? base.padding.top : base.padding.left);
		float nextWrap = 0f;

		//if (base.GetTotalFlexibleSize(inlineAxis) == 0f && base.GetTotalPreferredSize(inlineAxis) < lineSize)
		//{
		//	inlineOffset = base.GetStartOffset(inlineAxis, base.GetTotalPreferredSize(inlineAxis) - (float)((inlineAxis != 0) ? base.padding.vertical : base.padding.horizontal));
		//}

		//float num5 = 0f;
		//if (lineSize > base.GetTotalPreferredSize(inlineAxis) && base.GetTotalFlexibleSize(inlineAxis) > 0f)
		//{
		//	num5 = (lineSize - base.GetTotalPreferredSize(inlineAxis)) / base.GetTotalFlexibleSize(inlineAxis);
		//}

		bool lineBreak = false;

		for (int j = 0; j < base.rectChildren.Count; j++)
		{
			RectTransform rect = base.rectChildren[j];

			float minSizeInline = LayoutUtility.GetMinSize(rect, inlineAxis);
			float preferredSizeInline = LayoutUtility.GetPreferredSize(rect, inlineAxis);
			float flexibleSizeInline = LayoutUtility.GetFlexibleSize(rect, inlineAxis);
			float sizeInline = preferredSizeInline;
			sizeInline += flexibleSizeInline;// *num5;

			float minSizeWrap = LayoutUtility.GetMinSize(rect, wrapAxis);
			float preferredSizeWrap = LayoutUtility.GetPreferredSize(rect, wrapAxis);
			float flexibleSizeWrap = LayoutUtility.GetFlexibleSize(rect, wrapAxis);
			float sizeWrap = preferredSizeWrap;
			sizeWrap += flexibleSizeWrap;// *num5;
			nextWrap = Mathf.Max(nextWrap, sizeWrap);

			if (lineBreak || inlineOffset + sizeInline >= lineSize)
			{
				inlineOffset = inlineStartOffset;
				wrapOffset += nextWrap + this.spacingLine;
				nextWrap = 0f;
				lineBreak = false;
			}

			if (calculatingWrapAxis)
				base.SetChildAlongAxis(rect, wrapAxis, wrapOffset, sizeWrap);
			else
				base.SetChildAlongAxis(rect, inlineAxis, inlineOffset, sizeInline);

			inlineOffset += sizeInline + this.spacingInline;
			lineBreak = rect.GetComponent<WrapLineBreak>() != null;
		}
		
	}

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		CalcAlongAxis(0, this.isVertical);
	}

	/// <summary>
	///   <para>Called by the layout system.</para>
	/// </summary>
	public override void CalculateLayoutInputVertical()
	{
		CalcAlongAxis(1, this.isVertical);
	}

	/// <summary>
	///   <para>Called by the layout system.</para>
	/// </summary>
	public override void SetLayoutHorizontal()
	{
		SetChildrenAlongAxis(0, this.isVertical);
	}

	/// <summary>
	///   <para>Called by the layout system.</para>
	/// </summary>
	public override void SetLayoutVertical()
	{
		SetChildrenAlongAxis(1, this.isVertical);
	}
}


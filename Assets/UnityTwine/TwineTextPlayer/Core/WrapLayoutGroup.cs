using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///   <para>Abstract base class for HorizontalLayoutGroup and VerticalLayoutGroup.</para>
/// </summary>
public class WrapLayoutGroup : LayoutGroup
{
	[SerializeField]
	protected float m_LineBreakMinHeight;

	[SerializeField]
	protected float m_SpacingInline;

	[SerializeField]
	protected float m_SpacingLine;

	[SerializeField]
	protected bool m_IsVertical;

	public float lineBreakMinHeight
	{
		get
		{
			return this.m_LineBreakMinHeight;
		}
		set
		{
			base.SetProperty<float>(ref this.m_LineBreakMinHeight, value);
		}
	}
	
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
		int inlineAxis = isVertical ? 1 : 0;
		int wrapAxis = isVertical ? 0 : 1;
		bool calculatingWrapAxis = isVertical ^ axis == 1;
		float lineSize = base.rectTransform.rect.size[inlineAxis];

		float size = 0f;

		if (!calculatingWrapAxis)
			size = lineSize;
		else
			Calculate(axis, isVertical, (child, ax, position, elemSize) => size = position + elemSize);

		base.SetLayoutInputForAxis(size, size, size, axis);
	}

	/// <summary>
	///   <para>Set the positions and sizes of the child layout elements for the given axis.</para>
	/// </summary>
	/// <param name="axis">The axis to handle. 0 is horizontal and 1 is vertical.</param>
	/// <param name="isVertical">Is this group a vertical group?</param>
	protected void SetChildrenAlongAxis(int axis, bool isVertical)
	{
		Calculate(axis, isVertical, base.SetChildAlongAxis);	
	}

	void Calculate(int axis, bool isVertical, Action<RectTransform, int, float, float> childAction)
	{
		int inlineAxis = isVertical ? 1 : 0;
		int wrapAxis = isVertical ? 0 : 1;
		bool calculatingWrapAxis = isVertical ^ axis == 1;

		float lineSize = base.rectTransform.rect.size[inlineAxis];


		float inlineStartOffset = (float)((inlineAxis != 0) ? base.padding.top : base.padding.left);
		float inlineOffset = inlineStartOffset;

		float wrapOffset = (float)((wrapAxis != 0) ? base.padding.top : base.padding.left);
		float nextWrap = 0f;

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
				wrapOffset += Mathf.Max(nextWrap, this.lineBreakMinHeight) + this.spacingLine;
				nextWrap = 0f;
				lineBreak = false;
			}

			if (childAction != null)
			{
				if (calculatingWrapAxis)
					childAction(rect, wrapAxis, wrapOffset, sizeWrap);
				else
					childAction(rect, inlineAxis, inlineOffset, sizeInline);
			}

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


using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class PointerHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public Action OnPointerEnter;
	public Action OnPointerExit;

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		if (OnPointerEnter != null)
			OnPointerEnter.Invoke();
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		if (OnPointerExit != null)
			OnPointerExit.Invoke();		
	}
}

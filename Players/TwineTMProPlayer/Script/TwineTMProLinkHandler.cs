using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using TMPro;

namespace Cradle.Players
{
	[RequireComponent(typeof(TextMeshProUGUI))]
    public class TwineTMProLinkHandler : MonoBehaviour, IPointerClickHandler
    {
        [Serializable]
        public class LinkClickEvent : UnityEvent<string> { }

        public LinkClickEvent OnLinkClick
        {
            get { return _linkClick; }
            set { _linkClick = value; }
        }

        [SerializeField]
        private LinkClickEvent _linkClick = new LinkClickEvent();

		TextMeshProUGUI _textUI;
        Camera _camera;
        Canvas _canvas;

        void Awake()
        {
            _textUI = gameObject.GetComponent<TextMeshProUGUI>();
            _canvas = gameObject.GetComponentInParent<Canvas>();
            if (_canvas != null)
            {
                if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    _camera = null;
                else
                    _camera = _canvas.worldCamera;
            }
            
        }

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			if (!TMP_TextUtilities.IsIntersectingRectTransform(_textUI.rectTransform, eventData.position, _camera))
				return;
			
			// Check if mouse intersects with any links.
			int linkIndex = TMP_TextUtilities.FindIntersectingLink(_textUI, eventData.position, _camera);

			// Handle new Link selection.
			if (linkIndex != -1)
			{
				// Get information about the link.
				TMP_LinkInfo linkInfo = _textUI.textInfo.linkInfo[linkIndex];

				// Send the event to any listeners. 
				if (OnLinkClick != null)
					OnLinkClick.Invoke(Unescape(linkInfo.GetLinkID()));
			}
		}

		public static string Escape(string linkName)
		{
			return linkName.Replace("\"", "&quot;");

		}

		public static string Unescape(string linkName)
		{
			return linkName.Replace("&quot;", "\"");
		}
	}
}

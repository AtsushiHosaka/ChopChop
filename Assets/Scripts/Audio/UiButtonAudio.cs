using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChopChop.Audio
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class UiButtonAudio : MonoBehaviour, IPointerEnterHandler
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(PlayClick);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlayClick);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && button.interactable)
            {
                AudioService.Instance?.PlayCue(AudioCueId.UiHover);
            }
        }

        private void PlayClick()
        {
            if (button != null && button.interactable)
            {
                AudioService.Instance?.PlayCue(AudioCueId.UiClick);
            }
        }
    }
}

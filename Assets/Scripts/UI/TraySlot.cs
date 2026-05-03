using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TimeTravelBanana.UI
{
    public class TraySlot : MonoBehaviour, IPointerDownHandler
    {
        public int EntryIndex { get; private set; }
        private TrayController controller;
        private Text countText;
        private CanvasGroup group;

        public void Init(TrayController controller, int index, string label)
        {
            this.controller = controller;
            EntryIndex = index;
            UpdateCount(controller.GetStock(index));
            name = "TraySlot_" + label;
        }

        public void Bind(Text countText, CanvasGroup group)
        {
            this.countText = countText;
            this.group = group;
        }

        public void UpdateCount(int stock)
        {
            if (countText != null) countText.text = stock.ToString();
            if (group != null)
            {
                bool enabled = stock > 0 && (controller == null || controller.AcceptsInput);
                group.alpha = enabled ? 1f : 0.4f;
                group.interactable = enabled;
                group.blocksRaycasts = enabled;
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (group == null) return;
            int stock = controller != null ? controller.GetStock(EntryIndex) : 0;
            bool enabled = interactable && stock > 0;
            group.alpha = enabled ? 1f : 0.4f;
            group.interactable = enabled;
            group.blocksRaycasts = enabled;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (controller == null) return;
            controller.OnSlotPressed(this);
        }
    }
}

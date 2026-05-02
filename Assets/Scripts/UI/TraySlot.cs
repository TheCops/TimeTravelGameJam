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
        private Image icon;
        private CanvasGroup group;

        public void Init(TrayController controller, int index, Color iconColor, string label, Sprite iconSprite)
        {
            this.controller = controller;
            EntryIndex = index;
            icon.sprite = iconSprite;
            icon.color = iconColor;
            UpdateCount(controller.GetStock(index));
            name = "TraySlot_" + label;
        }

        public void Bind(Image icon, Text countText, CanvasGroup group)
        {
            this.icon = icon;
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

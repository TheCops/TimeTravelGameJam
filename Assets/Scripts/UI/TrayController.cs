using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TimeTravelBanana.Game;

namespace TimeTravelBanana.UI
{
    public class TrayController : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public string label;
            public int stock;
            public Color iconColor = Color.white;
            public Func<Vector2, PlanningDraggable> spawn;
        }

        [SerializeField] private RectTransform panel;
        [SerializeField] private float slotSize = 110f;
        [SerializeField] private float slotSpacing = 16f;

        private readonly List<Entry> entries = new List<Entry>();
        private readonly List<TraySlot> slots = new List<TraySlot>();
        private readonly List<int> stocks = new List<int>();
        private bool acceptsInput = true;
        private GameStateController gameState;
        private Camera worldCamera;

        public bool AcceptsInput => acceptsInput;

        public void Configure(RectTransform panelRect, GameStateController state, Camera cam, IEnumerable<Entry> initialEntries)
        {
            if (panelRect != null) panel = panelRect;
            gameState = state;
            worldCamera = cam;
            entries.Clear();
            stocks.Clear();
            foreach (var e in initialEntries)
            {
                entries.Add(e);
                stocks.Add(e.stock);
            }
            BuildSlots();
            if (gameState != null) gameState.OnStateChanged += HandleStateChanged;
            HandleStateChanged(gameState != null ? gameState.State : GameState.Planning);
        }

        private void OnDestroy()
        {
            if (gameState != null) gameState.OnStateChanged -= HandleStateChanged;
        }

        public int GetStock(int index) => (index >= 0 && index < stocks.Count) ? stocks[index] : 0;

        public void OnSlotPressed(TraySlot slot)
        {
            if (!acceptsInput) return;
            int idx = slot.EntryIndex;
            if (idx < 0 || idx >= entries.Count) return;
            if (stocks[idx] <= 0) return;

            Vector2 mouseWorld = MouseWorld();
            var draggable = entries[idx].spawn(mouseWorld);
            if (draggable == null) return;

            stocks[idx]--;
            slot.UpdateCount(stocks[idx]);

            int capturedIdx = idx;
            draggable.OnRequestDestroy += d => HandleDraggableDestroyed(d, capturedIdx);

            if (gameState != null) gameState.RegisterDraggable(draggable);
            draggable.BeginDragFromSpawn();
        }

        private void HandleDraggableDestroyed(PlanningDraggable d, int entryIdx)
        {
            if (d == null) return;
            if (entryIdx >= 0 && entryIdx < stocks.Count)
            {
                stocks[entryIdx]++;
                if (entryIdx < slots.Count) slots[entryIdx].UpdateCount(stocks[entryIdx]);
            }
            if (gameState != null) gameState.UnregisterDraggable(d);
            Destroy(d.gameObject);
        }

        private Vector2 MouseWorld()
        {
            var cam = worldCamera != null ? worldCamera : Camera.main;
            if (cam == null) return Vector2.zero;
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse == null) return cam.transform.position;
            Vector3 screen = mouse.position.ReadValue();
            Vector3 world = cam.ScreenToWorldPoint(screen);
            world.z = 0f;
            return world;
        }

        private void BuildSlots()
        {
            if (panel == null) return;
            for (int i = panel.childCount - 1; i >= 0; i--) Destroy(panel.GetChild(i).gameObject);
            slots.Clear();

            for (int i = 0; i < entries.Count; i++)
            {
                var slot = CreateSlot(panel, entries[i], i);
                slots.Add(slot);
            }
        }

        private TraySlot CreateSlot(RectTransform parent, Entry entry, int index)
        {
            var go = new GameObject("Slot", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(TraySlot));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(slotSize, slotSize);
            rt.anchoredPosition = new Vector2(slotSpacing, -(slotSpacing + index * (slotSize + slotSpacing)));

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.1f, 0.9f);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            var iconRt = (RectTransform)iconGo.transform;
            iconRt.SetParent(rt, false);
            iconRt.anchorMin = iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.sizeDelta = new Vector2(slotSize * 0.6f, slotSize * 0.6f);
            iconRt.anchoredPosition = new Vector2(0f, 8f);
            var icon = iconGo.GetComponent<Image>();
            icon.sprite = SpriteFactory.WhiteSquare;
            icon.color = entry.iconColor;

            var countGo = new GameObject("Count", typeof(RectTransform), typeof(Text));
            var countRt = (RectTransform)countGo.transform;
            countRt.SetParent(rt, false);
            countRt.anchorMin = new Vector2(1f, 0f);
            countRt.anchorMax = new Vector2(1f, 0f);
            countRt.pivot = new Vector2(1f, 0f);
            countRt.sizeDelta = new Vector2(48f, 36f);
            countRt.anchoredPosition = new Vector2(-6f, 4f);
            var count = countGo.GetComponent<Text>();
            count.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            count.fontSize = 28;
            count.alignment = TextAnchor.LowerRight;
            count.color = Color.white;

            var slot = go.GetComponent<TraySlot>();
            var group = go.GetComponent<CanvasGroup>();
            slot.Bind(icon, count, group);
            slot.Init(this, index, entry.iconColor, entry.label, SpriteFactory.WhiteSquare);
            return slot;
        }

        private void HandleStateChanged(GameState s)
        {
            acceptsInput = (s == GameState.Planning);
            for (int i = 0; i < slots.Count; i++) slots[i].SetInteractable(acceptsInput);
        }
    }
}

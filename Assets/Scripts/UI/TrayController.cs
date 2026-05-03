using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TimeTravelBanana.Game;

namespace TimeTravelBanana.UI
{
    public class TrayController : MonoBehaviour
    {
        public class Entry
        {
            public string label;
            public int stock;
            public Color iconColor = Color.white;
            public Func<Vector2, PlanningDraggable> spawn;
        }

        [SerializeField] private int trampolineStock = 3;
        [SerializeField] private int blockStock = 3;
        [SerializeField] private int rocketStock = 3;
        [SerializeField] private float panelWidth = 150f;
        [SerializeField] private float slotSize = 110f;
        [SerializeField] private float slotSpacing = 16f;

        private readonly List<Entry> entries = new List<Entry>();
        private readonly List<TraySlot> slots = new List<TraySlot>();
        private readonly List<int> stocks = new List<int>();

        private RectTransform panel;
        private Camera worldCamera;
        private bool acceptsInput = true;

        public bool AcceptsInput => acceptsInput;

        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) { Debug.LogWarning("TrayController: no Canvas in scene; tray UI will not be built."); return; }

            worldCamera = Camera.main;
            panel = BuildPanel(canvas.transform);
            entries.Add(new Entry { label = "Trampoline", stock = trampolineStock, iconColor = new Color(0.3f, 0.7f, 1f),  spawn = pos => Spawner.Trampoline(pos) });
            entries.Add(new Entry { label = "Block",      stock = blockStock,      iconColor = new Color(0.85f, 0.7f, 0.4f), spawn = pos => Spawner.Block(pos) });
            entries.Add(new Entry { label = "Rocket",     stock = rocketStock,     iconColor = new Color(0.95f, 0.45f, 0.35f), spawn = pos => Spawner.Rocket(pos) });
            for (int i = 0; i < entries.Count; i++) stocks.Add(entries[i].stock);
            BuildSlots();
        }

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnPlacingState  += HandlePlacing;
                gm.OnPlayingState  += HandlePlaying;
                gm.OnResolvedState += HandlePlaying;
                gm.OnPausedState   += HandlePlaying;
                ApplyAcceptsInput(gm.State == GameState.Placing);
            }
        }

        private void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnPlacingState  -= HandlePlacing;
            gm.OnPlayingState  -= HandlePlaying;
            gm.OnResolvedState -= HandlePlaying;
            gm.OnPausedState   -= HandlePlaying;
        }

        private void HandlePlacing() => ApplyAcceptsInput(true);
        private void HandlePlaying() => ApplyAcceptsInput(false);

        private void ApplyAcceptsInput(bool v)
        {
            acceptsInput = v;
            for (int i = 0; i < slots.Count; i++) slots[i].SetInteractable(v);
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

        private RectTransform BuildPanel(Transform canvasTransform)
        {
            var go = new GameObject("TrayPanel", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(canvasTransform, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(panelWidth, 0f);
            rt.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
            rt.SetAsFirstSibling();
            return rt;
        }

        private void BuildSlots()
        {
            if (panel == null) return;
            for (int i = panel.childCount - 1; i >= 0; i--) Destroy(panel.GetChild(i).gameObject);
            slots.Clear();
            for (int i = 0; i < entries.Count; i++) slots.Add(CreateSlot(panel, entries[i], i));
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
    }
}

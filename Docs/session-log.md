# Session log

Append-only. Newest entries at the bottom. Each entry ends with a prompt for the next session.

---

## 2026-05-02 — Design lock + scene plan

**Who:** DJ + Claude (dj-ui branch)

**Context coming in:** Repo had a partially-built timeline/bake/scrub system from the starter commits. `gamedoc.md` was a rough draft. Game jam clock at ~24h to submission.

**Decisions made this session (now in `Docs/gamedoc.md`):**
- Killed bake and manual scrub. World runs live physics during Play.
- Time-travel hook is a global `worldTimeRate` set by special blocks the banana flies through. Slow (0.3), fast (2.5), reverse (-1.0). Last block wins, trigger-based, no duration.
- Cyclical objects (rocket, balloon, future moving platform) read `worldTimeRate` and scale their behavior by it. Negative rate flips their direction.
- States collapsed to `Edit -> Playing -> Resolved`. No View Mode, no Bake.
- UCH-style left-side tray with stock counts for placement UX.
- One level. Win = banana enters fruit bowl trigger. Lose = level timer hits zero.
- Main menu: Play / Instructions / Quit. Two scenes: `MainMenu.unity` (build index 0) and `Level1.unity`.
- Submission target: WebGL on itch.

**Repo state at end of session:**
- `Docs/gamedoc.md` is the source of truth for design.
- `Docs/session-log.md` (this file) is append-only.
- Branch `dj-ui`, just rebased on `main`. The rebase pulled in commit `b5f44a0 disabling scene bootstrap`.
- `Assets/Scripts/Game/SceneBootstrap.cs` is **broken on purpose**. The `[RuntimeInitializeOnLoadMethod]` attribute was removed and the method was renamed to `Awake()` on a `static class`, which never fires under Unity. Nothing currently builds the level at runtime. `SampleScene.unity` is empty. This is intentional: we're moving to scene-based setup and out of code-spawned everything.
- Dead-code warning still applies: `TimelineSnapshotBuffer`, `RigidbodyTimelineRecorder`, `TransformTimelineRecorder`, `TimelineManager.BakeFor()`, recording/scrubbing modes are unused. Don't refactor under deadline; just stop adding to them.

**What's blocking us next:** No playable scene exists. Bootstrap is disabled, `SampleScene` is empty, `MainMenu` doesn't exist, `Level1` doesn't exist. Until there's a level scene that loads and runs, nobody else on the team can build against it.

**Owner of next session's work:** DJ + Claude (us). Scene setup is on our plate.

---

### Prompt for next session

You're picking up the Time-Travel Banana jam project mid-flight. Read `Docs/gamedoc.md` for design and the entry above for current state. Your job this session is to **stand up the two-scene structure and get a playable Level1 running again**.

Concrete tasks, in order:

1. **Create `Assets/Scenes/MainMenu.unity`.** Empty scene with a Canvas, three UI buttons (Play, Instructions, Quit), and a title. Play loads `Level1`. Instructions opens a panel with placeholder text and a Back button. Quit calls `Application.Quit()` (no-op in editor, fine).
2. **Create `Assets/Scenes/Level1.unity`.** This replaces `SampleScene.unity` — feel free to delete `SampleScene.unity` once `Level1` is wired up.
3. **Re-enable scene setup, but scoped to Level1.** Convert `SceneBootstrap` from a broken `static class` with a fake `Awake()` into either:
   - a `MonoBehaviour` you drop into `Level1.unity` and that runs in `Start()`, or
   - actual scene objects placed in the editor (preferred for anything the artist will eventually skin).
   The runtime-spawn approach is faster for now; the editor-placed approach is better long-term. Pick one and commit.
4. **Add both scenes to Build Settings** (File -> Build Settings). MainMenu at index 0, Level1 at index 1.
5. **Wire `SceneManager.LoadScene` calls** on the menu buttons and on a "Main Menu" button in the Level1 win/lose overlay (overlay itself can be a stub for this session — just empty hooks ready for P0.5 to fill in).
6. **Sanity-check the dead-code situation.** `TimelineManager` is still referenced by `SceneBootstrap`. If you keep the runtime-spawn path, leave the reference; if you move to editor-placed, decide whether `TimelineManager` even needs to exist anymore (it might still own `worldTimeRate` plumbing in the next pass — check `gamedoc.md` "Tech notes" for the plan).
7. **Verify in editor.** Open `MainMenu.unity`, press Play, click Play button, land in Level1 with at least the banana, launcher, walls, floor, and bucket visible. Don't worry about gameplay correctness yet — that's the next session.

Don't do this session:
- Tray UX (P1 owns this).
- `worldTimeRate` plumbing or time blocks (P2 owns this).
- Win/lose detection logic (just stub the overlay hooks).
- Art (artist will swap sprites later).

When you're done, append a new entry to `Docs/session-log.md` describing what shipped, what state the scenes are in, any gotchas hit, and a prompt for whoever picks up next.

---

## 2026-05-02 (later) — Scene scaffolding (code side)

**Who:** DJ + Claude (dj-ui)

**What shipped (code only — DJ still needs to click two menu items in Unity to materialize the scenes):**

- `Assets/Scripts/Game/SceneBootstrap.cs` rewritten as a real `MonoBehaviour`. `Awake()` builds floor, ceiling (bouncy), left/right walls, bucket, banana, launcher, trampoline, rocket, `TimelineManager`, `ObjectTimeController`, `GameStateController`. Bounds default to x∈[-10,10], y∈[-4,5] and are `[SerializeField]`-tunable on the component. `bakeDuration` still passed through to `GameStateController` — left intact per the dead-code call-out in the previous entry; we did not refactor `LaunchSequence`'s bake/playback path.
- `Assets/Scripts/UI/MainMenuController.cs`: Play / Instructions / Quit, with an instructions panel that toggles a placeholder text + Back button. Play loads scene named `Level1`.
- `Assets/Scripts/UI/LevelOverlay.cs`: stub overlay with `ShowWin()` / `ShowLose()` hooks. Holds win and lose panels, accepts arrays of "main menu" and "retry" buttons so both panels' buttons can resolve to the same handlers. Not yet wired to the `Banana.OnWin` / `OnLose` events — that hookup is P0.5's job.
- `Assets/Scripts/Editor/SceneSetupTool.cs`: editor menu under `TimeTravelBanana → Scenes`. Creates `MainMenu.unity` and `Level1.unity`, builds the Canvas + EventSystem + buttons + panels, drops the `SceneBootstrap` and `LevelOverlay`/`MainMenuController` GameObjects, wires their `[SerializeField]` references via `SerializedObject`, saves the scene file, and registers it in Build Settings at the right index.

**What DJ must do in Unity (once this branch is checked out and Unity has imported):**

1. Open Unity. Top menu: **TimeTravelBanana → Scenes → Create Both Scenes**. This creates `Assets/Scenes/MainMenu.unity` and `Level1.unity`, and registers them in Build Settings as index 0 and 1.
2. Delete `Assets/Scenes/SampleScene.unity` (and its `.meta`).
3. Open `MainMenu.unity`, press Play, click **PLAY**, confirm `Level1` loads with the banana, launcher, trampoline, rocket, walls, floor, bucket visible.
4. Commit.

**State of dead code:** Untouched. `TimelineManager`, `RigidbodyTimelineRecorder`, `BakeFor`, `ObjectTimeController.BeginPlayback`, `GameStateController.LaunchSequence`'s bake-and-replay path are all still wired through `SceneBootstrap`. They will run when SPACE is pressed and probably do something visually janky under the new live-physics design intent. Don't fix here; that's the worldTimeRate session.

**Gotchas / decisions:**

- Took the runtime-spawn path (option A from the previous prompt), not editor-placed objects. Reason: faster to get back to playable, and the artist's sprite swap can happen later via `SpriteFactory` or by editor-placing the level once art lands.
- The original bootstrap had no floor/walls/ceiling — the banana would have fallen forever. Added them here. The ceiling has a bouncy `PhysicsMaterial2D` (0.85 bounciness) per the gamedoc "ceiling as level constraint" idea.
- Did not commit `.unity` scene files from CLI. Hand-rolling Unity scene YAML is brittle and would have eaten time. The editor menu tool is the safe substitute — DJ runs it once in Unity and gets clean scenes with proper GUIDs.
- `LevelOverlay` does not yet self-subscribe to banana events. The session prompt explicitly said "stub the overlay hooks" so we left `ShowWin()`/`ShowLose()` as public methods with no caller.
- **Input System gotcha:** project has `Active Input Handling` set to the new Input System package (visible in `PlanningDraggable.cs`'s `UnityEngine.InputSystem` usage). The editor tool initially added the legacy `StandaloneInputModule` to the EventSystem, which spammed `InvalidOperationException` 1k+ times per frame. Fixed by swapping to `InputSystemUIInputModule` from `UnityEngine.InputSystem.UI`. If you ever extend the editor tool to make a third UI scene, use `InputSystemUIInputModule`.
- **Scene convention:** `Level1.unity` is the canonical level scene. `SampleScene.unity` is Clarke's sandbox — leave it alone, do not delete it. All level work goes in `Level1`. If we need throwaway scenes later, name them `Sandbox*.unity` and keep them out of Build Settings.

---

### Prompt for next session

You're picking up the Time-Travel Banana jam. Read `Docs/gamedoc.md` for design and the entries above for current state. **Confirm with DJ that the scene-creation menu items have been run in Unity and `MainMenu.unity` + `Level1.unity` exist on disk before starting.** If not, run them yourself (TimeTravelBanana → Scenes → Create Both Scenes) and commit the scene files.

Your job this session: **rip out the bake/scrub timeline path and replace it with live-physics + a global `worldTimeRate`** (P2 work from the previous session's deferred list). Concretely:

1. **Add `worldTimeRate` plumbing.** Pick a home — most likely `ObjectTimeController` (already exists, already references the timeline). Single `public static float WorldTimeRate { get; private set; } = 1f;` or similar. Default 1.0. Reset to 1.0 on level reset / `EnterPlanning`.
2. **Rewrite `GameStateController.LaunchSequence`** to skip baking. New flow: disable draggables, call `launcher.Launch()`, set state to `Playing`. The banana's rigidbody just runs against live physics. Delete the call to `timeline.BakeFor` and the call to `timeController.BeginPlayback`.
3. **Make cyclical objects read `worldTimeRate`.** Rocket's `ConstantForce2D` and any future moving platform/balloon need to scale their behavior by `worldTimeRate` (and flip direction when negative). The trampoline is passive — leave it. For now this means giving the rocket its own `RocketBehaviour` script that applies force itself instead of relying on `ConstantForce2D`, scaled by `worldTimeRate`.
4. **Decide what to do with `TimelineManager` / `RigidbodyTimelineRecorder` / `BakeFor`.** If nothing else needs them, delete them — including their references in `SceneBootstrap`. If you're nervous, `#if false` them out. Don't leave them quietly running.
5. **Add a level timer.** Simple `[SerializeField] float levelDurationSeconds = 30f;` on `GameStateController` (or a new `LevelTimer` component). Counts down only during `Playing`. When it hits zero, fire the lose path (currently `HandleLose`). Show the timer in the level UI — quick `Text` element in the Canvas top-center is enough.
6. **Wire `LevelOverlay` to the banana events.** `LevelOverlay` should subscribe to `launcher.Banana.OnWin → ShowWin` and `OnLose → ShowLose`. Either via `SerializeField GameStateController` reference, or by `FindFirstObjectByType` in `Start()`.
7. **Sanity-check.** Press Play in `MainMenu`, click PLAY, land in `Level1`, press SPACE. Banana should fly under live physics. Land it in the bucket → win panel shows. Miss it / time runs out → lose panel shows. Main Menu button on either panel goes back. Retry reloads `Level1`.

Don't do this session:
- Time blocks themselves (slow/fast/reverse triggers). Get the global `worldTimeRate` plumbing in place first; the actual block prefabs and triggers are the session after this.
- Tray UX (P1).
- Art (artist).

When you're done, append a new entry describing what shipped, what state the gameplay loop is in, any gotchas, and a prompt for the next session — likely the time-block triggers.

---

## 2026-05-02 (later still) — Tray UX + playtest mode

**Who:** DJ + Claude (dj-ui)

**What shipped:**

- **Tray UX (UCH-style, runtime-built).**
  - Left-edge `TrayPanel` with two slots: Trampoline (blue, x3) and Block (tan, x3). Stocks fixed per level, no refill except via destroy refund.
  - One-click-and-drag spawn: pointer down on slot → world object instantiates under cursor → user drags to place → release drops it. Object stays Kinematic so it doesn't fall under gravity once placed.
  - Grid snap (`snapSize = 0.5` world units, on each `PlanningDraggable`).
  - Scroll-wheel rotation while dragging (15° per tick, accumulator-based so trackpad smooth-scroll doesn't spin objects to oblivion).
  - Right-click on a placed obstacle (or while dragging) → destroy + refund stock to its tray slot.
  - Static `activeDragger` guard in `PlanningDraggable` prevents two draggables from arbitrating simultaneously, so right-click hits only the dragged one when objects overlap.
  - Tray locks (slots greyed out, non-interactable) when game state != Planning.
- **Playtest mode.**
  - New `Playtester` MonoBehaviour. Subscribed to `GameStateController.OnStateChanged`. While `State == Playing`, spawns one fresh `Banana` per second from `Launcher.SpawnAndLaunchInstance(autoDestroyOnResolve: true)`.
  - FIFO queue capped at 10. New spawns evict the oldest active banana. Playtest bananas self-destroy on bucket/timeout/offscreen (no win/lose UI fires; that's the "real game mode" path which is currently unbuilt).
  - Top-center `PLAY` button on the Canvas, label flips to `STOP` while playtesting. SPACE also toggles. R returns to Planning while Playing.
- **Code cleanup.** This batch went further than the prior session-prompt deferred — necessary because the bake/replay path was actively blocking the playtest spawn loop.
  - Ripped `BakeFor` + `BeginPlayback` calls out of `GameStateController`. Dropped `TimelineManager` and `ObjectTimeController` from `SceneBootstrap` entirely (no callers). They still exist as files in `Assets/Scripts/Timeline/` and `Assets/Scripts/Game/ObjectTimeController.cs` but nothing references them.
  - `GameState` enum trimmed to just `Planning` and `Playing`. `Baking` and `Resolved` gone.
  - `GameStateController` no longer holds `timeline`, `timeController`, `bakeDuration`, `subscribedToBanana`, or banana-event handlers. `Configure(launcher, draggables)` is the new signature.
  - Single-banana scene flow gone. `SceneBootstrap` no longer creates a banana. `Launcher` no longer holds a banana ref or has a no-arg `Launch()`. `Banana` no longer self-deactivates in `Awake` and lost `ResetBanana` / `startPosition` / `startRotation` (unused once playtest spawns fresh instances each fire).
  - New `BananaFactory.Create(pos)` is the canonical banana-spawn helper.

**Files in play:** `Banana.cs`, `BananaFactory.cs` (new), `Launcher.cs`, `GameStateController.cs`, `SceneBootstrap.cs`, `PlanningDraggable.cs`, `Spawner.cs` (new earlier this session), `UI/TrayController.cs`, `UI/TraySlot.cs`, `UI/Playtester.cs` (new), `UI/LevelOverlay.cs`.

**Gotchas / decisions:**

- The next-session prompt above (worldTimeRate plumbing, level timer, win/lose wiring) is now **partially obsolete**. The bake rip-out is done. `LaunchSequence` doesn't exist anymore. The deferred items now look more like: (a) build worldTimeRate + cyclical objects + time-block triggers, (b) build a real "single-banana mission mode" that wires win/lose to `LevelOverlay`. The level-timer item only matters once mission mode exists.
- `LevelOverlay` is still wired to win/lose panels via the editor tool, but nothing currently drives `ShowWin()` / `ShowLose()`. Playtest bananas auto-destroy without firing the overlay. That's intentional for now.
- `Assets/Scripts/Timeline/*` and `Assets/Scripts/Game/ObjectTimeController.cs` are now fully orphaned. Safe to delete in the next cleanup pass — none of the live game scripts reference them. Kept them around because they have `.meta` files and removing under Unity is cleaner from inside the editor.
- Snap is hardcoded at 0.5 world units. Rotation step at 15°. Both are `[SerializeField]` on `PlanningDraggable` so per-instance overrides are possible if a particular obstacle wants finer placement.

---

### Prompt for next session

You're picking up the Time-Travel Banana jam. Read `gamedoc.md` and the entries above before starting. Confirm Level1 still plays — open `MainMenu`, press Play, click `PLAY` in Level1, watch bananas arc out of the launcher. Place a trampoline mid-arc, watch the bananas bounce. Right-click trampoline to refund. If any of that's broken, fix it before doing new work.

The big remaining gameplay mechanic is the **time-rate system from `gamedoc.md`** — special blocks the banana flies through that change a global `worldTimeRate` (slow 0.3, fast 2.5, reverse -1.0). Concretely:

1. **Plumb `worldTimeRate`.** New static or singleton — likely `WorldTime.Rate` (a static class). Default 1.0. Resets to 1.0 every time `GameStateController` enters Planning. While `Playing`, it's mutated by trigger blocks.
2. **Add cyclical objects that respond.** Rocket and balloon from the gamedoc. They tick their own behavior scaled by `WorldTime.Rate` (negative rate flips direction). Add them to the tray, give each a stock count. The trampoline stays passive (already correct).
3. **Time blocks.** Three trigger-block variants (slow / fast / reverse). On `OnTriggerEnter2D` with a Banana, set `WorldTime.Rate` to the block's value. Last block wins. No duration — the rate stays until another block changes it. Add them to the tray with stock counts.
4. **Test in playtest.** Place a fast block, watch bananas accelerate. Place a reverse block, watch the rocket flip. Place a chain of slow→fast and confirm last-block-wins.
5. **Real game mode (probably the session after).** Single-banana fire on player input, wire `Banana.OnWin → LevelOverlay.ShowWin` and `OnLose → LevelOverlay.ShowLose`, level timer, success/fail UI. Skip if running short on jam time — playtest mode is enough for the demo.

Don't do this session:
- Art (artist).
- Audio.
- Multi-level support.

When you're done, append a session-log entry as usual.

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

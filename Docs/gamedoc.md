# Time-Travel Banana

Mini Jame Gam #54. Theme: **Time Travel**. Special object: **Banana**. Submission due May 3, 5:30 PM. ~24 hours left.

Team: 2.5 programmers (one is also designing) + 1 artist. Submission target: WebGL build on itch.

Design inspiration: Ultimate Chicken Horse for the place-and-watch loop and the side tray UX.

## Pitch

Place wacky objects in a 2D scene, then launch a slow-moving banana out of a pipe. The banana flies through a Rube Goldberg world full of cyclical contraptions and scattered "time blocks" that bend how time flows for everything around the banana. Land the banana in the fruit bowl before the level timer runs out.

## Core loop

1. **Edit.** Time is frozen. Player drags placeables out of a left-side tray (UCH-style) into the play area and arranges them. Per-tile stock counts limit how many of each.
2. **Play.** Player presses Space (or Launch button). World objects start their live behaviors, banana fires from the launcher. No bake step, no recording, no scrub. Just live physics + parametric object behaviors that read a global `worldTimeRate`.
3. **Resolve.** Banana enters the fruit bowl -> win. Level timer hits zero -> lose. R or Retry button returns to Edit.

## The time-block hook

The thing that earns the theme.

A global `worldTimeRate` (default 1.0) modulates every cyclical object's behavior. Rocket thrust, balloon rise speed, moving platform direction, anything on a recurring or directional behavior reads this rate. The banana itself does not; it stays on real time, a temporal tourist.

Time blocks are placeable tiles in the tray. When the banana enters one's trigger, it sets `worldTimeRate` to that block's value. The effect persists until the banana enters another block (trigger-based, not duration-based).

- **Slow block.** Sets `worldTimeRate = 0.3`.
- **Fast block.** Sets `worldTimeRate = 2.5`.
- **Reverse block.** Sets `worldTimeRate = -1`. Cyclical objects flip direction: rockets thrust down, balloons sink, platforms reverse drift.

Stacking is trivial because it's "last block wins." No queues, no math.

Reactive objects (trampoline) don't read the rate; they just bounce. Static objects (walls, ceiling, fruit bowl) don't care.

## States

`Edit -> Playing -> Resolved`. No Bake state. No scrub.

## Controls

- Mouse: drag from left tray into the play area, drag to reposition, drag back to tray (or right-click) to remove.
- Space or Launch button: start Play.
- R or Retry button: reset to Edit.

## Objects

Already in:
- Banana. Dynamic rigidbody, gravity-affected, not on the timeline. Slow and bouncy.
- Trampoline. Dynamic, very bouncy material, falls under gravity, recorded.
- Rocket. Dynamic, constant upward thrust, recorded.
- Floor and side walls. Static.

Needs to be built (UI first, mechanics second):

UI track:
1. Main menu scene: Play, Instructions, Quit.
2. Instructions screen: text + simple diagram, Back button.
3. Level scene HUD: level timer, retry button.
4. Win/lose overlay with Retry and Main Menu buttons.
5. Side tray UI. Each tile shows the object icon + remaining stock count. Drag a tile into the play area to spawn an instance; drag back to tray (or right-click) to return it to stock. Disabled tiles when stock is 0.

Mechanics track:
6. `worldTimeRate` plumbed through `ObjectTimeController` (or successor). Default 1.0.
7. Time block trigger. On banana enter, set `worldTimeRate`. Slow / fast / reverse variants. Last block wins.
8. Cyclical-object refactor. Rocket and balloon read `worldTimeRate` to scale (and flip sign on) their thrust/rise behavior. Trampoline is reactive only, doesn't care.
9. Balloon placeable. Rises at constant speed scaled by `worldTimeRate`, soft bounce material.
10. Bouncy ceiling. Top static collider with bouncy material.
11. Fruit bowl goal. Open-top static box with non-bouncy interior material; inner trigger fires win on banana enter.
12. Level timer. Countdown visible in HUD, on zero -> lose.
13. Audio: ~5 SFX, optional music loop.

Stretch:
- Moving platform placeable (drifts horizontally, reverses on collision, direction flips with `worldTimeRate`).
- Visual feedback on time-rate change (screen tint, particles on banana, world color shift).
- Sound pitch shift tied to `worldTimeRate`.
- Multiple levels.

## Tech notes

- **Dead code warning.** With bake and scrub gone, the timeline-recording infra is unused: `TimelineSnapshotBuffer`, `RigidbodyTimelineRecorder`, `TransformTimelineRecorder`, `TimelineManager.BakeFor()`, `Recording`/`Scrubbing` modes. Don't refactor under deadline. Stop adding new recorders. New cyclical objects skip the recorder entirely and just read `worldTimeRate`.
- `ObjectTimeController` becomes the home of `worldTimeRate`. Strip out the keyboard arrow handling. Add a `SetRate(float)` method called by time-block triggers. Default rate is 1.0; resets to 1.0 on Edit entry.
- Cyclical objects multiply their per-frame velocity, force, or position delta by `worldTimeRate`. A negative rate flips direction naturally.
- `SceneBootstrap` currently builds everything via `[RuntimeInitializeOnLoadMethod]`, which fires for every scene. Move it to a `MonoBehaviour` placed in the Level scene's hierarchy so it doesn't try to build a level on the Main Menu.
- `Banana.cs` lose conditions (`maxFlightTime`, offscreen) get replaced by the level timer. Win is fruit-bowl trigger entry.
- `GameStateController` needs to fire UI events instead of `Debug.Log`.

## Scenes and project structure

Two scenes is the right answer:
- `Assets/Scenes/MainMenu.unity` (build index 0)
- `Assets/Scenes/Level1.unity` (build index 1)

Add both via File -> Build Settings. Index 0 is what the built game opens to.

Dev workflow:
- To work on the level: open `Level1.unity`, press Play. Unity starts from whatever scene is open in the editor.
- To test the menu flow: open `MainMenu.unity`, press Play, click Play button, scene loads Level1.
- Optional: Project Settings -> Editor -> "Play Mode Start Scene" forces every Play to start from MainMenu. Skip this for now; it slows iteration.

Scene transitions: `UnityEngine.SceneManagement.SceneManager.LoadScene("Level1")` from a button OnClick. Same to return: `LoadScene("MainMenu")`.

Persistent state across scenes (e.g. "did the player just win"): for one level you don't need any. If a stretch level select shows up, use a small `static` class or a `DontDestroyOnLoad` GameStateController seeded in MainMenu.

A common gotcha: `SceneBootstrap`'s `[RuntimeInitializeOnLoadMethod]` will fire on the Main Menu scene too and try to build a level on top of it. Delete that attribute, make Bootstrap a `MonoBehaviour`, drop one in `Level1.unity`. Done.

## Scope for the next 24h

Locked:
- One level.
- Tray-based placement UX with stock counts.
- Three time-block types: slow, fast, reverse.
- Trampoline, rocket, balloon, time-blocks.
- Bouncy ceiling, non-bouncy fruit bowl interior.
- Win on fruit-bowl entry, lose on level-timer expiry.
- Main menu (Play / Instructions / Quit) + win/lose overlay with Retry.
- Two scenes: MainMenu and Level1. WebGL itch build.
- Audio TBD.

Order of work:
- P0.5: main menu scene, instructions screen, scene transitions, win/lose overlay, level timer HUD. This is the critical path so the loop is real.
- P1: tray UX. Prefab-ify trampoline / rocket / balloon / time-blocks behind a common placeable interface. Stock counts on tiles.
- P2 (designer/programmer): `worldTimeRate` plumbing, time-block triggers, refactor rocket and balloon to read the rate. One slow block end-to-end first as the vertical slice, then fast and reverse fall out of the same wiring.
- Artist: banana, trampoline, rocket, balloon, time-blocks (slow, fast, reverse — visually distinct), fruit bowl, launcher pipe, ceiling, walls, background, title art, button states. Banana rotting frames are dropped (timer is a HUD bar, not a visual on the banana).

## Open questions

Numbers to tune in flight, not blockers:

- Stock counts. Starting suggestion: 3 trampolines, 2 rockets, 2 balloons, 2 slow, 1 fast, 1 reverse. Adjust after first playtest.
- Level timer length. Suggest 20s. Long enough that a reverse-block run can still finish.
- `worldTimeRate` values. Slow 0.3, fast 2.5, reverse -1.0 are starting points.
- Launcher angle and speed. Expose in inspector, designer tweaks last.
- Instructions text vs labelled diagram. Diagram reads faster but costs artist time. Default to text for MVP, diagram if there's slack.

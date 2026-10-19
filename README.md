# CS464 Assignment 02 · Muhammad Abdullah · BSCS25017

- **Video:** https://drive.google.com/file/d/10hJGrjb7Yb51Stri3ljFarEZVKw3o5Z8/view?usp=sharing
- **Input:** Legacy Input with Active Input Handling set to Both.
- **Ground check:** CheckSphere at Feet on the Ground layer, with a visible gizmo.
- **Camera:** CameraFollow in LateUpdate, using SmoothDamp and velocity-based look-ahead.
- **Juice:** Landing squash using an OutBack curve and a coroutine, triggered after more than 0.1 seconds airborne.

## Tuning

Gravity is **9.81 m/s²**.

| Setting | BlockOut1: arena | BlockOut2: maze | BloCkOut3: training | Why this value matters |
|---|---:|---:|---:|---|
| Speed | 5 m/s | 5 m/s | 5 m/s | Keeps movement fast but easy to control. |
| Jump height | 2.0 m | 0.9 m | 1.8 m | Gives enough height to cross gaps without making jumps too easy. |
| Time to apex | 0.639 s | 0.428 s | 0.606 s | A 1.8 m jump takes about 0.6 seconds to reach its peak. |
| Fall multiplier | 2.2× | 2.0× | 2.2× | Makes the player fall faster and makes jumps feel more responsive. |
| Coyote time | 0.10 s | 0.10 s | 0.10 s | Gives players a little room for timing mistakes. |
| Jump buffer | 0.12 s | 0.12 s | 0.12 s | Helps the player jump immediately after touching the ground. |
| Camera smoothing (`smoothTime`) | 0.50 s | 0.30 s | 0.20 s | The arena uses a more gradual response; the training course catches up more quickly. |
| Look-ahead | 0.10 s | 0.35 s | 0.20 s | Keeps the player visible while showing some of the path ahead. |

## Four answers

### 1. The jump, in numbers

With gravity `g = 9.81 m/s²` and mass `m = 1 kg`, launch speed is `sqrt(2gh)` and jump impulse is `m × launch speed`. Time to apex is `launch speed / g`; fall time is `sqrt(2h / (g × fallMultiplier))`. Horizontal reach is `speed × total airtime`.

| Level | Launch speed | Jump impulse | Time to apex | Fall time | Ideal reach |
|---|---:|---:|---:|---:|---:|
| Arena | 6.264 m/s | 6.264 N·s | 0.639 s | 0.431 s | 5.345 m |
| Maze | 4.202 m/s | 4.202 N·s | 0.428 s | 0.303 s | 3.656 m |
| Training | 5.943 m/s | 5.943 N·s | 0.606 s | 0.408 s | 5.071 m |

These estimates assume full horizontal speed and landing at the take-off height. Physics steps and input ramp-up affect actual clearance.

**Widest-gap comparison:** The widest required gap is **2 m** in maze. The arena, maze and training jumps have ideal reaches of **5.345 m**, **3.656 m** and **5.071 m**, respectively, all exceeding 2 m. These estimates support clearing the gap at full speed when the landing is at the take-off height.

### 2. Update and FixedUpdate

`Update` reads input, builds camera-relative movement, checks grounding, updates coyote/buffer timestamps, queues jumps and starts landing squash. `FixedUpdate` sets horizontal velocity while preserving Y, applies a queued jump once and adds falling gravity.

Reading a one-frame press only in `FixedUpdate` can miss input. Applying repeated forces in `Update` makes their application depend on render frequency. Velocity is already in metres per second, so it is not multiplied by `deltaTime`.

### 3. Vector3.Dot

The controller currently uses CheckSphere.

### 4. The juice

Landing squash uses `EaseOutBack` for **0.20 seconds**, approximately **12 frames at 60 fps**, providing brief landing feedback. Body height becomes 0.7 of its resting height; width and depth become approximately 1.195 to preserve volume. `LerpUnclamped` retains the curve's overshoot, and the coroutine ends at the exact resting scale. Only Body scales, so the collider stays unchanged.

## Levels

| Level | What the controller must do there | What changed for this jump |
|---|---|---|
| Level01 — BlockOut1 (arena) | Navigate cover and obstacles along the arena route. | Jump height: 2 m.
| Level02 — BlockOut2 (maze) | Traverse ramps and narrow passages toward the exit. | Jump height: 0.9 m; fall multiplier: 2.0.
| Level03 — BloCkOut3 (training) | Navigate stairs, cover and elevated sections toward the finish. | Jump height: 1.8 m; fall multiplier: 2.2.

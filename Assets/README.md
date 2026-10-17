# CS464 Assignment 02 · Muhammad Aqib Ramzan · BSCS24074

- Video: https://drive.google.com/file/d/1iJ3P_Qv9xbJdixGN_RTeN75OJLWtJvZ7/view?usp=drive_link
- Input: Legacy Input with Active Input Handling set to Both. Everything is read in Update (`Input.GetAxis` for movement, `Input.GetKeyDown(Space)` for the jump).
- Ground check: `Physics.CheckSphere` at a Feet child (radius 0.3) with only the Ground layer in the mask, so the player's own collider is never hit. A wire-sphere gizmo is drawn in `OnDrawGizmosSelected` (green when grounded, red in the air).
- Camera: Exact smoothing, `1 − e^(−k·dt)` with sharpness 5, in `LateUpdate`, plus a look-ahead of 0.35 s that aims at the player's flat velocity.
- Juice: A squash on the Body child when landing, shaped by `Ease.OutBack`, run by a coroutine. It starts only on a real landing (more than 0.1 s in the air) and stops any squash already running.

## Tuning

| Setting | Value | Why this number |
|---|---|---|
| Speed | 5 m/s | Fast enough to cover a 3 m gap with a run-up in about 0.6 s, slow enough to land precisely on 1.2 m ledges. |
| Jump height | 1.8 m | Clears the 1.2 m ledges with 0.6 m to spare. Launch speed is √(2·9.81·1.8) = 5.94 m/s. |
| Time to apex | 0.61 s | v / g = 5.94 / 9.81. It follows from the jump height. |
| Fall multiplier | 2.2× | The fall takes 0.41 s instead of 0.61 s (√2.2 times shorter), so jumps feel snappy while the apex stays at 1.8 m. |
| Coyote time | 0.1 s | At 5 m/s that is 0.5 m past a ledge edge, enough for a late run-off jump. |
| Jump buffer | 0.12 s | About 7 frames at 60 fps. A press this early before landing is held and fires on landing. |
| Camera smoothing | sharpness 5 | The camera lags by speed / k = 5 / 5 = 1 m at full speed: smooth, but the player never leaves the screen. |
| Look-ahead or dead zone | 0.35 s look-ahead | 5 m/s × 0.35 s = 1.75 m of extra view in the direction of travel. |

## Four answers

1. **The jump, in numbers.** With jumpHeight 1.8 m and g = 9.81, the launch speed is v = √(2·9.81·1.8) = 5.94 m/s. The Rigidbody has mass 1 kg, so the impulse (jumpForce) is 1 × 5.94 = 5.94. The time to apex is 5.94 / 9.81 = 0.61 s. Falling is faster because of the 2.2× gravity: the fall from 1.8 m takes √(2·1.8 / (2.2·9.81)) = 0.41 s. The whole jump lasts about 1.02 s, so at 5 m/s it carries me about 5.1 m (a little less in practice because of the 0.02 s physics steps). My widest gap is 3 m, so the jump clears it with about 2 m to spare.
2. **Update and FixedUpdate.** In Update (every frame) my script checks the ground, reads WASD, builds the camera-relative move vector, notes the Space press, stamps the two timers (coyote and buffer), queues the jump, and starts the landing squash. In FixedUpdate (every 0.02 s) it sets `rb.linearVelocity` from the move vector while keeping y, applies the queued jump once, and adds the extra fall gravity. If I swapped them, the key press would be read in FixedUpdate, and `GetKeyDown` is only true for one frame. At 144 fps many frames have no FixedUpdate, so presses would be lost. Doing physics in Update would make the speed and jump change with the frame rate.
3. **Vector3.Dot.** `float vertical = Vector3.Dot(rb.linearVelocity, Vector3.up);` could replace `rb.linearVelocity.y < 0f` in the fall-gravity check. A positive result means the player is rising, a negative result means falling, and zero means at the apex or moving flat. The same idea works for the ground normal: `Vector3.Dot(hit.normal, Vector3.up) > 0.5f` means the surface faces mostly up, so it counts as ground, and a wall (result near 0) does not.
4. **The juice.** The landing squash uses `Ease.OutBack`: it moves from squashed (y scale 0.7) to normal, overshoots slightly past rest and settles, which looks springy. The duration is 0.25 s, which is 15 frames at 60 fps. That is long enough to be seen (a few frames would be invisible) and short enough that it never hides the next jump. The x and z scale grow to 1 / √0.7 ≈ 1.2 so the volume stays the same, and the effect only runs on a real landing after more than 0.1 s in the air.

## Levels

| Level | What the controller must do there | What I changed for this jump |
|---|---|---|
| Level01 | Walk from the spawn along the path to the goal, with a short jump or two over the obstacles. | Nothing: the 1.8 m jump and 5 m/s speed already fit. |
| Level02 | | Nothing: it fits the 1.8 m jump. |
| Level03 | | Nothing: it fits the 1.8 m jump. |
| Level04 | | Nothing: it fits the 1.8 m jump. |
| Level05 | Leave the spawn room, go along the lane hall, cross the rows of platforms, and reach the goal platform. | Nothing: the gaps are under 3 m and the ledges under 1.6 m. |

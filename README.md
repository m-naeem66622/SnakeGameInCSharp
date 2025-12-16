# Neon Snake (WPF)

A modern take on the classic Snake game built with **.NET 8** and **WPF**. The project focuses on stylized visuals, responsive UI, and a modular game engine that is easy to extend.

## Features
- **Responsive layout** with neon-inspired theming and adaptive sidebar.
- **Visible playfield grid** using a tiled drawing brush for crisp cell lines at any size.
- **Multiple food types**
  - Normal food: +10 points
  - Fast food: temporary speed boost
  - Slow food: temporary slow-down window
  - Bonus orb: 5× point reward with a short lifespan
- **Dynamic HUD** showing score, best score (per session), level, and speed label.
- **Game-over overlay** with session summary and instant restart button.
- **Score history dialog** to inspect past runs within the current session.
- **Keyboard controls** (WASD / arrow keys, space to pause) handled during the tunneling phase for reliability.
- **Sound feedback** for eating different food types and on game over.

## Gameplay Flow
1. Press **Play** (or space) to start or resume the game.
2. Guide the snake to different food types:
   - Fast/slow foods change the tick rate momentarily while still granting standard length growth.
   - Bonus orbs spawn periodically, persist for a few seconds, and grant 50 points plus a length boost when collected.
3. Levels advance every **50 points**. Each level slightly increases the base speed before any temporary modifiers.
4. Collision with the wall or the snake body ends the run, displays the overlay, records the score, and allows instant restart.

## Controls
| Action | Keys |
| --- | --- |
| Move | Arrow keys or WASD |
| Pause/Resume | Spacebar |
| Restart | Play Again button or Play button after overlay |
| Reset Session | Reset button |
| View History | History button |

## Architecture Overview
- `GameEngine` encapsulates grid state, timing, snake movement, scoring, leveling, and bonus logic.
- `Snake` handles direction changes, movement, growth, and collision checks.
- `Food`, `BonusFood`, and `FoodType` describe consumables.
- `MainWindow` drives rendering, input, HUD updates, responsive layout, and plays system sounds in response to engine events.
- `ScoreHistoryWindow` displays the in-session score table.

## Rendering Approach
- The playfield uses a cached `DrawingBrush` as background, tiled based on current cell dimensions for a consistent grid.
- Snake segments, foods, and bonus items are drawn onto a `Canvas` each frame, with glow effects and gradients for a neon look.
- The game-over overlay is a XAML `Grid` toggled via visibility, keeping the control tree simple.

## Audio
The app relies on `SystemSounds` for lightweight feedback:
- Normal: `Asterisk`
- Fast: `Beep`
- Slow: `Question`
- Bonus: `Exclamation`
- Game over: `Hand`

## Building and Running
1. Install the **.NET 8 SDK** and Visual Studio 2022 (or newer) with WPF support.
2. Restore and build: `dotnet build` (from the solution directory).
3. Run via `dotnet run` or press **F5** inside Visual Studio.

## Extensibility Ideas
- Persist best scores to disk or online leaderboards.
- Add touch input / on-screen controls.
- Introduce obstacles, power-ups, or different board sizes.
- Replace system sounds with custom audio samples.

## License
See `LICENSE.txt` for licensing information.

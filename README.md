# NguyenTuanThanh-Unity-Intern-Test

## Changelog

* **Removed** unused files: `BonusItem`, `Item`, `NormalItem`.
* **Level structure:** Each level now has multiple layers → each **layer = a Board**; `BoardsController` manages **multiple boards**.
* **Refactors:** `Board`, `Cell`, and `BoardsController` logic updated for the new gameplay.
* **New:** `TrayController` added as a **prefab**; instantiated when starting a new game.
* **Settings:** `GameSetting` updated.
* **Unchanged:** `GameState` and UI logic remain the same as the sample project.
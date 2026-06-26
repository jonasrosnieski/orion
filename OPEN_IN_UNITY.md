# Open ORION in Unity

## Correct folder (important)

When Unity Hub asks for a project folder, select **this folder**:

```
C:\Users\jonas\Projects\orion
```

Do **not** select:

- `C:\Users\jonas\Projects\aether` (Aether hub — not a Unity project)
- `C:\Users\jonas\Projects` (parent folder)
- `Assets` or `testModels` (subfolders)

In the file picker: navigate to `Projects` → click the **`orion`** folder once → click **Select Folder**.

## If Hub still fails

1. Close Unity Hub completely (tray icon too).
2. Reopen Unity Hub — **ORION** should appear in Projects.
3. Or double-click `OpenOrion.bat` in this folder to launch the Editor directly.

## Controls (Play mode)

- **WASD** — move astronaut (relative to isometric camera)
- Fixed orthographic camera at classic isometric angle (35.26° × 45°)

## Test scene

Open `Assets/Scenes/MeshyTest.unity` and press **Play**.

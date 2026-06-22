# Meshy → Unity workflow

Drop exported models from [Meshy](https://www.meshy.ai/) here.

## Export from Meshy

1. Generate your model in Meshy (text-to-3D or image-to-3D).
2. **Download** as **FBX** (preferred) or **GLB**.
3. Save into this folder, e.g. `Assets/Art/Meshy/probe_drone.fbx`.

## Import in Unity

1. Unity detects new files automatically (or **Assets → Refresh**).
2. Select the model in the Project window.
3. In the **Inspector**:
   - **Scale Factor**: often `1` or `0.01` depending on export — adjust until size looks right.
   - Enable **Generate Colliders** if you need physics on props.
   - For characters, set **Rig** → **Humanoid** or **Generic** as needed.
4. Drag the model into a scene or create a **Prefab** under `Assets/Prefabs/`.

## Tips for sci-fi props

- Prefer **FBX** for rigged meshes; **GLB** works for static props.
- Keep textures embedded in the FBX when Meshy offers that option.
- Name assets clearly: `sector_door_01`, `crate_scifi_02`, etc.
- Large binaries: consider Git LFS (`git lfs track "*.fbx"`).

## URP materials

After import, assign **Universal Render Pipeline/Lit** materials. Meshy PBR textures usually map to:

| Texture | URP slot |
|---------|----------|
| Base Color / Albedo | Base Map |
| Normal | Normal Map |
| Metallic | Metallic Map |
| Roughness | Smoothness (invert if needed) |

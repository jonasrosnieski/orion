using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class AnimationClipInspector
{
    private const string WalkModelPath =
        "Assets/testModels/Meshy_AI_Cosmic_Cadet_biped/Meshy_AI_Cosmic_Cadet_biped_Animation_Walking_withSkin.glb";

    private const string ResampledWalkClipPath = "Assets/Settings/Animations/CosmicCadet_Walk_24f.anim";

    public static void Inspect()
    {
        InspectClipAtPath(WalkModelPath, "Source GLB");
        InspectClipAtPath(ResampledWalkClipPath, "Resampled walk");
        EditorApplication.Exit(0);
    }

    private static void InspectClipAtPath(string assetPath, string label)
    {
        if (assetPath.EndsWith(".anim", System.StringComparison.OrdinalIgnoreCase))
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (clip == null)
            {
                Debug.Log($"{label}: clip not found at {assetPath}");
                return;
            }

            LogClip(label, new[] { clip });
            return;
        }

        var clips = AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .OfType<AnimationClip>()
            .Where(clip => !clip.name.StartsWith("__"))
            .ToArray();

        LogClip(label, clips);
    }

    private static void LogClip(string label, AnimationClip[] clips)
    {
        var report = new StringBuilder();
        report.AppendLine($"{label} — clips found: {clips.Length}");

        foreach (var clip in clips)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var keyCount = 0;
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                keyCount = Mathf.Max(keyCount, curve.length);
            }

            report.AppendLine(
                $"{clip.name}: length={clip.length:F3}s frameRate={clip.frameRate} legacy={clip.legacy} maxKeys={keyCount} curves={bindings.Length}");
        }

        Debug.Log(report.ToString());
    }
}

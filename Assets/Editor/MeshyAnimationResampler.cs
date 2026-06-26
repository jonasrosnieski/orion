using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MeshyAnimationResampler
{
    public const int WalkCycleFrameCount = 24;
    public const float WalkCycleFrameRate = 24f;

    private const string ResampledWalkClipPath = "Assets/Settings/Animations/CosmicCadet_Walk_24f.anim";

    public static AnimationClip GetOrCreateResampledWalkClip(string glbAssetPath, bool forceRebuild = false)
    {
        var sourceClip = LoadWalkClipFromGlb(glbAssetPath);
        if (sourceClip == null)
        {
            return null;
        }

        EnsureAnimationsFolder();

        if (forceRebuild && AssetDatabase.LoadAssetAtPath<AnimationClip>(ResampledWalkClipPath) != null)
        {
            AssetDatabase.DeleteAsset(ResampledWalkClipPath);
        }

        var existingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ResampledWalkClipPath);
        if (existingClip != null)
        {
            return existingClip;
        }

        var resampledClip = ResampleWalkClip(sourceClip);
        AssetDatabase.CreateAsset(resampledClip, ResampledWalkClipPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Resampled walk clip saved ({WalkCycleFrameCount} frames @ {WalkCycleFrameRate}fps): {ResampledWalkClipPath}");
        return resampledClip;
    }

    public static AnimationClip ResampleWalkClip(AnimationClip source)
    {
        var clip = new AnimationClip
        {
            legacy = true,
            frameRate = WalkCycleFrameRate,
            wrapMode = WrapMode.Loop,
        };

        var settings = AnimationUtility.GetAnimationClipSettings(source);
        settings.loopTime = true;
        settings.loopBlend = true;
        settings.cycleOffset = 0f;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        foreach (var binding in AnimationUtility.GetCurveBindings(source))
        {
            var sourceCurve = AnimationUtility.GetEditorCurve(source, binding);
            if (sourceCurve == null || sourceCurve.length == 0)
            {
                continue;
            }

            var resampledCurve = SampleCurve(sourceCurve, source.length, WalkCycleFrameCount, WalkCycleFrameRate);
            AnimationUtility.SetEditorCurve(clip, binding, resampledCurve);
        }

        foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(source))
        {
            var sourceCurve = AnimationUtility.GetObjectReferenceCurve(source, binding);
            if (sourceCurve == null || sourceCurve.Length == 0)
            {
                continue;
            }

            var resampledCurve = SampleObjectCurve(sourceCurve, source.length, WalkCycleFrameCount, WalkCycleFrameRate);
            AnimationUtility.SetObjectReferenceCurve(clip, binding, resampledCurve);
        }

        clip.EnsureQuaternionContinuity();
        return clip;
    }

    [MenuItem("ORION/Resample Walk Animation (24 frames)")]
    public static void ResampleFromMenu()
    {
        const string walkModelPath =
            "Assets/testModels/Meshy_AI_Cosmic_Cadet_biped/Meshy_AI_Cosmic_Cadet_biped_Animation_Walking_withSkin.glb";

        MeshyAnimationUtility.EnsureWalkAnimationImported(walkModelPath);

        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(ResampledWalkClipPath) != null)
        {
            AssetDatabase.DeleteAsset(ResampledWalkClipPath);
        }

        var clip = GetOrCreateResampledWalkClip(walkModelPath);
        if (clip == null)
        {
            Debug.LogError("Failed to resample walk animation.");
            return;
        }

        var keyCount = CountMaxKeys(clip);
        Debug.Log($"Walk clip ready: {clip.name}, length={clip.length:F3}s, maxKeys={keyCount}");
    }

    private static AnimationClip LoadWalkClipFromGlb(string glbAssetPath)
    {
        var clips = AssetDatabase.LoadAllAssetsAtPath(glbAssetPath)
            .OfType<AnimationClip>()
            .Where(clip => !clip.name.StartsWith("__", StringComparison.Ordinal))
            .ToArray();

        if (clips.Length == 0)
        {
            Debug.LogWarning($"No animation clips found in {glbAssetPath}");
            return null;
        }

        return clips.FirstOrDefault(clip =>
            clip.name.Contains("Walk", StringComparison.OrdinalIgnoreCase)) ?? clips[0];
    }

    private static void EnsureAnimationsFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Settings"))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Settings/Animations"))
        {
            AssetDatabase.CreateFolder("Assets/Settings", "Animations");
        }
    }

    private static AnimationCurve SampleCurve(
        AnimationCurve source,
        float sourceLength,
        int frameCount,
        float frameRate)
    {
        var curve = new AnimationCurve();

        for (var frame = 0; frame < frameCount; frame++)
        {
            var normalizedTime = frame / (float)frameCount;
            var sampleTime = normalizedTime * sourceLength;
            var keyTime = frame / frameRate;
            curve.AddKey(keyTime, source.Evaluate(sampleTime));
        }

        return curve;
    }

    private static ObjectReferenceKeyframe[] SampleObjectCurve(
        ObjectReferenceKeyframe[] source,
        float sourceLength,
        int frameCount,
        float frameRate)
    {
        var keys = new ObjectReferenceKeyframe[frameCount];

        for (var frame = 0; frame < frameCount; frame++)
        {
            var normalizedTime = frame / (float)frameCount;
            var sampleTime = normalizedTime * sourceLength;
            keys[frame] = new ObjectReferenceKeyframe
            {
                time = frame / frameRate,
                value = EvaluateObjectReference(source, sampleTime),
            };
        }

        return keys;
    }

    private static UnityEngine.Object EvaluateObjectReference(ObjectReferenceKeyframe[] keys, float time)
    {
        if (keys.Length == 0)
        {
            return null;
        }

        if (time <= keys[0].time)
        {
            return keys[0].value;
        }

        for (var i = 1; i < keys.Length; i++)
        {
            if (time < keys[i].time)
            {
                return keys[i - 1].value;
            }
        }

        return keys[^1].value;
    }

    private static int CountMaxKeys(AnimationClip clip)
    {
        var keyCount = 0;
        foreach (var binding in AnimationUtility.GetCurveBindings(clip))
        {
            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            keyCount = Mathf.Max(keyCount, curve.length);
        }

        return keyCount;
    }
}

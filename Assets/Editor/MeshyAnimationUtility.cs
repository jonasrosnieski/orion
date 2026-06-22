using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class MeshyAnimationUtility
{
    public static void EnsureWalkAnimationImported(string glbAssetPath)
    {
        var importer = AssetImporter.GetAtPath(glbAssetPath);
        if (importer == null)
        {
            return;
        }

        var serializedImporter = new SerializedObject(importer);
        var animationMethod = serializedImporter.FindProperty("importSettings.animationMethod");
        if (animationMethod != null && animationMethod.intValue != 1)
        {
            animationMethod.intValue = 1;
            serializedImporter.ApplyModifiedPropertiesWithoutUndo();
            importer.SaveAndReimport();
        }
        else
        {
            AssetDatabase.ImportAsset(glbAssetPath, ImportAssetOptions.ForceUpdate);
        }
    }

    public static void BindWalkAnimation(GameObject modelRoot, string glbAssetPath)
    {
        var clips = AssetDatabase.LoadAllAssetsAtPath(glbAssetPath)
            .OfType<AnimationClip>()
            .Where(clip => !clip.name.StartsWith("__", StringComparison.Ordinal))
            .ToArray();

        if (clips.Length == 0)
        {
            Debug.LogWarning($"No animation clips found in {glbAssetPath}");
            return;
        }

        var walkClip = clips.FirstOrDefault(clip =>
            clip.name.Contains("Walk", StringComparison.OrdinalIgnoreCase)) ?? clips[0];

        var legacyAnimation = modelRoot.GetComponentInChildren<Animation>();
        if (legacyAnimation != null)
        {
            legacyAnimation.playAutomatically = false;
            if (legacyAnimation.clip != null)
            {
                Debug.Log($"Legacy walk clip ready: {legacyAnimation.clip.name}");
                return;
            }

            if (legacyAnimation.GetClip(walkClip.name) == null)
            {
                legacyAnimation.AddClip(walkClip, walkClip.name);
            }

            legacyAnimation.clip = walkClip;
            Debug.Log($"Legacy walk clip assigned: {walkClip.name}");
            return;
        }

        var animator = modelRoot.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            return;
        }

        const string controllerPath = "Assets/Settings/CosmicCadet_Walk.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        var stateMachine = controller.layers[0].stateMachine;
        AnimatorState walkState = null;
        foreach (var childState in stateMachine.states)
        {
            if (childState.state.name == "Walk")
            {
                walkState = childState.state;
                break;
            }
        }

        if (walkState == null)
        {
            walkState = stateMachine.AddState("Walk");
        }

        walkState.motion = walkClip;
        walkState.writeDefaultValues = true;

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }
}

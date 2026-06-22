using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class PlaygroundSceneBuilder
{
    private const string WalkModelPath =
        "Assets/testModels/Meshy_AI_Cosmic_Cadet_biped/Meshy_AI_Cosmic_Cadet_biped_Animation_Walking_withSkin.glb";

    private const string RingModelPath =
        "Assets/testModels/Meshy_AI_Amber_Heart_Vine_Ring_0622141025_texture.glb";

    private const string ScenePath = "Assets/Scenes/MeshyTest.unity";
    private const string PipelineAssetPath = "Assets/Settings/URP_Asset.asset";
    private const string RendererDataPath = "Assets/Settings/URP_ForwardRenderer.asset";

    public static void Build()
    {
        EnsureFolders();
        EnsureUrpPipeline();
        RemoveLegacyRingAsset();

        var fullPath = Path.Combine(
            Application.dataPath,
            "testModels",
            "Meshy_AI_Cosmic_Cadet_biped",
            "Meshy_AI_Cosmic_Cadet_biped_Animation_Walking_withSkin.glb");

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Walk model not found at {WalkModelPath}");
            EditorApplication.Exit(1);
            return;
        }

        AssetDatabase.ImportAsset(WalkModelPath, ImportAssetOptions.ForceUpdate);

        MeshyAnimationUtility.EnsureWalkAnimationImported(WalkModelPath);

        var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WalkModelPath);
        if (modelPrefab == null)
        {
            Debug.LogError($"Failed to load GLB prefab at {WalkModelPath}");
            EditorApplication.Exit(1);
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateLighting();
        CreateGround();
        var player = CreatePlayer(modelPrefab);
        var camera = CreateIsometricCamera(player.transform);

        var playerController = player.GetComponent<ThirdPersonPlayerController>();
        playerController.SetCameraTransform(camera.transform);

        EditorSceneManager.SaveScene(scene, ScenePath);

        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(ScenePath, true),
        };
        EditorBuildSettings.scenes = scenes.ToArray();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Playground scene saved to {ScenePath} with Cosmic Cadet third-person setup.");

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    [MenuItem("ORION/Rebuild Playground Scene")]
    public static void RebuildFromMenu()
    {
        Build();
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Editor"))
        {
            AssetDatabase.CreateFolder("Assets", "Editor");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Settings"))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Scripts"))
        {
            AssetDatabase.CreateFolder("Assets", "Scripts");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Scripts/Player"))
        {
            AssetDatabase.CreateFolder("Assets/Scripts", "Player");
        }
    }

    private static void EnsureUrpPipeline()
    {
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
        if (pipeline == null)
        {
            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, RendererDataPath);

            pipeline = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(pipeline, PipelineAssetPath);
        }

        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
    }

    private static void RemoveLegacyRingAsset()
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(RingModelPath) != null)
        {
            AssetDatabase.DeleteAsset(RingModelPath);
        }
    }

    private static void CreateLighting()
    {
        var keyLightGo = new GameObject("Directional Light");
        var keyLight = keyLightGo.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 1.15f;
        keyLight.color = new Color(1f, 0.97f, 0.92f);
        keyLight.shadows = LightShadows.Soft;
        keyLightGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

        var fillLightGo = new GameObject("Fill Light");
        var fillLight = fillLightGo.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.35f;
        fillLight.color = new Color(0.75f, 0.85f, 1f);
        fillLightGo.transform.rotation = Quaternion.Euler(12f, 140f, 0f);
    }

    private static void CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(4f, 1f, 4f);

        var groundRenderer = ground.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            var groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = new Color(0.1f, 0.11f, 0.14f),
            };
            groundRenderer.sharedMaterial = groundMaterial;
        }
    }

    private static GameObject CreatePlayer(GameObject modelPrefab)
    {
        var player = new GameObject("Player");
        player.transform.position = Vector3.zero;

        var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, player.transform);
        modelInstance.name = "Cosmic_Cadet";
        AlignModelToGround(modelInstance);
        MeshyAnimationUtility.BindWalkAnimation(modelInstance, WalkModelPath);

        var controller = player.AddComponent<CharacterController>();
        FitCharacterController(controller, modelInstance);

        player.AddComponent<ThirdPersonPlayerController>();
        player.AddComponent<MeshyCharacterAnimation>();

        var animator = modelInstance.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        return player;
    }

    private static Camera CreateIsometricCamera(Transform target)
    {
        var cameraGo = new GameObject("IsometricCamera");
        cameraGo.tag = "MainCamera";

        var camera = cameraGo.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.orthographic = true;
        camera.orthographicSize = 2.8f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 200f;
        cameraGo.AddComponent<AudioListener>();

        var isometricCamera = cameraGo.AddComponent<IsometricCamera>();
        isometricCamera.SetTarget(target);

        cameraGo.transform.position = target.position + new Vector3(10f, 10f, -10f);
        cameraGo.transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);

        return camera;
    }

    private static void AlignModelToGround(GameObject modelRoot)
    {
        var renderers = modelRoot.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return;
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var height = bounds.size.y;
        if (height > 0.01f && height < 0.5f)
        {
            var scale = 1.75f / height;
            modelRoot.transform.localScale = Vector3.one * scale;
        }

        renderers = modelRoot.GetComponentsInChildren<Renderer>();
        bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var offset = modelRoot.transform.position - bounds.min;
        modelRoot.transform.localPosition = new Vector3(0f, offset.y, 0f);
    }

    private static void FitCharacterController(CharacterController controller, GameObject modelRoot)
    {
        var renderers = modelRoot.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            return;
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var localBounds = bounds;
        localBounds.center -= modelRoot.transform.parent.position;

        controller.height = Mathf.Max(localBounds.size.y, 1.2f);
        controller.radius = Mathf.Clamp(Mathf.Max(localBounds.extents.x, localBounds.extents.z) * 0.35f, 0.25f, 0.5f);
        controller.center = new Vector3(0f, controller.height * 0.5f, 0f);
        controller.slopeLimit = 45f;
        controller.stepOffset = 0.25f;
    }
}

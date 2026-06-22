using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class MeshyTestSceneBuilder
{
    private const string ModelPath =
        "Assets/testModels/Meshy_AI_Amber_Heart_Vine_Ring_0622141025_texture.glb";

    private const string ScenePath = "Assets/Scenes/MeshyTest.unity";
    private const string PipelineAssetPath = "Assets/Settings/URP_Asset.asset";
    private const string RendererDataPath = "Assets/Settings/URP_ForwardRenderer.asset";

    public static void Build()
    {
        EnsureFolders();
        EnsureUrpPipeline();

        var fullPath = Path.Combine(
            Application.dataPath,
            "testModels",
            "Meshy_AI_Amber_Heart_Vine_Ring_0622141025_texture.glb");

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"GLB not found at {ModelPath}");
            EditorApplication.Exit(1);
            return;
        }

        AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceUpdate);

        var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (modelPrefab == null)
        {
            Debug.LogError($"Failed to load GLB prefab at {ModelPath}");
            EditorApplication.Exit(1);
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateLighting();
        CreateGround();
        var modelInstance = PlaceModel(modelPrefab);

        EditorSceneManager.SaveScene(scene, ScenePath);

        var buildSettings = EditorBuildSettings.scenes;
        var scenes = new List<EditorBuildSettingsScene>(buildSettings)
        {
            new EditorBuildSettingsScene(ScenePath, true),
        };
        EditorBuildSettings.scenes = scenes.ToArray();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Meshy test scene saved to {ScenePath}. Model at {modelInstance.transform.position}.");
        EditorApplication.Exit(0);
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

    private static void CreateCamera()
    {
        var cameraGo = new GameObject("Main Camera");
        cameraGo.tag = "MainCamera";

        var camera = cameraGo.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 100f;
        cameraGo.AddComponent<AudioListener>();

        cameraGo.transform.position = new Vector3(0.8f, 0.9f, -2.2f);
        cameraGo.transform.LookAt(Vector3.up * 0.35f);
    }

    private static void CreateLighting()
    {
        var keyLightGo = new GameObject("Directional Light");
        var keyLight = keyLightGo.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 1.1f;
        keyLight.color = new Color(1f, 0.97f, 0.92f);
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
        ground.transform.position = new Vector3(0f, -0.01f, 0f);
        ground.transform.localScale = new Vector3(0.35f, 1f, 0.35f);

        var groundRenderer = ground.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            var groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = new Color(0.12f, 0.13f, 0.16f),
            };
            groundRenderer.sharedMaterial = groundMaterial;
        }
    }

    private static GameObject PlaceModel(GameObject modelPrefab)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
        instance.name = "Meshy_Amber_Heart_Vine_Ring";

        CenterAndFit(instance, targetSize: 1.2f);
        instance.transform.rotation = Quaternion.Euler(0f, 25f, 0f);

        return instance;
    }

    private static void CenterAndFit(GameObject root, float targetSize)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return;
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        if (maxExtent > 0.0001f)
        {
            var scale = targetSize / (maxExtent * 2f);
            root.transform.localScale = Vector3.one * scale;
        }

        renderers = root.GetComponentsInChildren<Renderer>();
        bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        root.transform.position = -bounds.center + Vector3.up * bounds.extents.y;
    }
}

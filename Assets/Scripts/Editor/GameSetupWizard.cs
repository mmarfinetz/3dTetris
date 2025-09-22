using UnityEngine;
using UnityEditor;
using TetrisJenga.Core;
using TetrisJenga.Pieces;
using TetrisJenga.Physics;
using TetrisJenga.Input;
using TetrisJenga.UI;

public class GameSetupWizard : EditorWindow
{
    [MenuItem("TetrisJenga/Setup Game Scene")]
    public static void SetupGameScene()
    {
        UnityEngine.Debug.Log("Setting up 3D Tetris-Jenga scene...");

        // Clear existing objects (optional)
        if (EditorUtility.DisplayDialog("Setup Scene",
            "This will create all necessary game objects. Continue?",
            "Yes", "Cancel"))
        {
            CreateGameObjects();
            ConfigureLayers();
            ConfigurePhysics();

            UnityEngine.Debug.Log("Scene setup complete! Press Play to start the game.");
            EditorUtility.DisplayDialog("Setup Complete",
                "Scene has been configured successfully!\n\nPress Play to start the game.",
                "OK");
        }
    }

    static void CreateGameObjects()
    {
        // 1. Create GameManager
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<GameManager>();
        var pieceController = gameManager.AddComponent<PieceController>();
        gameManager.AddComponent<PieceSpawner>();
        gameManager.AddComponent<InputManager>();

        // 2. Create Physics Systems
        GameObject stabilityAnalyzer = new GameObject("StabilityAnalyzer");
        stabilityAnalyzer.AddComponent<StabilityAnalyzer>();
        stabilityAnalyzer.AddComponent<SettlementDetector>();

        // 3. Create Tower Base
        GameObject towerBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        towerBase.name = "TowerBase";
        towerBase.transform.position = Vector3.zero;
        towerBase.transform.localScale = new Vector3(12f, 0.5f, 12f);
        towerBase.tag = "Base";

        // Add physics material to base
        Collider baseCollider = towerBase.GetComponent<Collider>();
        PhysicsMaterial baseMaterial = new PhysicsMaterial("BaseMaterial");
        baseMaterial.dynamicFriction = 0.6f;
        baseMaterial.staticFriction = 0.8f;
        baseMaterial.bounciness = 0.1f;
        baseCollider.material = baseMaterial;

        // Make base static
        towerBase.isStatic = true;

        // 4. Setup Camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
        }

        mainCamera.transform.position = new Vector3(10, 10, -10);
        mainCamera.transform.LookAt(Vector3.up * 5);
        mainCamera.gameObject.AddComponent<CameraController>();

        // 5. Create UI Manager
        GameObject uiManager = new GameObject("UIManager");
        uiManager.AddComponent<UIManager>();

        // 6. Create Directional Light
        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj == null)
        {
            lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;
        }
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0);

        // 7. Create spawn point
        GameObject spawnPoint = new GameObject("SpawnPoint");
        spawnPoint.transform.position = new Vector3(0, 15, 0);

        // 8. Create preview container
        GameObject previewContainer = new GameObject("PreviewContainer");
        previewContainer.transform.position = new Vector3(7, 10, 0);

        UnityEngine.Debug.Log("All game objects created successfully!");
    }

    static void ConfigureLayers()
    {
        // Create layers
        CreateLayer("Piece", 8);
        CreateLayer("Ghost", 9);
        CreateLayer("UI", 10);

        // Create tags
        CreateTag("Base");
        CreateTag("Piece");
        CreateTag("Boundary");

        UnityEngine.Debug.Log("Layers and tags configured!");
    }

    static void ConfigurePhysics()
    {
        // Set gravity
        UnityEngine.Physics.gravity = new Vector3(0, -9.81f, 0);

        // These settings would need to be set manually in Project Settings
        // as they're not accessible via script in newer Unity versions
        UnityEngine.Debug.Log("Physics configured! Check Project Settings > Physics for fine-tuning.");
    }

    static void CreateLayer(string name, int layer)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        if (layers != null && layer < layers.arraySize)
        {
            SerializedProperty layerSP = layers.GetArrayElementAtIndex(layer);
            if (layerSP != null && string.IsNullOrEmpty(layerSP.stringValue))
            {
                layerSP.stringValue = name;
                tagManager.ApplyModifiedProperties();
            }
        }
    }

    static void CreateTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Check if tag already exists
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tagName))
            {
                found = true;
                break;
            }
        }

        // Add tag if it doesn't exist
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(0);
            newTag.stringValue = tagName;
            tagManager.ApplyModifiedProperties();
        }
    }
}

// Auto-run setup on first import
[InitializeOnLoad]
public class AutoSetup
{
    static AutoSetup()
    {
        EditorApplication.delayCall += CheckFirstRun;
    }

    static void CheckFirstRun()
    {
        if (!SessionState.GetBool("TetrisJengaSetupComplete", false))
        {
            if (EditorUtility.DisplayDialog("3D Tetris-Jenga Setup",
                "Would you like to automatically set up the game scene?",
                "Yes", "No"))
            {
                GameSetupWizard.SetupGameScene();
            }
            SessionState.SetBool("TetrisJengaSetupComplete", true);
        }
    }
}
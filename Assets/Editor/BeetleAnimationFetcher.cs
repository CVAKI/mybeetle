// ============================================================
//  BeetleAnimationFetcher.cs
//  Place this file in:  Assets/Editor/BeetleAnimationFetcher.cs
//
//  HOW TO USE:
//  1. Copy this file into your Assets/Editor/ folder in Unity
//  2. In the Unity menu bar click:  MyBettle ▶ Fetch Beetle Animations
//  3. A window opens — drag your Beetle GLB/FBX model into the slot
//  4. Click "Get All Animations"
//  5. All clip names are printed to Console AND saved to
//     Assets/BeetleAnimationList.txt
// ============================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class BeetleAnimationFetcher : EditorWindow
{
    private GameObject beetleModel;
    private Vector2 scrollPos;
    private List<string> clipNames = new List<string>();
    private string statusMessage = "";

    [MenuItem("MyBettle/Fetch Beetle Animations")]
    public static void ShowWindow()
    {
        var window = GetWindow<BeetleAnimationFetcher>("Beetle Animation Fetcher");
        window.minSize = new Vector2(500, 600);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("🪲 Beetle Animation Clip Fetcher", EditorStyles.boldLabel);
        GUILayout.Label("Drag your Beetle model (GLB/FBX) into the field below.", EditorStyles.helpBox);
        GUILayout.Space(8);

        beetleModel = (GameObject)EditorGUILayout.ObjectField(
            "Beetle Model", beetleModel, typeof(GameObject), false);

        GUILayout.Space(8);

        if (GUILayout.Button("▶  Get All Animations", GUILayout.Height(36)))
        {
            FetchAnimations();
        }

        if (!string.IsNullOrEmpty(statusMessage))
        {
            GUILayout.Space(6);
            GUILayout.Label(statusMessage, EditorStyles.helpBox);
        }

        if (clipNames.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Found {clipNames.Count} animation clips:", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos,
                GUILayout.ExpandHeight(true));

            for (int i = 0; i < clipNames.Count; i++)
            {
                EditorGUILayout.SelectableLabel(
                    $"{(i + 1).ToString("D3")}. {clipNames[i]}",
                    EditorStyles.textField,
                    GUILayout.Height(18));
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(8);

            if (GUILayout.Button("💾  Save List to Assets/BeetleAnimationList.txt",
                GUILayout.Height(32)))
            {
                SaveToFile();
            }
        }
    }

    private void FetchAnimations()
    {
        clipNames.Clear();
        statusMessage = "";

        if (beetleModel == null)
        {
            statusMessage = "⚠ Please assign a Beetle model first.";
            return;
        }

        string path = AssetDatabase.GetAssetPath(beetleModel);
        if (string.IsNullOrEmpty(path))
        {
            statusMessage = "⚠ Could not get asset path. Make sure the model is in your project.";
            return;
        }

        // Load ALL assets embedded inside the GLB/FBX
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);

        foreach (Object asset in allAssets)
        {
            if (asset is AnimationClip clip)
            {
                // Skip Unity's auto-generated __preview__ clips
                if (clip.name.StartsWith("__preview__")) continue;
                clipNames.Add(clip.name);
            }
        }

        if (clipNames.Count == 0)
        {
            // Fallback: try through ModelImporter
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                foreach (var clipInfo in importer.clipAnimations)
                {
                    clipNames.Add(clipInfo.name);
                }
            }
        }

        if (clipNames.Count == 0)
        {
            statusMessage = "⚠ No animation clips found in this model. " +
                            "Make sure 'Import Animation' is enabled in the model's Import Settings.";
        }
        else
        {
            statusMessage = $"✅ Found {clipNames.Count} clips. Scroll to see all.";
            // Also log to console
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"=== Beetle Animations ({clipNames.Count} clips) ===");
            for (int i = 0; i < clipNames.Count; i++)
                sb.AppendLine($"{(i + 1).ToString("D3")}. {clipNames[i]}");
            Debug.Log(sb.ToString());
        }

        Repaint();
    }

    private void SaveToFile()
    {
        string savePath = Path.Combine(Application.dataPath, "BeetleAnimationList.txt");
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("MyBettle — Animation Clips");
        sb.AppendLine(new string('-', 50));
        for (int i = 0; i < clipNames.Count; i++)
            sb.AppendLine($"{(i + 1).ToString("D3")}. {clipNames[i]}");

        File.WriteAllText(savePath, sb.ToString());
        AssetDatabase.Refresh();
        statusMessage = $"✅ Saved to Assets/BeetleAnimationList.txt";
        Debug.Log($"Animation list saved to: {savePath}");
    }
}

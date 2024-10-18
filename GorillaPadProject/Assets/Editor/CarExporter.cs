using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class AppExporter : EditorWindow
{
    private AppDescribtor[] descriptorNotes;
    [MenuItem("Custom apps/apps Exporter")]

    public static void ShowWindow()
    {
        GetWindow(typeof(AppExporter), false, "apps Exporter", false);
    }

    public void OnFocus()
    {
        descriptorNotes = FindObjectsOfType<AppDescribtor>();
    }

    public Vector2 scrollPosition = Vector2.zero;
    public void OnGUI()
    {
        var window = GetWindow(typeof(AppExporter), false, "apps Exporter", false);

        int ScrollSpace = (16 + 20) + (16 + 17 + 17 + 20 + 20);
        foreach (var note in descriptorNotes)
        {
            if (note != null)
            {
                ScrollSpace += (16 + 17 + 17 + 20 + 20);
            }
        }

        float currentWindowWidth = EditorGUIUtility.currentViewWidth;
        float windowWidthIncludingScrollbar = currentWindowWidth;
        if (window.position.size.y >= ScrollSpace)
        {
            windowWidthIncludingScrollbar += 30;
        }

        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, EditorGUIUtility.currentViewWidth, window.position.size.y), scrollPosition, new Rect(0, 0, EditorGUIUtility.currentViewWidth - 20, ScrollSpace), false, false);

        foreach (AppDescribtor descriptorNote in descriptorNotes)
        {
            if (descriptorNote != null)
            {
                GUILayout.Label(descriptorNote.gameObject.name, EditorStyles.boldLabel, GUILayout.Height(16));
                descriptorNote.Name = EditorGUILayout.TextField("Name:", descriptorNote.Name, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(17));
                descriptorNote.Author = EditorGUILayout.TextField("Author:", descriptorNote.Author, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(17));
                descriptorNote.Description = EditorGUILayout.TextField("Description:", descriptorNote.Description, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(17));

                if (GUILayout.Button("Export " + descriptorNote.Name, GUILayout.Width(windowWidthIncludingScrollbar - 40), GUILayout.Height(20)))
                {
                    GameObject noteObject = descriptorNote.gameObject;
                    if (noteObject != null && descriptorNote != null)
                    {
                        if (descriptorNote.Name == "" || descriptorNote.Author == "" || descriptorNote.Description == "")
                        {
                            EditorUtility.DisplayDialog("Export Failed", "It is required to fill in the Name, Author, and Description for your apps.", "OK");
                            return;
                        }

                        string path = EditorUtility.SaveFilePanel("Where will you build your apps?", "", descriptorNote.Name + ".apps", "app");

                        if (path != "")
                        {
                            Debug.ClearDeveloperConsole();
                            Debug.Log("Exporting apps");
                            EditorUtility.SetDirty(descriptorNote);
                            BuildAssetBundle(descriptorNote.gameObject, path);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Export Failed", "Please include the path to where the apps will be exported at.", "OK");
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Export Failed", "The apps object couldn't be found.", "OK");
                    }
                }
                GUILayout.Space(20);
            }
        }
        GUI.EndScrollView();
    }
    static public void BuildAssetBundle(GameObject obj, string path)
    {
        GameObject selectedObject = obj;
        string assetBundleDirectoryTEMP = "Assets/ExportedApps";

        AppDescribtor descriptor = selectedObject.GetComponent<AppDescribtor>();

        if (!AssetDatabase.IsValidFolder("Assets/ExportedApps"))
        {
            AssetDatabase.CreateFolder("Assets", "ExportedApps");
        }

        string appsName = descriptor.Name;

        string prefabPathTEMP = "Assets/ExportedApps/App.prefab";

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        PrefabUtility.SaveAsPrefabAsset(selectedObject.gameObject, prefabPathTEMP);
        GameObject contentsRoot = PrefabUtility.LoadPrefabContents(prefabPathTEMP);
        contentsRoot.name = "App";

        if (File.Exists(prefabPathTEMP))
        {
            File.Delete(prefabPathTEMP);
        }

        string newprefabPath = "Assets/ExportedApps/" + contentsRoot.name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(contentsRoot, newprefabPath);
        PrefabUtility.UnloadPrefabContents(contentsRoot);
        AssetImporter.GetAtPath(newprefabPath).SetAssetBundleNameAndVariant("Appassetbundle", "");

        if (!Directory.Exists("Assets/ExportedApps"))
        {
            Directory.CreateDirectory(assetBundleDirectoryTEMP);
        }

        string asset_new = assetBundleDirectoryTEMP + "/" + appsName;
        if (File.Exists(asset_new + ".apps"))
        {
            File.Delete(asset_new + ".apps");
        }

        BuildPipeline.BuildAssetBundles(assetBundleDirectoryTEMP, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        if (File.Exists(newprefabPath))
        {
            File.Delete(newprefabPath);
        }

        string asset_temporary = assetBundleDirectoryTEMP + "/Appassetbundle";
        string metafile = asset_temporary + ".meta";
        if (File.Exists(asset_temporary))
        {
            File.Move(asset_temporary, asset_new + ".apps");
        }

        AssetDatabase.Refresh();
        Debug.ClearDeveloperConsole();

        string path1 = assetBundleDirectoryTEMP + "/" + appsName + ".apps";
        string path2 = path;

        if (!File.Exists(path2)) // add
        {
            File.Move(path1, path2);
        }
        else // replace
        {
            File.Delete(path2);
            File.Move(path1, path2);
        }
        EditorUtility.DisplayDialog("Export Success", $"Your App was exported!", "OK");

        try
        {
            AssetDatabase.RemoveAssetBundleName("Appassetbundle", true);
        }
        catch
        {

        }

        string appPath = path + "/";
        EditorUtility.RevealInFinder(appPath);

        if (AssetDatabase.IsValidFolder("Assets/ExportedApps"))
        {
            AssetDatabase.DeleteAsset("Assets/ExportedApps");
        }
    }
}
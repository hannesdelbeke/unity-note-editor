using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Security.Policy;

public class NoteEditor : EditorWindow, IHasCustomMenu
{
    private string noteText = "";
    private Vector2 scrollPosition;
    private string noteFilePath = "";
    private string currentAssetGUID = "";
    private bool isEditMode;
    private const string EditModePrefKey = "NoteEditor_IsEditMode";

    [MenuItem("Window/Note Editor")]
    public static void ShowWindow()
    {
        GetWindow<NoteEditor>("Note Editor");
    }

    static NoteEditor()
    {
        EditorGUI.hyperLinkClicked += EditorGUI_hyperLinkClicked;
    }

    private void OnEnable()
    {
        isEditMode = EditorPrefs.GetBool(EditModePrefKey, true);
    }

    private void OnDisable()
    {
        EditorPrefs.SetBool(EditModePrefKey, isEditMode);
    }

    public void AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Edit Mode"), isEditMode, ToggleMode);
    }

    private void ToggleMode()
    {
        isEditMode = !isEditMode;
        EditorPrefs.SetBool(EditModePrefKey, isEditMode);
        Repaint();
    }

    private void OnFocus()
    {
        LoadNoteForSelectedAsset();
    }

    private void OnSelectionChange()
    {
        LoadNoteForSelectedAsset();
        Repaint();
    }

    private void OnGUI()
    {
        if (Selection.activeObject == null || string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject)))
        {
            EditorGUILayout.HelpBox("Please select an asset in the Project window.", MessageType.Info);
            return;
        }

        GUI.enabled = false;
        GUILayout.Label("Selected Asset: " + Selection.activeObject.name, EditorStyles.label);
        GUI.enabled = true;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (isEditMode)
        {
            EditorGUI.BeginChangeCheck();
            noteText = EditorGUILayout.TextArea(noteText, GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
            {
                SaveOrDeleteNote();
            }
        }
        else
        {
            Color textColor = new GUIStyle(EditorStyles.label).normal.textColor;
            GUIStyle richTextStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
                wordWrap = true,
                normal = { textColor = textColor },  // Default text color
                focused = { textColor = textColor }  // Prevent blue text when clicked
            };
            EditorGUILayout.TextArea(noteText, richTextStyle, GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndScrollView();
    }

    private void LoadNoteForSelectedAsset()
    {
        if (Selection.activeObject == null)
        {
            noteText = "";
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        if (guid != currentAssetGUID)
        {
            currentAssetGUID = guid;
            noteFilePath = GetNoteFilePath(guid);

            if (File.Exists(noteFilePath))
            {
                noteText = File.ReadAllText(noteFilePath);
            }
            else
            {
                noteText = "";
            }
        }
    }

    private void SaveOrDeleteNote()
    {
        string noteFolder = GetNoteFolderPath();
        if (!Directory.Exists(noteFolder))
        {
            Directory.CreateDirectory(noteFolder);
        }

        if (string.IsNullOrEmpty(noteText.Trim()))
        {
            if (File.Exists(noteFilePath))
            {
                File.Delete(noteFilePath);
                AssetDatabase.Refresh();
            }
        }
        else
        {
            File.WriteAllText(noteFilePath, noteText);
            AssetDatabase.Refresh();
        }
    }

    private string GetNoteFolderPath()
    {
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        return Path.Combine(projectPath, "Notes");
    }

    private string GetNoteFilePath(string guid)
    {
        string noteFolder = GetNoteFolderPath();
        return Path.Combine(noteFolder, guid + ".txt");
    }

    private static void EditorGUI_hyperLinkClicked(EditorWindow window, HyperLinkClickedEventArgs args)
    {
        Debug.Log("URL clicked");
        if (window.titleContent.text == "Note Editor")
        {
            string href, GUID;

            var hyperLinkData = args.hyperLinkData;
            hyperLinkData.TryGetValue("href", out href);
            hyperLinkData.TryGetValue("GUID", out GUID);

            if (href != null)
            {
                Application.OpenURL(href);
            }

            if (GUID != null)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(GUID);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                }
            }
        }
    }
}

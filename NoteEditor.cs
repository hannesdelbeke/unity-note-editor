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
    private bool isEditMode = true; // true = Edit Mode; false = View Mode

    [MenuItem("Window/Note Editor")]
    public static void ShowWindow()
    {
       GetWindow<NoteEditor>("Note Editor");
    }

    static NoteEditor()
    {
        EditorGUI.hyperLinkClicked += EditorGUI_hyperLinkClicked;
    }

    // Implement IHasCustomMenu to add a toggle menu item.
    public void AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Edit Mode"), isEditMode, ToggleMode);
    }

    private void ToggleMode()
    {
        isEditMode = !isEditMode;
        Repaint();
    }

    private void OnFocus()
    {
        // Reload note when window is focused in case the selected asset changed.
        LoadNoteForSelectedAsset();
    }

    private void OnSelectionChange()
    {
        // Update note when the selected asset changes.
        LoadNoteForSelectedAsset();
        Repaint();
    }

    private void OnGUI()
    {
        // Check if there is a selected asset
        if (Selection.activeObject == null)
        {
            EditorGUILayout.HelpBox("Please select an asset in the Project window.", MessageType.Info);
            return;
        }

        // Display the current asset name
        GUI.enabled = false;
        GUILayout.Label("Selected Asset: " + Selection.activeObject.name, EditorStyles.label);
        GUI.enabled = true;

        // Begin scroll view for note content.
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
            // get the default unity text color
            Color textColor = new GUIStyle(EditorStyles.label).normal.textColor;


            // In view mode, display the note text using a rich text enabled style.
            GUIStyle richTextStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
                wordWrap = true,
                normal = { textColor = textColor },  // Default text color
                focused = { textColor = textColor } // Prevent blue text when clicked
            };
            EditorGUILayout.TextArea(noteText, richTextStyle, GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Loads the note associated with the currently selected asset.
    /// </summary>
    private void LoadNoteForSelectedAsset()
    {
        if (Selection.activeObject == null)
        {
            noteText = "";
            return;
        }

        // Get the asset path and then the GUID.
        string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        string guid = AssetDatabase.AssetPathToGUID(assetPath);

        // Only reload if the asset GUID has changed.
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

    /// <summary>
    /// Saves the note if it has text; otherwise, deletes the note file if it exists.
    /// </summary>
    private void SaveOrDeleteNote()
    {
        // Ensure the directory exists.
        string noteFolder = GetNoteFolderPath();
        if (!Directory.Exists(noteFolder))
        {
            Directory.CreateDirectory(noteFolder);
        }

        if (string.IsNullOrEmpty(noteText.Trim()))
        {
            // If text is empty and file exists, delete it.
            if (File.Exists(noteFilePath))
            {
                File.Delete(noteFilePath);
                AssetDatabase.Refresh();
            }
        }
        else
        {
            // Save the note text to the file.
            File.WriteAllText(noteFilePath, noteText);
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// Returns the full path for the notes folder, which is one level above the Assets folder.
    /// </summary>
    private string GetNoteFolderPath()
    {
        // Application.dataPath returns something like "C:/.../MyProject/Assets"
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        return Path.Combine(projectPath, "Notes");
    }

    /// <summary>
    /// Returns the full file path for the note corresponding to a given asset GUID.
    /// </summary>
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
            string href, url, GUID;

            var hyperLinkData = args.hyperLinkData;
            hyperLinkData.TryGetValue("href", out href);
            hyperLinkData.TryGetValue("url", out url);
            hyperLinkData.TryGetValue("GUID", out GUID);

            // load URLs in browser
            if (url != null) {
                Application.OpenURL(url);
            }
            if (href != null)
            {
                Application.OpenURL(href);
            }

            // select asset in project window
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

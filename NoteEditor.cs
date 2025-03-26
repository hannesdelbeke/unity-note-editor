using UnityEngine;
using UnityEditor;
using System.IO;

public class NoteEditor : EditorWindow
{
    private string noteText = "";
    private Vector2 scrollPosition;
    private string noteFilePath = "";
    private string currentAssetGUID = "";

    [MenuItem("Window/Note Editor")]
    public static void ShowWindow()
    {
        GetWindow<NoteEditor>("Note Editor");
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
        GUILayout.Label("Selected Asset: " + Selection.activeObject.name, EditorStyles.label);

        // Text field inside a scroll view for editing notes
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUI.BeginChangeCheck();
        noteText = EditorGUILayout.TextArea(noteText, GUILayout.ExpandHeight(true));
        if (EditorGUI.EndChangeCheck())
        {
            SaveOrDeleteNote();
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
}

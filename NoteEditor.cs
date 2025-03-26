using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Security.Policy;
using System.Collections.Generic;

public class NoteEditor : EditorWindow, IHasCustomMenu
{
    private string noteText = "";
    private Vector2 scrollPosition;
    private string noteFilePath = "";
    private string currentAssetGUID = "";
    private bool isEditMode;
    private const string EditModePrefKey = "NoteEditor_IsEditMode";

    private Stack<string> undoStack = new Stack<string>();
    private Stack<string> redoStack = new Stack<string>();

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
        EditorApplication.delayCall += () =>
        {
            isEditMode = !isEditMode;
            EditorPrefs.SetBool(EditModePrefKey, isEditMode);
            Repaint();
        };
    }

    private void OnFocus()
    {
        EditorApplication.delayCall += () =>
        {
            LoadNoteForSelectedAsset();
            Repaint();
        };
    }

    private void OnSelectionChange()
    {
        // clear undo & redo stacks
        undoStack.Clear();
        redoStack.Clear();

        EditorApplication.delayCall += () =>
        {
            LoadNoteForSelectedAsset();
            Repaint();
        };
    }

    private void drawToggleButton()
    {
        GUIContent iconContent = isEditMode
            ? EditorGUIUtility.IconContent("d_scenevis_visible_hover@2x")  // View mode icon
            : EditorGUIUtility.IconContent("TrueTypeFontImporter Icon");  // Edit mode icon

        // Access the texture from the icon content and scale it
        Texture2D iconTexture = iconContent.image as Texture2D;

        Rect iconRect = GUILayoutUtility.GetRect(20, 20); // Define the size for the icon

        GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
        };

        if (GUI.Button(new Rect(iconRect.x, iconRect.y, iconRect.width, iconRect.height), GUIContent.none, buttonStyle))
        {
            ToggleMode(); // Handle toggle action
        }

        // draw the texture on top of our button
        if (iconTexture != null)
        {
            // scale rect slightly smaller than button. then offset it to center it
            iconRect = new Rect(iconRect.x + 2, iconRect.y + 2, iconRect.width - 4, iconRect.height - 4);
            // Create a scaled version of the texture
            GUI.DrawTexture(iconRect, iconTexture, ScaleMode.ScaleToFit);
        }
    }


    private void DrawTopBar()
    {
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = false;
        GUILayout.Label("Selected Asset: " + Selection.activeObject.name, EditorStyles.label);
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        drawToggleButton();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawNoteEditor()
    {
        EditorGUI.BeginChangeCheck();
        string newText = EditorGUILayout.TextArea(noteText, GUILayout.ExpandHeight(true));

        if (EditorGUI.EndChangeCheck())
        {
            // Push current text to undo stack before changing
            undoStack.Push(noteText);
            redoStack.Clear(); // Clear redo stack on new edit

            noteText = newText;
            SaveOrDeleteNote();
        }
    }

    private void DrawNoteViewer()
    {
        Color textColor = new GUIStyle(EditorStyles.label).normal.textColor;
        GUIStyle richTextStyle = new GUIStyle(EditorStyles.label)
        {
            richText = true,
            wordWrap = true,
            normal = { textColor = textColor },  // Default text color
            focused = { textColor = textColor },  // Prevent blue text when clicked
            padding = new RectOffset(3, 2, 2, 2)  // match text edit area padding
        };
        EditorGUILayout.TextArea(noteText, richTextStyle, GUILayout.ExpandHeight(true));
        GUILayout.FlexibleSpace();
    }


    private void OnGUI()
    {
        if (Selection.activeObject == null || string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject)))
        {
            EditorGUILayout.HelpBox("Please select an asset in the Project window.", MessageType.Info);
            return;
        }

        DrawTopBar();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (isEditMode)
        {
            DrawNoteEditor();
        }
        else
        {
            DrawNoteViewer();
        }
        EditorGUILayout.EndScrollView();

        DetectUndoKeys();

    }

    private void LoadNoteForSelectedAsset()
    {
        if (Selection.activeObject == null)
        {
            noteText = "";
            currentAssetGUID = "";
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

    private void Undo()
    {
        if (undoStack.Count > 0)
        {
            redoStack.Push(noteText);
            noteText = undoStack.Pop();

            // lose focus to prevent text area from keeping text in cache
            GUI.FocusControl(null);
            // refocus
            GUI.FocusControl("NoteTextArea");

            Repaint();
        }
    }

    private void Redo()
    {
        if (redoStack.Count > 0)
        {
            undoStack.Push(noteText);
            noteText = redoStack.Pop();
            Repaint();
        }
    }

    private void DetectUndoKeys()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.control)
        {
            if (e.keyCode == KeyCode.Z)
            {
                Undo();
                e.Use(); // Mark event as used
            }
            else if (e.keyCode == KeyCode.Y)
            {
                Redo();
                e.Use();
            }
        }
    }

    [MenuItem("Assets/Copy GUID", false, 19)]
    private static void CopyGUID()
    {
        // copy the GUID from the selected asset to the clipboard
        if (Selection.activeObject != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            EditorGUIUtility.systemCopyBuffer = guid;
        }
    }

    // Validate the menu item defined by the function above.
    // The menu item will be disabled if this function returns false.
    [MenuItem("Assets/Copy GUID", true)]
    private static bool ValidateCopyGUID()
    {
        // Return false if valid asset selected, and ensure a valid GUID
        return Selection.activeObject != null && !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject)));
    }
}

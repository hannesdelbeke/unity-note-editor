# Unity note editor
Attach notes to Unity assets & edit them in editor.

## Quickstart
1. Open from the menu `Window/Note Editor`
2. Select an asset, and type a note.  
  ![image](https://github.com/user-attachments/assets/55903c4a-9dd6-45ca-add8-21c08a164de0)
3. To see richtext, click the eye button, to swap from edit to view mode  
![image](https://github.com/user-attachments/assets/893e6777-7a9b-4df1-b3ec-92790124b759)


### Rich text
You can add rich text to your note, example:
```
this is <b>bold</b> and <color=#ff0000ff>colorfull</color>
<a href="www.google.com">search<a>
click <a GUID="4de8532b09db6114d83c41762cfa7db5">here<a> to select an asset with GUID 4de8532b09db6114d83c41762cfa7db5
```

| | |
|--|--|
|open url in browser | `<a href="www.google.com">search</a>`|
|link to select assets | `<a GUID="123456">select asset</a>` |

see the Unity [documentation](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html) for more rich text info

## Other
Similar to my previous project [unity-folder-notes](https://github.com/hannesdelbeke/unity-folder-notes), differences:
- No custom inspectors, which required multiple implementations for each asset type, and broke some inspectors.
- Notes live in external files, which can be gitignored to be kept private, or commited to be shared.

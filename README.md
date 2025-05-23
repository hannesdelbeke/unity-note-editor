# Unity note editor
Attach notes to Unity assets & edit them in editor.  
- Notes can have **rich** _text_ and [URLs](https://github.com/hannesdelbeke/unity-note-editor).  
- Can be combined with https://github.com/hannesdelbeke/unity-deeplink to support URLs to other assets 

## Quickstart
1. Open from the menu `Window/Note Editor`
2. Select an asset, and type a note.
  
![image](https://github.com/user-attachments/assets/995c40c9-eb67-4bd2-8892-5a20b7cbfc1a)

### Rich text
You can add rich text to your note, example:
```
this is <b>bold</b> and <color=#ff0000ff>colorfull</color>
```
- see the Unity [documentation](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html)
- click ... to toggle between Edit & View mode ![image](https://github.com/user-attachments/assets/41fa915f-ebe3-4f74-ae72-d4dc792662e0)

### URLs

| | |
|--|--|
|open url in browser | `<a href="www.google.com">search</a>`|
|link to select assets | `<a GUID="123456">select asset</a>` |


## Other
Similar to my previous project [unity-folder-notes](https://github.com/hannesdelbeke/unity-folder-notes), differences:
- No custom inspectors, which required multiple implementations for each asset type, and broke some inspectors.
- Notes live in external files, which can be gitignored to be kept private, or commited to be shared.

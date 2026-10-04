# Folders for Unity Hierarchy

Specialized folder objects for Unity Hierarchy.

## Installation

Requires Unity 6000.5 or newer (the UI Toolkit Hierarchy window). For older
editors, use the `v0.6.0` tag, the last version that supports 6000.3 and the
legacy IMGUI Hierarchy:

```json
"io.redstart.hierarchy-folders": "https://github.com/redstartinteractive/unity-hierarchy-folders.git#v0.6.0"
```

This uses the new UPM system. The old copy-into-Assets method still works
perfectly decent so if you don't want to bother with UPM just copy the `Editor`
and `Runtime` folders into your project.

To add this project, add a [git dependency][1] in your `manifest.json`:

```json
{
  "dependencies": {
    "com.unity.package-manager-ui": "1.9.11",
    "io.redstart.hierarchy-folders": "https://github.com/redstartinteractive/unity-hierarchy-folders.git"
  }
}
```

A "New Folder" menu item should show up in the GameObject menu. In the Hierarchy
right-click menu the folder commands get their own section: **New Folder**, plus
**New Folder With Selection** (moves the selection into the new folder) and
**Send To Folder** when something is selected. Cmd/Ctrl+G folders the selection,
or creates an empty folder if nothing is selected (rebind under Shortcuts →
Hierarchy Folders). Add
`Tests/Example.unity` to your current scene for an example of what hierarchy
folders can do for you.

The UPM does not have much documentation at the moment so it probably will be
buggy, you're not going crazy!

[1]: https://forum.unity.com/threads/git-support-on-package-manager.573673/#post-3819487

### OpenUPM

Please note that this is a third party service, which means that Unity
Technologies will not provide support. Always be mindful when considering
unofficial plugins and tools.

```
$ openupm add com.xsduan.hierarchy-folders
```

To install OpenUPM, please see the [documentation][2].

[2]: https://openupm.com/docs/

## Stripping Modes

You can choose how exactly the folder will be removed from the hierarchy in **Project Settings -> Hierarchy Folders**. Stripping settings are stored per project in `ProjectSettings/Packages/io.redstart.hierarchy-folders/Settings.json`, so commit that file to share them with your team.

The following stripping modes are available:

- **Prepend With Folder Name** - The folder will be removed, and all child objects will be prepended with the folder name (e.g. childObject => Folder/childObject). This is the default behaviour.
- **Delete** - The folder will be removed, and names of child objects will not change.
- **Do Nothing** *(available only for Play Mode)* - The folder will not be removed, the hierarchy will not change in play mode. Use this mode if you don't need extra performance in Editor.
- **Replace With Separator** *(available only for Play Mode)* - The hierarchy will flatten, and the folder will be replaced with a separator (e.g. "--- FOLDER ---"). Useful if you need extra performance in Editor but still want to see what folder game objects belong to.

## Stripping folders from prefabs

With this plugin, it is possible to strip folders from prefabs that are not present in the scene but are instantiated at runtime. Upon entering Play Mode, the plugin goes through all prefabs containing folders and strips them. On exiting Play Mode, the changes are reverted. It shouldn't add significant overhead unless you have thousands of prefabs with folders inside, but if entering Play Mode takes too long, you can try disabling this option in **Project Settings -> Hierarchy Folders**. You can also choose whether to strip folders from prefabs before they are packed into a build.

## Display preferences

How folders look in the Hierarchy window is a per-user preference, under **Preferences -> Hierarchy Folders**:

- **Show Folder Icons** - Replace the folder GameObject's icon with a folder icon tinted with its color.
- **Show Colored Row Backgrounds** - Tint the whole row of folders that have a color set. Folders without a color are never tinted.
- **Row Background Opacity** - How strongly the row is tinted.

## Possible FAQs

### Why folders in the first place?

As projects get bigger, they tend to get cluttered in the scene. It's very
helpful if you can group them together into logical groups.

#### Why delete them on build then?

Because they are best used for level designers to declutter the hierarchy, but
calculating the global transform from the local during runtime can take a
noticeable impact on performance once scenes get to 1000, 10000, or more
objects.

#### So why can't I just use empty GameObjects and delete them on build?

Sometimes empty GameObjects are used for other things and it's useful to have a
specific type of object that should always be deleted on build.

Besides, I did all the legwork, so you wouldn't have to!

### There's another product/widget that exists that does this exact task.

So there are. This isn't exactly a unique concept and I only made it for future
personal use and shared it only to possibly to help other people because I
couldn't find it on Google.

If you are the owner of one such product, please contact me and we can work
something out.

The hope is to have it be a native component like it is in Unreal. (Not
necessarily this one specifically, but I'm not opposed to it ;) I've seen paid
components for this and frankly for the effort it took me it's a bit of a
rip-off to pay any amount for it.

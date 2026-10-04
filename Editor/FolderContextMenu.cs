using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityHierarchyFolders.Editor
{
    /// <summary>
    /// Gathers the folder commands into their own separated section of the Hierarchy context menu. Selection-based
    /// commands ("New Folder With Selection", "Send To Folder") only appear when something is selected.
    /// </summary>
    [InitializeOnLoad]
    internal static class FolderContextMenu
    {
        // Static GameObject/ menu items that Unity copies into this menu; replaced by the section below.
        private static readonly string[] _staticMenuLabels =
        {
            FolderEditorUtils.NewFolderLabel,
            FolderEditorUtils.NewFolderWithSelectionLabel,
            FolderEditorUtils.SendToFolderLabel,
        };

        private const string _createEmptyLabel = "Create Empty";

        static FolderContextMenu()
        {
            HierarchyWindow.PopulateContextMenu += OnPopulateContextMenu;
        }

        private static void OnPopulateContextMenu(HierarchyWindow window, HierarchyView view, HierarchyViewItem item,
            DropdownMenu menu)
        {
            // Scene headers build their own create menu; only handle GameObject rows and empty space.
            if (item != null && item.Handler is not HierarchyGameObjectHandler)
                return;

            int insertAt = RemoveStaticFolderItems(menu);
            if (insertAt < 0)
                insertAt = Mathf.Max(0, IndexOf(menu, _createEmptyLabel));

            List<DropdownMenuItem> items = menu.MenuItems();
            if (insertAt > 0 && items[insertAt - 1] is not DropdownMenuSeparator)
                menu.InsertSeparator(string.Empty, insertAt++);

            menu.InsertAction(insertAt++, FolderEditorUtils.NewFolderLabel,
                _ => FolderEditorUtils.CreateFolder(includeSelection: false), DropdownMenuAction.Status.Normal);

            if (FolderEditorUtils.HasSceneSelection())
            {
                menu.InsertAction(insertAt++, FolderEditorUtils.NewFolderWithSelectionLabel,
                    _ => FolderEditorUtils.CreateFolder(includeSelection: true), DropdownMenuAction.Status.Normal);
                menu.InsertAction(insertAt++, FolderEditorUtils.SendToFolderMenuName,
                    _ => SelectHierarchyFolderEditor.ShowWindow(),
                    FolderEditorUtils.CanSendToFolder() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }

            items = menu.MenuItems();
            if (insertAt < items.Count && items[insertAt] is not DropdownMenuSeparator)
                menu.InsertSeparator(string.Empty, insertAt);
        }

        /// <summary>Removes the copied static folder items and returns the index of the first one, or -1.</summary>
        private static int RemoveStaticFolderItems(DropdownMenu menu)
        {
            List<DropdownMenuItem> items = menu.MenuItems();
            int firstIndex = -1;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i] is DropdownMenuAction action && IsStaticFolderItem(action.name))
                {
                    menu.RemoveItemAt(i);
                    firstIndex = i;
                }
            }

            return firstIndex;
        }

        private static bool IsStaticFolderItem(string menuName)
        {
            string label = LabelWithoutHotkey(menuName);
            return System.Array.IndexOf(_staticMenuLabels, label) >= 0;
        }

        private static int IndexOf(DropdownMenu menu, string label)
        {
            List<DropdownMenuItem> items = menu.MenuItems();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is DropdownMenuAction action && LabelWithoutHotkey(action.name) == label)
                    return i;
            }

            return -1;
        }

        // Menu names carry Unity's hotkey suffix, e.g. "Send To Folder %#&M".
        private static string LabelWithoutHotkey(string menuName)
        {
            int space = menuName.LastIndexOf(' ');
            bool hasHotkey = space >= 0 && space + 1 < menuName.Length && "%#&_".IndexOf(menuName[space + 1]) >= 0;
            return hasHotkey ? menuName.Substring(0, space) : menuName;
        }
    }
}

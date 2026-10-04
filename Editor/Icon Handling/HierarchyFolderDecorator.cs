using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityHierarchyFolders.Runtime;

namespace UnityHierarchyFolders.Editor {
    [InitializeOnLoad]
    public static class HierarchyFolderDecorator
    {
        // Rows currently styled as folders, so they can be restyled without waiting for the Hierarchy to rebind them.
        private static readonly Dictionary<HierarchyViewItem, GameObject> _decoratedItems = new();
        private static readonly List<HierarchyViewItem> _detachedItems = new();

        static HierarchyFolderDecorator()
        {
            HierarchyWindow.BindViewItem   += OnBindViewItem;
            HierarchyWindow.UnbindViewItem += OnUnbindViewItem;
            DisplaySettings.Changed        += RedecorateAll;
            Folder.ColorChanged            += OnFolderColorChanged;
        }

        private static void OnBindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            if (item.Handler is not HierarchyGameObjectHandler handler)
                return;

            GameObject go = handler.GetGameObject(item.Node);
            if (Decorate(item, go))
                _decoratedItems[item] = go;
        }

        private static void OnUnbindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            if (_decoratedItems.Remove(item))
                ClearStyle(item);
        }

        // The Hierarchy doesn't rebind a row when a property changes, so restyle the folder's rows directly.
        private static void OnFolderColorChanged(Folder folder)
        {
            foreach ((HierarchyViewItem item, GameObject go) in _decoratedItems)
            {
                if (go == folder.gameObject)
                    Decorate(item, go);
            }
        }

        private static void RedecorateAll()
        {
            foreach ((HierarchyViewItem item, GameObject go) in _decoratedItems)
            {
                if (item.panel == null)
                    _detachedItems.Add(item);
                else
                    Decorate(item, go);
            }

            // Rows of a closed Hierarchy window may never get an unbind; don't keep them alive.
            foreach (HierarchyViewItem item in _detachedItems)
                _decoratedItems.Remove(item);
            _detachedItems.Clear();
        }

        /// <summary>Sets the folder styling for the row's current state; false if the GameObject isn't a folder.</summary>
        private static bool Decorate(HierarchyViewItem item, GameObject go)
        {
            if (go == null || !Folder.TryGetIconIndex(go, out int colorIndex))
                return false;

            Texture2D icon = DisplaySettings.ShowFolderIcons ? HierarchyFolderIcon.ColoredFolderIcon(colorIndex) : null;
            item.Icon.style.backgroundImage = icon != null ? new StyleBackground(icon) : StyleKeyword.Null;

            // Color index 0 means "no color": leave the row's own background alone.
            if (DisplaySettings.ShowRowBackgrounds && colorIndex > 0)
            {
                Color bgColor = HierarchyFolderIcon.FolderColor(colorIndex);
                bgColor.a = DisplaySettings.RowBackgroundOpacity;
                item.style.backgroundColor = new StyleColor(bgColor);
            }
            else
            {
                item.style.backgroundColor = StyleKeyword.Null;
            }

            return true;
        }

        private static void ClearStyle(HierarchyViewItem item)
        {
            item.Icon.style.backgroundImage = StyleKeyword.Null;
            item.style.backgroundColor = StyleKeyword.Null;
        }
    }
}

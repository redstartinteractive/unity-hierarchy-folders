using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityHierarchyFolders.Runtime;

namespace UnityHierarchyFolders.Editor {
#if UNITY_6000_5_OR_NEWER
    [InitializeOnLoad]
    public static class HierarchyFolderDecorator
    {
        // Items currently styled as folders, so a preference change can restyle them without a rebind.
        private static readonly Dictionary<HierarchyViewItem, HierarchyView> _decoratedItems = new();

        static HierarchyFolderDecorator()
        {
            HierarchyWindow.BindViewItem   += OnBindViewItem;
            HierarchyWindow.UnbindViewItem += OnUnbindViewItem;
            DisplaySettings.Changed        += OnDisplaySettingsChanged;
        }

        private static void OnBindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            if (Decorate(view, item))
                _decoratedItems[item] = view;
        }

        private static void OnUnbindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            if (_decoratedItems.Remove(item))
                ClearStyle(item);
        }

        private static void OnDisplaySettingsChanged()
        {
            foreach ((HierarchyViewItem item, HierarchyView view) in _decoratedItems)
            {
                ClearStyle(item);
                Decorate(view, item);
            }
        }

        private static bool Decorate(HierarchyView view, HierarchyViewItem item)
        {
            if (item.Handler is not HierarchyGameObjectHandler handler)
                return false;

            GameObject go = handler.GetGameObject(item.Node);

            if (go == null || !Folder.TryGetIconIndex(go, out int colorIndex))
                return false;

            if (DisplaySettings.ShowFolderIcons)
            {
                bool isExpanded = view.ViewModel.HasFlags(item.Node, HierarchyNodeFlags.Expanded);
                (Texture2D open, Texture2D closed) = HierarchyFolderIcon.ColoredFolderIcons(colorIndex);
                item.Icon.style.backgroundImage = new StyleBackground(isExpanded ? open : closed);
            }

            // Color index 0 means "no color": leave the row's own background alone.
            if (DisplaySettings.ShowRowBackgrounds && colorIndex > 0)
            {
                Color bgColor = HierarchyFolderIcon.FolderColor(colorIndex);
                bgColor.a = DisplaySettings.RowBackgroundOpacity;
                item.style.backgroundColor = new StyleColor(bgColor);
            }

            return true;
        }

        private static void ClearStyle(HierarchyViewItem item)
        {
            item.Icon.style.backgroundImage = StyleKeyword.Null;
            item.style.backgroundColor = StyleKeyword.Null;
        }
    }
#endif
}

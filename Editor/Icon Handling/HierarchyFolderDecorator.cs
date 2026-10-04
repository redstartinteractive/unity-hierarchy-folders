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
        // Items currently styled as folders, so they can be restyled without waiting for a rebind.
        private static readonly Dictionary<HierarchyViewItem, HierarchyView> _decoratedItems = new();

        static HierarchyFolderDecorator()
        {
            HierarchyWindow.BindViewItem   += OnBindViewItem;
            HierarchyWindow.UnbindViewItem += OnUnbindViewItem;
            DisplaySettings.Changed        += RedecorateAll;
            ObjectChangeEvents.changesPublished += OnChangesPublished;
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

        // The Hierarchy doesn't rebind a row when a property changes, so a new folder color (from the inspector,
        // undo/redo or a script) wouldn't show until the row was rebound some other way.
        private static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; i++)
            {
                if (stream.GetEventType(i) != ObjectChangeKind.ChangeGameObjectOrComponentProperties)
                    continue;

                stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var args);
                Object changed = EditorUtility.EntityIdToObject(args.entityId);
                if (changed is Folder || (changed is GameObject go && go.TryGetComponent<Folder>(out _)))
                {
                    RedecorateAll();
                    return;
                }
            }
        }

        private static void RedecorateAll()
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
                Texture2D icon = isExpanded ? open : closed;
                if (icon != null)
                    item.Icon.style.backgroundImage = new StyleBackground(icon);
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
}

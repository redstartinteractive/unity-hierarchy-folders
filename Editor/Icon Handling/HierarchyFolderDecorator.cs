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
        static HierarchyFolderDecorator()
        {
            HierarchyWindow.BindViewItem   += OnBindViewItem;
            HierarchyWindow.UnbindViewItem += OnUnbindViewItem;
        }

        private static void OnBindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            if (item.Handler is not HierarchyGameObjectHandler handler)
                return;

            GameObject go = handler.GetGameObject(item.Node);

            if (go == null || !Folder.TryGetIconIndex(go, out int colorIndex))
                return;

            bool isExpanded = view.ViewModel.HasFlags(item.Node, HierarchyNodeFlags.Expanded);
            (Texture2D open, Texture2D closed) = HierarchyFolderIcon.ColoredFolderIcons(colorIndex);

            item.Icon.style.backgroundImage = new StyleBackground(isExpanded ? open : closed);

            Color bgColor = HierarchyFolderIcon.FolderColor(colorIndex);
            bgColor.a = 0.5f;
            item.style.backgroundColor = new StyleColor(bgColor);
        }

        private static void OnUnbindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            item.Icon.style.backgroundImage = StyleKeyword.Null;
            item.style.backgroundColor = StyleKeyword.Null;
        }
    }
#endif
}

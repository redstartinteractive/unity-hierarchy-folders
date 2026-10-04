namespace UnityHierarchyFolders.Editor
{
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>Loads the package's shared editor stylesheet.</summary>
    internal static class FolderStyles
    {
        private const string StyleSheetPath = "Packages/" + PackageSettings.PackageName + "/Editor/UI/HierarchyFolders.uss";

        private static StyleSheet _styleSheet;

        public static void Apply(VisualElement element)
        {
            if (_styleSheet == null)
                _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);

            if (_styleSheet != null)
                element.styleSheets.Add(_styleSheet);
        }

        /// <summary>A row with an icon and a label, as used by the package's list views.</summary>
        public static VisualElement CreateIconRow()
        {
            var row = new VisualElement();
            row.AddToClassList("hf-row");
            var icon = new Image { name = "icon", pickingMode = PickingMode.Ignore };
            icon.AddToClassList("hf-row__icon");
            row.Add(icon);
            row.Add(new Label { name = "label", pickingMode = PickingMode.Ignore });
            return row;
        }
    }
}

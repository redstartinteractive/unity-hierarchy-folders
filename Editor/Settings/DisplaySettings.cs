namespace UnityHierarchyFolders.Editor
{
    using System;
    using UnityEditor;
    using UnityEditor.SettingsManagement;

    /// <summary>
    /// Per-user preferences for how folders are drawn in the Hierarchy window. Purely cosmetic, so they live in
    /// EditorPrefs and are never committed.
    /// </summary>
    internal static class DisplaySettings
    {
        private static readonly UserSetting<bool> _showFolderIcons = Create("display.showFolderIcons", true);
        private static readonly UserSetting<bool> _showRowBackgrounds = Create("display.showRowBackgrounds", true);
        private static readonly UserSetting<float> _rowBackgroundOpacity = Create("display.rowBackgroundOpacity", 0.5f);

        /// <summary>Raised after any display preference changes, so the Hierarchy can be redrawn.</summary>
        public static event Action Changed;

        /// <summary>Replace the GameObject icon of folders with a (colored) folder icon.</summary>
        public static bool ShowFolderIcons
        {
            get => _showFolderIcons.value;
            set => Set(_showFolderIcons, value);
        }

        /// <summary>Tint the whole Hierarchy row of a colored folder.</summary>
        public static bool ShowRowBackgrounds
        {
            get => _showRowBackgrounds.value;
            set => Set(_showRowBackgrounds, value);
        }

        public static float RowBackgroundOpacity
        {
            get => _rowBackgroundOpacity.value;
            set => Set(_rowBackgroundOpacity, value);
        }

        private static void Set<T>(UserSetting<T> setting, T value)
        {
            setting.value = value;
            Changed?.Invoke();
        }

        private static UserSetting<T> Create<T>(string key, T defaultValue) =>
            new UserSetting<T>(PackageSettings.Instance, key, defaultValue, SettingsScope.User);
    }
}

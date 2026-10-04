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
        private static UserSetting<bool> _showFolderIcons;
        private static UserSetting<bool> _showRowBackgrounds;
        private static UserSetting<float> _rowBackgroundOpacity;

        /// <summary>Raised after any display preference changes, so the Hierarchy can be redrawn.</summary>
        public static event Action Changed;

        /// <summary>Replace the GameObject icon of folders with a (colored) folder icon.</summary>
        public static bool ShowFolderIcons
        {
            get => Get(ref _showFolderIcons).value;
            set => Set(ref _showFolderIcons, value);
        }

        /// <summary>Tint the whole Hierarchy row of a colored folder (new Hierarchy window only).</summary>
        public static bool ShowRowBackgrounds
        {
            get => Get(ref _showRowBackgrounds).value;
            set => Set(ref _showRowBackgrounds, value);
        }

        public static float RowBackgroundOpacity
        {
            get => Get(ref _rowBackgroundOpacity).value;
            set => Set(ref _rowBackgroundOpacity, value);
        }

        private static UserSetting<T> Get<T>(ref UserSetting<T> setting)
        {
            InitializeIfNeeded();
            return setting;
        }

        private static void Set<T>(ref UserSetting<T> setting, T value)
        {
            InitializeIfNeeded();
            setting.value = value;
            Changed?.Invoke();
        }

        private static void InitializeIfNeeded()
        {
            if (_showFolderIcons != null)
                return;

            Settings settings = PackageSettings.Instance;
            _showFolderIcons = new UserSetting<bool>(settings, "display.showFolderIcons", true, SettingsScope.User);
            _showRowBackgrounds = new UserSetting<bool>(settings, "display.showRowBackgrounds", true, SettingsScope.User);
            _rowBackgroundOpacity =
                new UserSetting<float>(settings, "display.rowBackgroundOpacity", 0.5f, SettingsScope.User);
        }
    }
}

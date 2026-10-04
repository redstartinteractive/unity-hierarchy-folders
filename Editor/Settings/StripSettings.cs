namespace UnityHierarchyFolders.Editor
{
    using Runtime;
    using UnityEditor;
    using UnityEditor.SettingsManagement;

    /// <summary>
    /// Folder stripping settings. These change how the project behaves in Play Mode and builds, so they are
    /// project-scoped and meant to be committed alongside the project.
    /// </summary>
    internal static class StripSettings
    {
        private static UserSetting<StrippingMode> _playModeSetting;
        private static UserSetting<StrippingMode> _buildSetting;
        private static UserSetting<bool> _capitalizeName;
        private static UserSetting<bool> _stripFoldersFromPrefabsInPlayMode;
        private static UserSetting<bool> _stripFoldersFromPrefabsInBuild;

        public static StrippingMode PlayMode
        {
            get => Get(ref _playModeSetting).value;
            set => Set(ref _playModeSetting, value);
        }

        public static StrippingMode Build
        {
            get => Get(ref _buildSetting).value;
            set => Set(ref _buildSetting, value);
        }

        public static bool CapitalizeName
        {
            get => Get(ref _capitalizeName).value;
            set => Set(ref _capitalizeName, value);
        }

        public static bool StripFoldersFromPrefabsInPlayMode
        {
            get => Get(ref _stripFoldersFromPrefabsInPlayMode).value;
            set => Set(ref _stripFoldersFromPrefabsInPlayMode, value);
        }

        public static bool StripFoldersFromPrefabsInBuild
        {
            get => Get(ref _stripFoldersFromPrefabsInBuild).value;
            set => Set(ref _stripFoldersFromPrefabsInBuild, value);
        }

        private static UserSetting<T> Get<T>(ref UserSetting<T> setting)
        {
            InitializeIfNeeded();
            return setting;
        }

        private static void Set<T>(ref UserSetting<T> setting, T value)
        {
            InitializeIfNeeded();
            setting.SetValue(value, saveProjectSettingsImmediately: true);
        }

        private static void InitializeIfNeeded()
        {
            if (_playModeSetting != null)
                return;

            Settings settings = PackageSettings.Instance;

            _playModeSetting = Create(settings, nameof(_playModeSetting), StrippingMode.DoNothing);
            _buildSetting = Create(settings, nameof(_buildSetting), StrippingMode.Delete);
            _capitalizeName = Create(settings, nameof(_capitalizeName), true);
            _stripFoldersFromPrefabsInPlayMode = Create(settings, nameof(_stripFoldersFromPrefabsInPlayMode), true);
            _stripFoldersFromPrefabsInBuild = Create(settings, nameof(_stripFoldersFromPrefabsInBuild), true);
        }

        private static UserSetting<T> Create<T>(Settings settings, string key, T defaultValue) =>
            new UserSetting<T>(settings, key, defaultValue, SettingsScope.Project);
    }
}

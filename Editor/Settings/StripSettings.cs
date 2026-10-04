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
        private static readonly UserSetting<StrippingMode> _playModeSetting =
            Create(nameof(_playModeSetting), StrippingMode.DoNothing);
        private static readonly UserSetting<StrippingMode> _buildSetting =
            Create(nameof(_buildSetting), StrippingMode.Delete);
        private static readonly UserSetting<bool> _capitalizeName = Create(nameof(_capitalizeName), true);
        private static readonly UserSetting<bool> _stripFoldersFromPrefabsInPlayMode =
            Create(nameof(_stripFoldersFromPrefabsInPlayMode), true);
        private static readonly UserSetting<bool> _stripFoldersFromPrefabsInBuild =
            Create(nameof(_stripFoldersFromPrefabsInBuild), true);

        public static StrippingMode PlayMode
        {
            get => _playModeSetting.value;
            set => _playModeSetting.SetValue(value, saveProjectSettingsImmediately: true);
        }

        public static StrippingMode Build
        {
            get => _buildSetting.value;
            set => _buildSetting.SetValue(value, saveProjectSettingsImmediately: true);
        }

        public static bool CapitalizeName
        {
            get => _capitalizeName.value;
            set => _capitalizeName.SetValue(value, saveProjectSettingsImmediately: true);
        }

        public static bool StripFoldersFromPrefabsInPlayMode
        {
            get => _stripFoldersFromPrefabsInPlayMode.value;
            set => _stripFoldersFromPrefabsInPlayMode.SetValue(value, saveProjectSettingsImmediately: true);
        }

        public static bool StripFoldersFromPrefabsInBuild
        {
            get => _stripFoldersFromPrefabsInBuild.value;
            set => _stripFoldersFromPrefabsInBuild.SetValue(value, saveProjectSettingsImmediately: true);
        }

        private static UserSetting<T> Create<T>(string key, T defaultValue) =>
            new UserSetting<T>(PackageSettings.Instance, key, defaultValue, SettingsScope.Project);
    }
}

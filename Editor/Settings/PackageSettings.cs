namespace UnityHierarchyFolders.Editor
{
    using UnityEditor.SettingsManagement;

    /// <summary>
    /// Single <see cref="Settings"/> instance shared by <see cref="StripSettings"/> (project scope, committed to
    /// ProjectSettings/Packages/io.redstart.hierarchy-folders/Settings.json) and <see cref="DisplaySettings"/>
    /// (user scope, stored in EditorPrefs).
    /// </summary>
    internal static class PackageSettings
    {
        public const string PackageName = "io.redstart.hierarchy-folders";

        private static Settings _instance;

        public static Settings Instance => _instance ??= new Settings(PackageName);
    }
}

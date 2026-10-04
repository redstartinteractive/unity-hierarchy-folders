namespace UnityHierarchyFolders.Editor
{
    using System;
    using System.Collections.Generic;
    using Runtime;
    using UnityEditor;
    using UnityEngine;

    internal static class SettingsDrawer
    {
        private const string ProviderPath = "Hierarchy Folders";

        private static readonly GUIContent _playModeLabel = new GUIContent("Play Mode Stripping Type");
        private static readonly GUIContent _buildLabel = new GUIContent("Build Stripping Type");
        private static readonly GUIContent _capitalizeLabel = new GUIContent("Capitalize Folder Names");
        private static readonly GUIContent _prefabsInPlayModeLabel = new GUIContent("Strip folders from prefabs in Play Mode");
        private static readonly GUIContent _prefabsInBuildLabel = new GUIContent("Strip folders from prefabs in build");

        private static readonly GUIContent _folderIconsLabel = new GUIContent("Show Folder Icons",
            "Replace the GameObject icon of folders with a folder icon tinted with the folder's color.");
        private static readonly GUIContent _rowBackgroundsLabel = new GUIContent("Show Colored Row Backgrounds",
            "Tint the Hierarchy row of folders that have a color set.");
        private static readonly GUIContent _rowOpacityLabel = new GUIContent("Row Background Opacity");

        [SettingsProvider]
        public static SettingsProvider CreateProjectSettingsProvider()
        {
            return new SettingsProvider("Project/" + ProviderPath, SettingsScope.Project)
            {
                guiHandler = OnProjectGUI,
                keywords = GetKeywords(_playModeLabel, _buildLabel, _capitalizeLabel, _prefabsInPlayModeLabel,
                    _prefabsInBuildLabel)
            };
        }

        [SettingsProvider]
        public static SettingsProvider CreatePreferencesProvider()
        {
            return new SettingsProvider("Preferences/" + ProviderPath, SettingsScope.User)
            {
                guiHandler = OnPreferencesGUI,
                keywords = GetKeywords(_folderIconsLabel, _rowBackgroundsLabel, _rowOpacityLabel)
            };
        }

        private static void OnProjectGUI(string searchContext)
        {
            using (new TemporaryLabelWidth(230f))
            {
                EditorGUI.BeginChangeCheck();
                var playMode = (StrippingMode) EditorGUILayout.EnumPopup(_playModeLabel, StripSettings.PlayMode);
                if (EditorGUI.EndChangeCheck())
                    StripSettings.PlayMode = playMode;

                if (StripSettings.PlayMode == StrippingMode.ReplaceWithSeparator)
                {
                    EditorGUI.BeginChangeCheck();
                    bool capitalize = EditorGUILayout.Toggle(_capitalizeLabel, StripSettings.CapitalizeName);
                    if (EditorGUI.EndChangeCheck())
                        StripSettings.CapitalizeName = capitalize;
                }

                EditorGUI.BeginChangeCheck();
                var build = (StrippingMode) EditorGUILayout.EnumPopup(
                    _buildLabel, StripSettings.Build, TypeCanBeInBuild, true);
                if (EditorGUI.EndChangeCheck())
                    StripSettings.Build = build;

                EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

                if (StripSettings.StripFoldersFromPrefabsInPlayMode)
                {
                    EditorGUILayout.HelpBox(
                        "If you notice that entering play mode takes too long, you can try disabling this option. " +
                        "Folders will not be stripped from prefabs that are instantiated at runtime, but if performance in " +
                        "Play Mode does not matter, you will be fine.", MessageType.Info);
                }

                EditorGUI.BeginChangeCheck();
                bool prefabsInPlayMode = EditorGUILayout.Toggle(_prefabsInPlayModeLabel,
                    StripSettings.StripFoldersFromPrefabsInPlayMode);
                if (EditorGUI.EndChangeCheck())
                    StripSettings.StripFoldersFromPrefabsInPlayMode = prefabsInPlayMode;

                EditorGUI.BeginChangeCheck();
                bool prefabsInBuild = EditorGUILayout.Toggle(_prefabsInBuildLabel,
                    StripSettings.StripFoldersFromPrefabsInBuild);
                if (EditorGUI.EndChangeCheck())
                    StripSettings.StripFoldersFromPrefabsInBuild = prefabsInBuild;
            }
        }

        private static void OnPreferencesGUI(string searchContext)
        {
            using (new TemporaryLabelWidth(230f))
            {
                EditorGUI.BeginChangeCheck();
                bool showIcons = EditorGUILayout.Toggle(_folderIconsLabel, DisplaySettings.ShowFolderIcons);
                if (EditorGUI.EndChangeCheck())
                    DisplaySettings.ShowFolderIcons = showIcons;

#if UNITY_6000_5_OR_NEWER
                EditorGUI.BeginChangeCheck();
                bool showBackgrounds = EditorGUILayout.Toggle(_rowBackgroundsLabel, DisplaySettings.ShowRowBackgrounds);
                if (EditorGUI.EndChangeCheck())
                    DisplaySettings.ShowRowBackgrounds = showBackgrounds;

                using (new EditorGUI.DisabledScope(!DisplaySettings.ShowRowBackgrounds))
                {
                    EditorGUI.BeginChangeCheck();
                    float opacity = EditorGUILayout.Slider(_rowOpacityLabel, DisplaySettings.RowBackgroundOpacity, 0f, 1f);
                    if (EditorGUI.EndChangeCheck())
                        DisplaySettings.RowBackgroundOpacity = opacity;
                }
#endif
            }
        }

        private static HashSet<string> GetKeywords(params GUIContent[] labels)
        {
            var keywords = new HashSet<string>();

            foreach (GUIContent label in labels)
            {
                foreach (string word in label.text.Split(' '))
                {
                    keywords.Add(word);
                }
            }

            return keywords;
        }

        private static bool TypeCanBeInBuild(Enum enumValue)
        {
            var mode = (StrippingMode) enumValue;
            return mode == StrippingMode.Delete;
        }

        /// <summary>
        /// Temporarily sets <see cref="EditorGUIUtility.labelWidth"/> to a certain value, than reverts it.
        /// </summary>
        private readonly struct TemporaryLabelWidth : IDisposable
        {
            private readonly float _oldWidth;

            public TemporaryLabelWidth(float width)
            {
                _oldWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = width;
            }

            public void Dispose()
            {
                EditorGUIUtility.labelWidth = _oldWidth;
            }
        }
    }
}

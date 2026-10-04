namespace UnityHierarchyFolders.Editor
{
    using System.Collections.Generic;
    using Runtime;
    using UnityEditor;
    using UnityEngine.UIElements;

    internal static class SettingsDrawer
    {
        private const string ProviderPath = "Hierarchy Folders";
        private static readonly string AlignedFieldClass = BaseField<bool>.alignedFieldUssClassName;

        private const string PlayModeLabel = "Play Mode Stripping Type";
        private const string BuildLabel = "Build Stripping Type";
        private const string CapitalizeLabel = "Capitalize Folder Names";
        private const string PrefabsInPlayModeLabel = "Strip folders from prefabs in Play Mode";
        private const string PrefabsInBuildLabel = "Strip folders from prefabs in build";

        private const string FolderIconsLabel = "Show Folder Icons";
        private const string RowBackgroundsLabel = "Show Colored Row Backgrounds";
        private const string RowOpacityLabel = "Row Background Opacity";

        [SettingsProvider]
        public static SettingsProvider CreateProjectSettingsProvider()
        {
            return new SettingsProvider("Project/" + ProviderPath, SettingsScope.Project)
            {
                activateHandler = (_, root) => BuildProjectSettings(root),
                keywords = GetKeywords(PlayModeLabel, BuildLabel, CapitalizeLabel, PrefabsInPlayModeLabel,
                    PrefabsInBuildLabel)
            };
        }

        [SettingsProvider]
        public static SettingsProvider CreatePreferencesProvider()
        {
            return new SettingsProvider("Preferences/" + ProviderPath, SettingsScope.User)
            {
                activateHandler = (_, root) => BuildPreferences(root),
                keywords = GetKeywords(FolderIconsLabel, RowBackgroundsLabel, RowOpacityLabel)
            };
        }

        private static void BuildProjectSettings(VisualElement root)
        {
            VisualElement panel = CreatePanel(root);

            var playMode = Aligned(new EnumField(PlayModeLabel, StripSettings.PlayMode));
            var capitalize = Aligned(new Toggle(CapitalizeLabel) { value = StripSettings.CapitalizeName });

            // Builds can only delete folders, so this is shown for clarity but not editable.
            var build = Aligned(new EnumField(BuildLabel, StripSettings.Build)
            {
                tooltip = "Folders are always deleted from builds."
            });
            build.SetEnabled(false);

            var prefabsHelp = new HelpBox(
                "If you notice that entering play mode takes too long, you can try disabling this option. " +
                "Folders will not be stripped from prefabs that are instantiated at runtime, but if performance in " +
                "Play Mode does not matter, you will be fine.", HelpBoxMessageType.Info);
            var prefabsInPlayMode = Aligned(new Toggle(PrefabsInPlayModeLabel)
            {
                value = StripSettings.StripFoldersFromPrefabsInPlayMode
            });
            var prefabsInBuild = Aligned(new Toggle(PrefabsInBuildLabel)
            {
                value = StripSettings.StripFoldersFromPrefabsInBuild
            });

            void UpdateVisibility()
            {
                SetVisible(capitalize, StripSettings.PlayMode == StrippingMode.ReplaceWithSeparator);
                SetVisible(prefabsHelp, StripSettings.StripFoldersFromPrefabsInPlayMode);
            }

            playMode.RegisterValueChangedCallback(evt =>
            {
                StripSettings.PlayMode = (StrippingMode) evt.newValue;
                UpdateVisibility();
            });
            capitalize.RegisterValueChangedCallback(evt => StripSettings.CapitalizeName = evt.newValue);
            prefabsInPlayMode.RegisterValueChangedCallback(evt =>
            {
                StripSettings.StripFoldersFromPrefabsInPlayMode = evt.newValue;
                UpdateVisibility();
            });
            prefabsInBuild.RegisterValueChangedCallback(evt => StripSettings.StripFoldersFromPrefabsInBuild = evt.newValue);

            panel.Add(playMode);
            panel.Add(capitalize);
            panel.Add(build);
            panel.Add(Spacer());
            panel.Add(prefabsHelp);
            panel.Add(prefabsInPlayMode);
            panel.Add(prefabsInBuild);
            UpdateVisibility();
        }

        private static void BuildPreferences(VisualElement root)
        {
            VisualElement panel = CreatePanel(root);

            var showIcons = Aligned(new Toggle(FolderIconsLabel)
            {
                value = DisplaySettings.ShowFolderIcons,
                tooltip = "Replace the GameObject icon of folders with a folder icon tinted with the folder's color."
            });
            var showBackgrounds = Aligned(new Toggle(RowBackgroundsLabel)
            {
                value = DisplaySettings.ShowRowBackgrounds,
                tooltip = "Tint the Hierarchy row of folders that have a color set."
            });
            var opacity = Aligned(new Slider(RowOpacityLabel, 0f, 1f)
            {
                value = DisplaySettings.RowBackgroundOpacity,
                showInputField = true
            });
            opacity.SetEnabled(DisplaySettings.ShowRowBackgrounds);

            showIcons.RegisterValueChangedCallback(evt => DisplaySettings.ShowFolderIcons = evt.newValue);
            showBackgrounds.RegisterValueChangedCallback(evt =>
            {
                DisplaySettings.ShowRowBackgrounds = evt.newValue;
                opacity.SetEnabled(evt.newValue);
            });
            opacity.RegisterValueChangedCallback(evt => DisplaySettings.RowBackgroundOpacity = evt.newValue);

            panel.Add(showIcons);
            panel.Add(showBackgrounds);
            panel.Add(opacity);
        }

        private static VisualElement CreatePanel(VisualElement root)
        {
            FolderStyles.Apply(root);

            var panel = new VisualElement();
            panel.AddToClassList("hf-settings");

            var title = new Label(ProviderPath);
            title.AddToClassList("hf-settings__title");
            panel.Add(title);

            root.Add(panel);
            return panel;
        }

        private static T Aligned<T>(T field) where T : VisualElement
        {
            field.AddToClassList(AlignedFieldClass);
            return field;
        }

        private static VisualElement Spacer()
        {
            var spacer = new VisualElement();
            spacer.AddToClassList("hf-settings__spacer");
            return spacer;
        }

        private static void SetVisible(VisualElement element, bool visible) =>
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        private static HashSet<string> GetKeywords(params string[] labels)
        {
            var keywords = new HashSet<string>();

            foreach (string label in labels)
            {
                foreach (string word in label.Split(' '))
                {
                    keywords.Add(word);
                }
            }

            return keywords;
        }
    }
}

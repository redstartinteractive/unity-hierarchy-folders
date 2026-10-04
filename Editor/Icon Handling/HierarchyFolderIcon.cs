using UnityEditor;
using UnityEngine;

namespace UnityHierarchyFolders.Editor {
    /// <summary>The folder color palette and the folder icons tinted with it.</summary>
    public static class HierarchyFolderIcon {
        private const string _folderIconName = "Folder Icon";

        // Index 0 is the untinted "no color" icon; index i > 0 is IconColors entry i - 1.
        private static (Texture2D open, Texture2D closed)[] _coloredFolderIcons;

        private static readonly Color[,] IconColors = {
            { new(0.09f, 0.57f, 0.82f), new(0.05f, 0.34f, 0.48f) },
            { new(0.09f, 0.67f, 0.67f), new(0.05f, 0.42f, 0.42f) },
            { new(0.23f, 0.73f, 0.36f), new(0.15f, 0.41f, 0.22f) },
            { new(0.55f, 0.35f, 0.71f), new(0.35f, 0.24f, 0.44f) },
            { new(0.78f, 0.27f, 0.55f), new(0.52f, 0.15f, 0.35f) },
            { new(0.80f, 0.66f, 0.10f), new(0.56f, 0.46f, 0.02f) },
            { new(0.91f, 0.49f, 0.13f), new(0.62f, 0.33f, 0.07f) },
            { new(0.91f, 0.30f, 0.24f), new(0.77f, 0.15f, 0.09f) },
            { new(0.35f, 0.49f, 0.63f), new(0.24f, 0.33f, 0.42f) }
        };

        public static int IconColumnCount => IconColors.GetLength(0);
        public static int IconRowCount => IconColors.GetLength(1);
        public static int ColorCount => IconColumnCount * IconRowCount;

        /// <summary>Icons for a color index, or default if it's out of range or the icons can't be built yet.</summary>
        public static (Texture2D open, Texture2D closed) ColoredFolderIcons(int i)
        {
            if (!TryInitIcons() || i < 0 || i >= _coloredFolderIcons.Length)
                return default;
            return _coloredFolderIcons[i];
        }

        public static Color FolderColor(int colorIndex)
        {
            if (colorIndex <= 0 || colorIndex > ColorCount) return Color.clear;
            int i = colorIndex - 1;
            return IconColors[i % IconColumnCount, i / IconColumnCount];
        }

        [InitializeOnLoadMethod]
        private static void Startup() {
            // Shader.Find can return null for a package shader right after a domain reload; warm up a tick later.
            // Callers also init lazily, so a failure here is retried on first use.
            EditorApplication.delayCall += () => TryInitIcons();
        }

        private static bool TryInitIcons() {
            if (_coloredFolderIcons != null)
                return true;

            var folderTexture = EditorGUIUtility.FindTexture(_folderIconName);
            if (folderTexture == null) {
                Debug.LogError($"[HierarchyFolders] Built-in icon \"{_folderIconName}\" not found.");
                return false;
            }

            Texture2D whiteTexture;
            try {
                whiteTexture = TextureHelper.GetWhiteTexture(folderTexture, $"{_folderIconName} White");
            } catch (System.Exception e) {
                Debug.LogException(e);
                return false;
            }

            var icons = new (Texture2D, Texture2D)[1 + ColorCount];
            icons[0] = (folderTexture, folderTexture);

            for (int index = 1; index < icons.Length; index++) {
                Texture2D tinted = TextureHelper.GetTintedTexture(whiteTexture, FolderColor(index), $"{whiteTexture.name} {index}");
                icons[index] = (tinted, tinted);
            }

            _coloredFolderIcons = icons;
            return true;
        }
    }
}

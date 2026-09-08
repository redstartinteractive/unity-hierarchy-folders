#if UNITY_2019_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityHierarchyFolders.Runtime;

namespace UnityHierarchyFolders.Editor {
    public static class HierarchyFolderIcon {
#if UNITY_2020_1_OR_NEWER
        private const string _openedFolderPrefix = "FolderOpened";
#else
        private const string _openedFolderPrefix = "d_FolderOpened";
#endif
        private const string _closedFolderPrefix = "d_Folder";

        private static Texture2D _openFolderTexture;
        private static Texture2D _closedFolderTexture;
        private static Texture2D _openFolderSelectedTexture;
        private static Texture2D _closedFolderSelectedTexture;

        private static List<EntityId> expandedIDs = new();

        private static object treeViewState;
        private static PropertyInfo expandedIDsProperty;
        private static Type sceneHierarchyWindowType;

        private static (Texture2D open, Texture2D closed)[] _coloredFolderIcons;
        public static (Texture2D open, Texture2D closed) ColoredFolderIcons(int i)
        {
            if (_coloredFolderIcons == null || i < 0 || i >= _coloredFolderIcons.Length)
                return default;
            return _coloredFolderIcons[i];
        }
        public static Color FolderColor(int colorIndex)
        {
            if (colorIndex == 0) return Color.clear;
            int i = colorIndex - 1;
            int row = i / IconColumnCount;
            int col = i % IconColumnCount;
            return IconColors[col, row];
        }

        public static int IconColumnCount => IconColors.GetLength(0);
        public static int IconRowCount => IconColors.GetLength(1);

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

        [InitializeOnLoadMethod]
        private static void Startup() {
            InitIcons();
#if !UNITY_6000_5_OR_NEWER
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.hierarchyWindowItemOnGUI += HandleDrawIcon;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            UpdateExpandedIDs();
#endif
        }

        private static void InitIcons() {
            if(EditorGUIUtility.IconContent($"Folder Icon") != null) {
                _openFolderTexture = (Texture2D)EditorGUIUtility.IconContent($"Folder Icon").image;
            } else {
                _openFolderTexture = (Texture2D)EditorGUIUtility.IconContent($"{_closedFolderPrefix} Icon").image;
            }

            if(EditorGUIUtility.IconContent($"Folder Icon") != null) {
                _closedFolderTexture = (Texture2D)EditorGUIUtility.IconContent($"Folder Icon").image;
            } else {
                _closedFolderTexture = (Texture2D)EditorGUIUtility.IconContent($"{_closedFolderPrefix} Icon").image;
            }

            if(_openFolderTexture == null) {
                Debug.LogError("_openFolderTexture is null.");
                return;
            }

            if(_closedFolderTexture == null) {
                Debug.LogError("_closedFolderTexture is null.");
                return;
            }

            _openFolderSelectedTexture = TextureHelper.GetWhiteTexture(_openFolderTexture, $"{_openedFolderPrefix} Icon White");
            _closedFolderSelectedTexture = TextureHelper.GetWhiteTexture(_closedFolderTexture, $"{_closedFolderPrefix} Icon White");

            _coloredFolderIcons = new (Texture2D, Texture2D)[] { (_openFolderTexture, _closedFolderTexture) };

            for(int row = 0; row < IconRowCount; row++)
            for(int column = 0; column < IconColumnCount; column++) {
                int index = 1 + column + row * IconColumnCount;
                Color color = IconColors[column, row];

                Texture2D openFolderIcon = TextureHelper.GetTintedTexture(_openFolderSelectedTexture,
                    color, $"{_openFolderSelectedTexture.name} {index}");

                Texture2D closedFolderIcon = TextureHelper.GetTintedTexture(_closedFolderSelectedTexture,
                    color, $"{_closedFolderSelectedTexture.name} {index}");

                ArrayUtility.Add(ref _coloredFolderIcons, (openFolderIcon, closedFolderIcon));
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange playModeState) {
            if(playModeState == PlayModeStateChange.ExitingPlayMode) {
                ResubscribeToEvents();
            }
        }

        public static void ResubscribeToEvents() {
#if !UNITY_6000_5_OR_NEWER
            EditorApplication.hierarchyWindowItemOnGUI -= HandleDrawIcon;
            EditorApplication.hierarchyWindowItemOnGUI += HandleDrawIcon;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateExpandedIDs() {
            if(expandedIDsProperty == null) {
                if(sceneHierarchyWindowType == null) {
                    Assembly unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
                    sceneHierarchyWindowType = unityEditorAssembly.GetType("UnityEditor.SceneHierarchyWindow");
                }

                EditorWindow hierarchyWindow = HierarchyWindowUtility.GetHierarchyWindow();
                if(hierarchyWindow == null) {
                    Selection.selectionChanged -= OnSelectionChanged;
                    Selection.selectionChanged += OnSelectionChanged;
                    return;
                }

                PropertyInfo sceneHierarchyProperty = sceneHierarchyWindowType.GetProperty("sceneHierarchy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if(sceneHierarchyProperty == null) {
                    return;
                }

                object sceneHierarchy = sceneHierarchyProperty.GetValue(hierarchyWindow, null);

                PropertyInfo treeViewStateProperty = sceneHierarchy.GetType().GetProperty("treeViewState", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if(treeViewStateProperty == null) {
                    return;
                }

                treeViewState = treeViewStateProperty.GetValue(sceneHierarchy, null);

                expandedIDsProperty = treeViewState.GetType().GetProperty("expandedIDs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if(expandedIDsProperty != null) {
                expandedIDs = (List<EntityId>)expandedIDsProperty.GetValue(treeViewState, null);
            }
        }

        private static void OnHierarchyChanged() {
            UpdateExpandedIDs();
        }

        private static void OnSelectionChanged() {
            Selection.selectionChanged -= OnSelectionChanged;
            UpdateExpandedIDs();
        }
#if !UNITY_6000_5_OR_NEWER
        private static void HandleDrawIcon(int instanceId, Rect itemRect) {
            if(Event.current.type == EventType.Repaint) {
                DrawIcon(instanceId, itemRect);
            }
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DrawIcon(EntityId entityId, Rect itemRect) {
            GameObject gameObject = EditorUtility.EntityIdToObject(entityId) as GameObject;
            bool isFolder = Folder.TryGetIconIndex(gameObject, out int colorIndex);

            if(gameObject != null && isFolder) {
                Rect iconRect = itemRect;
                iconRect.x = itemRect.x - 2;
                iconRect.y = itemRect.y - 2;
                iconRect.width = itemRect.height + 4;
                iconRect.height = itemRect.height + 4;

#if UNITY_2019_1
                iconRect.x += 25f;
#endif

                bool expanded = expandedIDs.IndexOf(entityId) != -1;
                (Texture2D open, Texture2D closed) icons = ColoredFolderIcons(Mathf.Clamp(colorIndex, 0, _coloredFolderIcons.Length - 1));
                Texture2D icon = expanded ? icons.open : icons.closed;
                GUI.DrawTexture(iconRect, icon);
            }
        }
    }
}
#endif

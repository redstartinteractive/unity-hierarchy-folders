using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityHierarchyFolders.Editor {
    public static class HierarchyWindowUtility {
        private static Type sceneHierarchyWindowType;
        private static object treeViewState;
        private static PropertyInfo expandedIDsProperty;
        private static PropertyInfo lastInteractedHierarchyWindowProperty;

        private static EditorWindow hierarchyWindow;
        private static bool setupDone;

        [CanBeNull]
        public static EditorWindow GetHierarchyWindow() {
            if(hierarchyWindow == null) {
                if(!setupDone) {
                    Setup();
                }

                if(lastInteractedHierarchyWindowProperty != null) {
                    object hierarchyWindow = lastInteractedHierarchyWindowProperty.GetValue(null, null);
                    if(hierarchyWindow != null) {
                        return hierarchyWindow as EditorWindow;
                    }
                }

                Object[] hierarchyWindows = Resources.FindObjectsOfTypeAll(sceneHierarchyWindowType);
                if(hierarchyWindows.Length > 0) {
                    return hierarchyWindows[0] as EditorWindow;
                }
            } else if(lastInteractedHierarchyWindowProperty != null) {
                EditorWindow lastInteractedHierarchyWindow = lastInteractedHierarchyWindowProperty.GetValue(null, null) as EditorWindow;
                if(hierarchyWindow != lastInteractedHierarchyWindow && lastInteractedHierarchyWindow != null) {
                    hierarchyWindow = lastInteractedHierarchyWindow;
                }
            }

            return hierarchyWindow;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetExpandedIDs([CanBeNull] ref List<int> expandedIDs) {
            if(!setupDone) {
                Setup();
            }

            if(expandedIDsProperty != null) {
                expandedIDs = (List<int>)expandedIDsProperty.GetValue(treeViewState, null);
            }
        }

        private static void Setup() {
            setupDone = true;

            Assembly unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
            sceneHierarchyWindowType = unityEditorAssembly.GetType("UnityEditor.SceneHierarchyWindow");

            if(lastInteractedHierarchyWindowProperty == null) {
                lastInteractedHierarchyWindowProperty = sceneHierarchyWindowType.GetProperty("lastInteractedHierarchyWindow", BindingFlags.Public | BindingFlags.Static);
            }

            PropertyInfo sceneHierarchyProperty = sceneHierarchyWindowType.GetProperty("sceneHierarchy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if(sceneHierarchyProperty == null) {
                return;
            }

            hierarchyWindow = GetHierarchyWindow();
            if(hierarchyWindow != null) {
                SetupForHierarchyWindow();
            }
        }

        private static void SetupForHierarchyWindow() {
            PropertyInfo sceneHierarchyProperty = hierarchyWindow.GetType().GetProperty("sceneHierarchy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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
    }
}

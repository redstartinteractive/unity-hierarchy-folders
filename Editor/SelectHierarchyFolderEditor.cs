using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using UnityHierarchyFolders.Editor;
using UnityHierarchyFolders.Runtime;

namespace Editor {
    public class SelectHierarchyFolderEditor : EditorWindow {
        private const float K_MIN_WIDTH = 720f;
        private const float K_MAX_WIDTH = 1280f;
        private const float K_MIN_HEIGHT = 405f;
        private const float K_MAX_HEIGHT = 720f;

        private List<Folder> folders;
        private List<GameObject> currentSelection;
        private ReorderableList selectableFolders;
        private Folder selectedFolder;

        [MenuItem("Window/Send To Hierarchy Folder")]
        public static void ShowWindow() {
            SelectHierarchyFolderEditor window = (SelectHierarchyFolderEditor)GetWindowWithRect(typeof(SelectHierarchyFolderEditor), new Rect(0, 0, K_MIN_WIDTH, K_MIN_HEIGHT), false, "Send To folders");
            window.minSize = new Vector2(K_MIN_WIDTH, K_MIN_HEIGHT);
            window.maxSize = new Vector2(K_MAX_WIDTH, K_MAX_HEIGHT);
            window.Show();
        }

        private void OnEnable() {
            folders = GetAllHierarchyFolders();
            currentSelection = Selection.gameObjects.ToList();
            folders.Sort((f1, f2) => f1.name.CompareTo(f2.name));
            selectableFolders = new ReorderableList(folders, typeof(Folder), false, false, false, false) {
                onSelectCallback = (element) => {
                    selectedFolder = folders[element.index];
                    EditorGUIUtility.PingObject(selectedFolder.gameObject);
                },
                drawElementCallback = (rect, index, active, focused) => { EditorGUI.LabelField(rect, EditorGUIUtility.ObjectContent(folders[index], typeof(Transform))); }
            };
        }

        private void OnGUI() {
            selectableFolders.DoLayoutList();
            if(selectedFolder) {
                RenderSendToHierarchyFolderButton();
            }

            GUILayout.Space(10f);
            RenderElementsToMove();
        }

        private void RenderElementsToMove() {
            GUIStyle titleStyle = new() {
                fontSize = 15,
                normal = {
                    textColor = Color.white
                }
            };
            EditorGUILayout.LabelField("Game Objects to move", titleStyle);
            GUILayout.Space(10f);
            for(int i = 0; i < currentSelection.Count; i++) {
                EditorGUILayout.LabelField(EditorGUIUtility.ObjectContent(currentSelection[i], typeof(Transform)));
            }
        }

        private List<Folder> GetAllHierarchyFolders() {
            return FindObjectsOfType<Folder>().ToList();
        }

        private void RenderSendToHierarchyFolderButton() {
            if(!GUILayout.Button("Send To Hierarchy Folder")) {
                return;
            }

            GameObject folderGameObject = selectedFolder.gameObject;

            foreach(GameObject gameObject in currentSelection) {
                Undo.SetTransformParent(gameObject.transform, folderGameObject.transform, "Send Selection To Hierarchy Folder Window");
            }

            Close();
        }
    }
}

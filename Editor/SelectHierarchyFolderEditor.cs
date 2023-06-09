using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityHierarchyFolders.Runtime;

namespace UnityHierarchyFolders.Editor {
    public class SelectHierarchyFolderEditor : EditorWindow {
        private const float K_MIN_WIDTH = 320f;
        private const float K_MIN_HEIGHT = 200f;

        private List<Folder> folders;
        private List<GameObject> currentSelection;
        private ReorderableList selectableFolders;
        private Folder selectedFolder;
        private GUIStyle titleStyle;
        private Vector2 scrollPosFolders = Vector2.zero;
        private Vector2 scrollPosObjects = Vector2.zero;
        private Color defaultColor;

        public static void ShowWindow() {
            SelectHierarchyFolderEditor window = (SelectHierarchyFolderEditor)GetWindow(typeof(SelectHierarchyFolderEditor), false, "Send To Folder");
            window.minSize = new Vector2(K_MIN_WIDTH, K_MIN_HEIGHT);
            window.Show();
        }

        private void OnEnable() {
            defaultColor = GUI.backgroundColor;
            titleStyle = new GUIStyle {
                fontSize = 14,
                normal = {
                    textColor = Color.white
                }
            };
            folders = GetAllHierarchyFolders();
            currentSelection = Selection.gameObjects.ToList();
            folders.Sort((f1, f2) => f1.name.CompareTo(f2.name));
            selectableFolders = new ReorderableList(folders, typeof(Folder), false, false, false, false) {
                onSelectCallback = (element) => {
                    selectedFolder = folders[element.index];
                    EditorGUIUtility.PingObject(selectedFolder.gameObject);
                    Event e = Event.current;
                    if(e.clickCount >= 2) SendToFolder();
                },

                drawElementCallback = (rect, index, active, focused) => { EditorGUI.LabelField(rect, EditorGUIUtility.ObjectContent(folders[index], typeof(Transform))); }
            };
        }

        private void OnGUI() {
            RenderSelectButton();

            EditorGUILayout.BeginHorizontal();
            RenderElementsToMove();
            RenderFolders();
            EditorGUILayout.EndHorizontal();
        }

        private void RenderSelectButton() {
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color32(110, 200, 255, 255);

            if(!selectedFolder) GUI.enabled = false;

            if(GUILayout.Button("Send To Folder", GUILayout.Height(30))) SendToFolder();

            GUI.enabled = true;
            GUI.backgroundColor = defaultColor;
            EditorGUILayout.EndHorizontal();
        }

        private void RenderElementsToMove() {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("GameObjects to Move", titleStyle);
            GUILayout.Space(10f);

            scrollPosObjects = EditorGUILayout.BeginScrollView(scrollPosObjects);
            for(int i = 0; i < currentSelection.Count; i++) EditorGUILayout.LabelField(EditorGUIUtility.ObjectContent(currentSelection[i], typeof(Transform)));

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void RenderFolders() {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Select a Folder", titleStyle, GUILayout.Height(30), GUILayout.Width(100));
            scrollPosFolders = EditorGUILayout.BeginScrollView(scrollPosFolders);
            selectableFolders.DoLayoutList();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private List<Folder> GetAllHierarchyFolders() {
            return FindObjectsOfType<Folder>().ToList();
        }

        private void SendToFolder() {
            foreach(GameObject gameObject in currentSelection) Undo.SetTransformParent(gameObject.transform, selectedFolder.transform, "Send To Folder");

            Close();
        }
    }
}

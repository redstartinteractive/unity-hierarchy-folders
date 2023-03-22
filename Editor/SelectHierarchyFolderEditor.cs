using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityHierarchyFolders.Runtime;

namespace UnityHierarchyFolders.Editor
{
    public class SelectHierarchyFolderEditor : EditorWindow
    {
        private const float K_MIN_WIDTH = 320f;
        private const float K_MIN_HEIGHT = 200f;

        private List<Folder> folders;
        private List<GameObject> currentSelection;
        private ReorderableList selectableFolders;
        private Folder selectedFolder;
        private GUIStyle titleStyle;
        Vector2 scrollPos = Vector2.zero;

        [MenuItem("Window/Send To Hierarchy Folder")]
        public static void ShowWindow() 
        {
            SelectHierarchyFolderEditor window = (SelectHierarchyFolderEditor)GetWindow(typeof(SelectHierarchyFolderEditor), false, "Send To Folder");
            window.minSize = new Vector2(K_MIN_WIDTH, K_MIN_HEIGHT);
            window.Show();
        }

        private void OnEnable() 
        {
            titleStyle = new() {
                fontSize = 14,
                normal = {
                    textColor = Color.white
                }
            };
            folders = GetAllHierarchyFolders();
            currentSelection = Selection.gameObjects.ToList();
            folders.Sort((f1, f2) => f1.name.CompareTo(f2.name));
            selectableFolders = new ReorderableList(folders, typeof(Folder), false, false, false, false) 
            {
                onSelectCallback = (element) => 
                {
                    selectedFolder = folders[element.index];
                    EditorGUIUtility.PingObject(selectedFolder.gameObject);
                },
                
                drawElementCallback = (rect, index, active, focused) => 
                {
                    EditorGUI.LabelField(rect, EditorGUIUtility.ObjectContent(folders[index], typeof(Transform)));
                }
            };
        }

        private void OnGUI() 
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            RenderHeading();
            selectableFolders.DoLayoutList();
            RenderElementsToMove();
            EditorGUILayout.EndScrollView();
        }

        private void RenderHeading() 
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select a Folder", titleStyle, GUILayout.Height(30), GUILayout.Width(100));
            GUILayout.Space(10f);
            if(selectedFolder) 
            {
                if(GUILayout.Button("Send To Folder", GUILayout.Height(30)))
                {
                    GameObject folderGameObject = selectedFolder.gameObject;
                    foreach(GameObject gameObject in currentSelection) 
                    {
                        Undo.SetTransformParent(gameObject.transform, folderGameObject.transform, "Send To Folder");
                    }

                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RenderElementsToMove() 
        {
            EditorGUILayout.LabelField("GameObjects to Move", titleStyle);
            GUILayout.Space(10f);
            for(int i = 0; i < currentSelection.Count; i++) 
            {
                EditorGUILayout.LabelField(EditorGUIUtility.ObjectContent(currentSelection[i], typeof(Transform)));
            }
        }
        
        private List<Folder> GetAllHierarchyFolders() 
        {
            return FindObjectsOfType<Folder>().ToList();
        }
    }
}

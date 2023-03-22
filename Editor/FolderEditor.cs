﻿using UnityEditor;
using UnityEngine;
using UnityHierarchyFolders.Runtime;

namespace UnityHierarchyFolders.Editor
{
    [CustomEditor(typeof(Folder))]
    [CanEditMultipleObjects]
    public class FolderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            this.RenderColorPicker();
            if(Selection.count <= 1)
            {
                this.RenderSelectChildrenButton();
            }
        }

        private void RenderColorPicker()
        {
            var colorIndexProperty = this.serializedObject.FindProperty("_colorIndex");

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            float buttonSize = 28f;

            var gridRect = EditorGUILayout.GetControlRect(false, buttonSize * HierarchyFolderIcon.IconRowCount,
                GUILayout.Width(buttonSize * HierarchyFolderIcon.IconColumnCount));

            int currentIndex = colorIndexProperty.intValue;
            for(int row = 0; row < HierarchyFolderIcon.IconRowCount; row++)
            {
                for(int column = 0; column < HierarchyFolderIcon.IconColumnCount; column++)
                {
                    int index = 1 + column + row * HierarchyFolderIcon.IconColumnCount;
                    float width = gridRect.width / HierarchyFolderIcon.IconColumnCount;
                    float height = gridRect.height / HierarchyFolderIcon.IconRowCount;
                    var rect = new Rect(gridRect.x + width * column, gridRect.y + height * row, width, height);
                    (var openIcon, var closeIcon) = HierarchyFolderIcon.ColoredFolderIcons(index);

                    if(Event.current.type == EventType.Repaint)
                    {
                        if(index == currentIndex)
                        {
                            GUIStyle hover = "TV Selection";
                            hover.Draw(rect, false, false, false, false);
                        } else if(rect.Contains(Event.current.mousePosition))
                        {
                            GUI.backgroundColor = new Color(.7f, .7f, .7f, 1f);
                            GUIStyle white = "WhiteBackground";
                            white.Draw(rect, false, false, true, false);
                            GUI.backgroundColor = Color.white;
                        }
                    }

                    if(GUI.Button(rect, currentIndex == index ? openIcon : closeIcon, EditorStyles.label))
                    {
                        Undo.RecordObject(this.target, "Set Folder Color");
                        colorIndexProperty.intValue = currentIndex == index ? 0 : index;
                        this.serializedObject.ApplyModifiedProperties();
                        EditorApplication.RepaintHierarchyWindow();
                        GUIUtility.ExitGUI();
                    }
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10f);
        }

        private void RenderSelectChildrenButton()
        {
            if(GUILayout.Button("Select Child Objects"))
            {
                Folder script = (Folder)target;
                GameObject[] children = new GameObject[script.transform.childCount];
                for(int i = 0; i < script.transform.childCount; i++)
                {
                    children[i] = script.transform.GetChild(i).gameObject;
                }

                Selection.objects = children;
            }
        }
    }
}

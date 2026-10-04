using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityHierarchyFolders.Runtime;

namespace UnityHierarchyFolders.Editor
{
    [CustomEditor(typeof(Folder))]
    [CanEditMultipleObjects]
    public class FolderEditor : UnityEditor.Editor
    {
        private const string SelectedSwatchClass = "hf-color-swatch--selected";

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            FolderStyles.Apply(root);

            root.Add(CreateColorGrid(root));
            root.Add(new Button(SelectChildObjects) { text = "Select Child Objects" });

            return root;
        }

        private VisualElement CreateColorGrid(VisualElement root)
        {
            SerializedProperty colorIndex = serializedObject.FindProperty("_colorIndex");
            var swatches = new List<(int index, Button button)>();

            var grid = new VisualElement();
            grid.AddToClassList("hf-color-grid");

            for (int row = 0; row < HierarchyFolderIcon.IconRowCount; row++)
            {
                var rowElement = new VisualElement();
                rowElement.AddToClassList("hf-color-grid__row");
                grid.Add(rowElement);

                for (int column = 0; column < HierarchyFolderIcon.IconColumnCount; column++)
                {
                    int index = 1 + column + row * HierarchyFolderIcon.IconColumnCount;
                    Button swatch = CreateSwatch(index, () => ToggleColor(colorIndex, index));
                    swatches.Add((index, swatch));
                    rowElement.Add(swatch);
                }
            }

            void RefreshSelection(SerializedProperty property)
            {
                int current = property.hasMultipleDifferentValues ? -1 : property.intValue;
                foreach ((int index, Button button) in swatches)
                {
                    button.EnableInClassList(SelectedSwatchClass, index == current);
                }
            }

            // Also catches undo/redo and edits from other inspectors.
            root.TrackPropertyValue(colorIndex, RefreshSelection);
            RefreshSelection(colorIndex);

            return grid;
        }

        private static Button CreateSwatch(int index, System.Action onClick)
        {
            var swatch = new Button(onClick);
            swatch.AddToClassList("hf-color-swatch");

            var icon = new Image { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("hf-color-swatch__icon");
            Texture2D texture = HierarchyFolderIcon.ColoredFolderIcon(index);
            if (texture != null)
                icon.image = texture;
            else
                icon.style.backgroundColor = HierarchyFolderIcon.FolderColor(index);

            swatch.Add(icon);
            return swatch;
        }

        /// <summary>Clicking the current color clears it; anything else sets it.</summary>
        private void ToggleColor(SerializedProperty colorIndex, int index)
        {
            serializedObject.Update();
            bool isCurrent = !colorIndex.hasMultipleDifferentValues && colorIndex.intValue == index;
            colorIndex.intValue = isCurrent ? 0 : index;
            serializedObject.ApplyModifiedProperties();
        }

        private void SelectChildObjects()
        {
            List<GameObject> allObjects = new();

            foreach (var target in targets)
            {
                SelectChildrenRecursive(((Folder) target).transform, allObjects);
            }

            Selection.objects = allObjects.ToArray();
        }

        private static void SelectChildrenRecursive(Transform parent, List<GameObject> allObjects)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);

                // If the child is a Folder, recurse into it but do not add it to the selection
                if (child.TryGetComponent<Folder>(out _))
                {
                    SelectChildrenRecursive(child, allObjects);
                }
                else
                {
                    allObjects.Add(child.gameObject);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityHierarchyFolders.Runtime;

namespace UnityHierarchyFolders.Editor {
    /// <summary>Picks a folder to move the current selection into.</summary>
    public class SelectHierarchyFolderEditor : EditorWindow {
        private const float K_MIN_WIDTH = 320f;
        private const float K_MIN_HEIGHT = 200f;
        private const string UndoName = "Send To Folder";

        private readonly List<GameObject> objectsToMove = new();
        private readonly List<(Folder folder, string path)> folders = new();

        private ToolbarSearchField searchField;
        private ListView objectList;
        private ListView folderList;
        private Button sendButton;

        public static void ShowWindow() {
            var window = GetWindow<SelectHierarchyFolderEditor>(false, "Send To Folder");
            window.minSize = new Vector2(K_MIN_WIDTH, K_MIN_HEIGHT);
            window.Show();
        }

        private void OnEnable() {
            Selection.selectionChanged += Refresh;
            EditorApplication.hierarchyChanged += Refresh;
        }

        private void OnDisable() {
            Selection.selectionChanged -= Refresh;
            EditorApplication.hierarchyChanged -= Refresh;
        }

        private void CreateGUI() {
            VisualElement root = rootVisualElement;
            FolderStyles.Apply(root);

            var toolbar = new Toolbar();
            searchField = new ToolbarSearchField { style = { flexGrow = 1, width = StyleKeyword.Auto } };
            searchField.RegisterValueChangedCallback(_ => Refresh());
            toolbar.Add(searchField);
            root.Add(toolbar);

            var columns = new VisualElement();
            columns.AddToClassList("hf-send-window__columns");
            root.Add(columns);

            objectList = new ListView(objectsToMove, makeItem: FolderStyles.CreateIconRow, bindItem: BindObjectRow) {
                selectionType = SelectionType.None,
            };
            columns.Add(CreateColumn("GameObjects to Move", objectList));

            folderList = new ListView(folders, makeItem: FolderStyles.CreateIconRow, bindItem: BindFolderRow) {
                selectionType = SelectionType.Single,
            };
            folderList.selectionChanged += OnFolderSelectionChanged;
            folderList.itemsChosen += _ => SendToFolder();
            columns.Add(CreateColumn("Select a Folder", folderList));

            sendButton = new Button(SendToFolder) { text = "Send To Folder" };
            sendButton.AddToClassList("hf-send-window__send");
            root.Add(sendButton);

            Refresh();
            searchField.Focus();
        }

        private static VisualElement CreateColumn(string title, ListView list) {
            var column = new VisualElement();
            column.AddToClassList("hf-send-window__column");

            var header = new Label(title);
            header.AddToClassList("hf-send-window__header");
            column.Add(header);

            list.fixedItemHeight = 20;
            list.AddToClassList("hf-send-window__list");
            column.Add(list);
            return column;
        }

        private void BindObjectRow(VisualElement row, int index) {
            GameObject go = objectsToMove[index];
            row.Q<Image>("icon").image = EditorGUIUtility.ObjectContent(go, typeof(GameObject)).image;
            row.Q<Label>("label").text = go != null ? go.name : "(missing)";
        }

        private void BindFolderRow(VisualElement row, int index) {
            (Folder folder, string path) = folders[index];
            row.Q<Image>("icon").image = HierarchyFolderIcon.ColoredFolderIcons(folder != null ? folder.ColorIndex : 0).closed;
            row.Q<Label>("label").text = path;
        }

        /// <summary>Rebuilds both lists from the live selection and scene, keeping the chosen folder if still valid.</summary>
        private void Refresh() {
            if (folderList == null)
                return;

            Folder previouslyChosen = SelectedFolder;

            objectsToMove.Clear();
            objectsToMove.AddRange(Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable | SelectionMode.ExcludePrefab)
                .Select(t => t.gameObject));

            string filter = searchField.value;
            folders.Clear();
            folders.AddRange(FindObjectsByType<Folder>(FindObjectsInactive.Include)
                .Where(CanReceiveSelection)
                .Select(f => (f, GetPath(f.transform)))
                .Where(entry => string.IsNullOrEmpty(filter) || entry.Item2.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(entry => entry.Item2, StringComparer.OrdinalIgnoreCase));

            objectList.RefreshItems();
            folderList.RefreshItems();

            int chosenIndex = folders.FindIndex(entry => entry.folder == previouslyChosen);
            if (chosenIndex >= 0)
                folderList.SetSelectionWithoutNotify(new[] { chosenIndex });
            else
                folderList.ClearSelection();

            UpdateSendButton();
        }

        // A folder can't receive itself or anything it's inside of.
        private bool CanReceiveSelection(Folder folder) =>
            !objectsToMove.Any(go => folder.transform.IsChildOf(go.transform));

        private static string GetPath(Transform transform) {
            string path = transform.name;
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
                path = parent.name + "/" + path;
            return path;
        }

        private Folder SelectedFolder =>
            folderList != null && folderList.selectedIndex >= 0 && folderList.selectedIndex < folders.Count
                ? folders[folderList.selectedIndex].folder
                : null;

        private void OnFolderSelectionChanged(IEnumerable<object> _) {
            if (SelectedFolder != null)
                EditorGUIUtility.PingObject(SelectedFolder.gameObject);
            UpdateSendButton();
        }

        private void UpdateSendButton() => sendButton.SetEnabled(SelectedFolder != null && objectsToMove.Count > 0);

        private void SendToFolder() {
            Folder folder = SelectedFolder;
            if (folder == null || objectsToMove.Count == 0)
                return;

            foreach (GameObject go in objectsToMove)
                Undo.SetTransformParent(go.transform, folder.transform, UndoName);

            Close();
        }
    }
}

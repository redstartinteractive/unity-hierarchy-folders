using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityHierarchyFolders.Runtime;

namespace UnityHierarchyFolders.Editor 
{
    public static class FolderEditorUtils 
    {
        internal const string NewFolderLabel = "New Folder";
        internal const string NewFolderWithSelectionLabel = "New Folder With Selection";
        internal const string SendToFolderLabel = "Send To Folder";
        internal const string SendToFolderMenuName = SendToFolderLabel + " %#&M";

        [MenuItem("GameObject/" + NewFolderLabel, isValidateFunction: false, priority: 0)]
        public static void CreateFolderMenuItem(MenuCommand command)
        {
            if (IsRepeatedForMultiSelection(command))
                return;

            CreateFolder(includeSelection: false);
        }

        [MenuItem("GameObject/" + NewFolderWithSelectionLabel, isValidateFunction: false, priority: 0)]
        public static void NewFolderWithSelectionMenuItem(MenuCommand command)
        {
            if (IsRepeatedForMultiSelection(command))
                return;

            CreateFolder(includeSelection: true);
        }

        [MenuItem("GameObject/" + NewFolderWithSelectionLabel, isValidateFunction: true, priority: 0)]
        public static bool NewFolderWithSelectionValidate() => HasSceneSelection();

        /// <summary>Folders the current selection if there is one, otherwise just creates a folder.</summary>
        [Shortcut("Hierarchy Folders/New Folder", KeyCode.G, ShortcutModifiers.Action)]
        private static void NewFolderShortcut() => CreateFolder(includeSelection: HasSceneSelection());

        internal static bool HasSceneSelection() => SelectedSceneTransforms().Length > 0;

        /// <summary>
        /// Creates a folder next to the active selection (or at the scene root if nothing is selected), optionally
        /// moving the selected objects into it.
        /// </summary>
        public static GameObject CreateFolder(bool includeSelection)
        {
            string undoName = includeSelection ? NewFolderWithSelectionLabel : NewFolderLabel;
            Transform[] selection = SelectedSceneTransforms();
            Transform anchor = selection.Length == 0 ? null
                : Selection.activeTransform != null ? Selection.activeTransform : selection[0];

            var folder = new GameObject("Folder", typeof(Folder));
            Undo.RegisterCreatedObjectUndo(folder, undoName);

            if (anchor != null)
            {
                if (anchor.parent != null)
                    Undo.SetTransformParent(folder.transform, anchor.parent, undoName);
                else
                    SceneManager.MoveGameObjectToScene(folder, anchor.gameObject.scene);

                folder.transform.SetSiblingIndex(anchor.GetSiblingIndex() + 1);
            }

            if (includeSelection)
            {
                foreach (Transform t in selection)
                {
                    Undo.SetTransformParent(t, folder.transform, undoName);
                }
            }

            return folder;
        }

        // Editable top-level scene objects only, so a child isn't pulled out of a selected parent.
        private static Transform[] SelectedSceneTransforms() =>
            Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable | SelectionMode.ExcludePrefab);

        // A GameObject/ menu item runs once per selected object; only act on the first.
        private static bool IsRepeatedForMultiSelection(MenuCommand command) =>
            Selection.objects.Length > 1 && command.context && command.context != Selection.objects[0];

        /// <summary>Add new folder "prefab".</summary>
        /// <param name="command">Menu command information.</param>
        [MenuItem("GameObject/" + SendToFolderMenuName, isValidateFunction: false, priority: 0)]
        public static void SendToFolder(MenuCommand command)
        {
            if(Selection.objects.Length > 1) 
            {
                if(command.context != Selection.objects[0]) 
                {
                    return;
                }
            }

            SelectHierarchyFolderEditor.ShowWindow();
        }

        [MenuItem("GameObject/" + SendToFolderMenuName, isValidateFunction: true, priority: 0)]
        public static bool SendToFolderValidate(MenuCommand command) => CanSendToFolder();

        internal static bool CanSendToFolder()
        {
            return Selection.objects.Length > 0 && Object.FindObjectsByType<Folder>(FindObjectsInactive.Include).Length > 0;
        }
    }

    public class FolderOnBuild : IProcessSceneWithReport 
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report) 
        {
            var strippingMode = report == null ? StripSettings.PlayMode : StripSettings.Build;
            foreach(var folder in Object.FindObjectsByType<Folder>(FindObjectsInactive.Include))
            {
                folder.Flatten(strippingMode, StripSettings.CapitalizeName);
            }
        }
    }
}

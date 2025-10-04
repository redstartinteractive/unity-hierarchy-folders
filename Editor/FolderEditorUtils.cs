using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityHierarchyFolders.Runtime;

namespace UnityHierarchyFolders.Editor 
{
    public static class FolderEditorUtils 
    {
        private const string _actionNewFolder = "Create Folder %G";
        private const string _actionSelectionFolder = "Move To New Folder";
        private const string _actionSendToFolderWindow = "Send To Folder %#&M";

        /// <summary>Add new folder "prefab". If objects are selected they will be placed inside the new folder.</summary>
        [MenuItem("GameObject/" + _actionNewFolder, isValidateFunction: false, priority: 0)]
        public static void AddFolderPrefab(MenuCommand command) 
        {
            // Prevent running this command on each selected object individually
            if (Selection.objects.Length > 1)
            {
                if (command.context && command.context != Selection.objects[0])
                {
                    return;
                }
            }

            var obj = new GameObject { name = "Folder" };
            obj.AddComponent<Folder>();
            Undo.RegisterCreatedObjectUndo(obj, _actionNewFolder);

            if(Selection.activeGameObject)
            {
                obj.transform.SetSiblingIndex(Selection.activeGameObject.transform.GetSiblingIndex() + 1);
            }

            foreach(GameObject go in Selection.gameObjects)
            {
                Undo.SetTransformParent(go.transform, obj.transform, _actionSelectionFolder);
            }
        }

        /// <summary>Add new folder "prefab".</summary>
        /// <param name="command">Menu command information.</param>
        [MenuItem("GameObject/" + _actionSendToFolderWindow, isValidateFunction: false, priority: 0)]
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

        [MenuItem("GameObject/" + _actionSendToFolderWindow, isValidateFunction: true, priority: 0)]
        public static bool SendToFolderValidate(MenuCommand command) 
        {
            return Selection.objects.Length > 0 && Object.FindObjectsByType<Folder>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0;
        }
    }

    public class FolderOnBuild : IProcessSceneWithReport 
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report) 
        {
            var strippingMode = report == null ? StripSettings.PlayMode : StripSettings.Build;

            foreach(var folder in Object.FindObjectsByType<Folder>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                folder.Flatten(strippingMode, StripSettings.CapitalizeName);
            }
        }
    }
}

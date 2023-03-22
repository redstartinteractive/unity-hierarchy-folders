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
        private const string _actionNewFolder = "Create Folder %#&N";
        private const string _actionSelectionFolder = "Create Folder With Selection %#&N";
        private const string _actionSendToFolderWindow = "Send To Folder %#&M";

        /// <summary>Add new folder "prefab".</summary>
        /// <param name="command">Menu command information.</param>
        [MenuItem("GameObject/" + _actionNewFolder, isValidateFunction: false, priority: 0)]
        public static void AddFolderPrefab(MenuCommand command) 
        {
            var obj = new GameObject { name = "Folder" };
            obj.AddComponent<Folder>();

            GameObjectUtility.SetParentAndAlign(obj, (GameObject)command.context);
            Undo.RegisterCreatedObjectUndo(obj, _actionNewFolder);
        }

        [MenuItem("GameObject/" + _actionNewFolder, isValidateFunction: true, priority: 0)]
        public static bool AddFolderPrefabValidate(MenuCommand command) 
        {
            return Selection.objects.Length <= 0;
        }

        /// <summary>Add new folder "prefab" and place selected objects inside it as children.</summary>
        /// <param name="command">Menu command information.</param>
        [MenuItem("GameObject/" + _actionSelectionFolder, isValidateFunction: false, priority: 0)]
        public static void AddFolderWithSelection(MenuCommand command) 
        {
            if(Selection.objects.Length > 1) 
            {
                if(command.context != Selection.objects[0]) 
                {
                    return;
                }
            }

            var obj = new GameObject { name = "Folder" };
            obj.AddComponent<Folder>();

            GameObject parentGo = (GameObject)command.context;
            if(parentGo.transform.parent) 
            {
                GameObjectUtility.SetParentAndAlign(obj, parentGo.transform.parent.gameObject);
            }

            Undo.RegisterCreatedObjectUndo(obj, _actionSelectionFolder);

            foreach(GameObject go in Selection.gameObjects) 
            {
                Undo.SetTransformParent(go.transform, obj.transform, _actionSelectionFolder);
            }
        }

        [MenuItem("GameObject/" + _actionSelectionFolder, isValidateFunction: true, priority: 0)]
        public static bool AddFolderWithSelectionValidate(MenuCommand command) 
        {
            return Selection.objects.Length > 0;
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
            return Selection.objects.Length > 0 && Object.FindObjectsOfType<Folder>().Length > 0;
        }
    }

    public class FolderOnBuild : IProcessSceneWithReport 
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report) 
        {
            var strippingMode = report == null ? StripSettings.PlayMode : StripSettings.Build;

            foreach(var folder in Object.FindObjectsOfType<Folder>()) 
            {
                folder.Flatten(strippingMode, StripSettings.CapitalizeName);
            }
        }
    }
}


using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace UniNPermissions
{
    public class UniNPermissionsWindow : EditorWindow
    {
        [MenuItem ("Edit/Project Settings/UniNPermissions")]
        public static void ShowWindow()
        {
            GetWindow(typeof(UniNPermissionsWindow));
        }

        #region GUI

        private int selectedTab = 0;

        private void OnGUI()
        {
            var settingsFileExist = File.Exists(settingsAssetPath);
            if (!settingsFileExist)
                UniNPermissionsSettingsNotFoundGUI();
            else
                UniNPermissionsSettingsExistGUI();
        }

        private void UniNPermissionsSettingsNotFoundGUI()
        {
            if (GUILayout.Button("Create UniNPermissions settings"))
                CreateSettingsAsset();

            EditorGUILayout.HelpBox("To use UniNPermissions, you need a settings file.\n"+settingsAssetPath, MessageType.Info);
        }

        private void UniNPermissionsSettingsExistGUI()
        {
            var settingsAsset = LoadSettingsAsset();
            if (GUILayout.Button("Locate UniNPermissions settings"))
                Selection.activeObject = settingsAsset;

            EditorGUILayout.HelpBox("Settings file:\n" + settingsAssetPath, MessageType.Info);

            this.selectedTab = GUILayout.Toolbar(this.selectedTab, new string[] {"Android", "iOS"});
            switch (this.selectedTab)
            {
                case 0:
                {
                    AndroidGUI();
                    break;
                }
                case 1:
                {
                    IOSGUI();
                    break;
                }
            }
        }

        private void AndroidGUI()
        {
            var androidManifestFileExist = File.Exists(androidManifestFile);
            if (!androidManifestFileExist)
            {
                if (GUILayout.Button("Create AndroidManifest.xml"))
                    CreateAndroidManifestFile();
            }
        }

        private void IOSGUI()
        {
            GUILayout.Label("Permission usage descriptions:");
            GUILayout.Label("From player settings");
            PlayerSettings.iOS.cameraUsageDescription = EditorGUILayout.TextField("For Camera:", PlayerSettings.iOS.cameraUsageDescription);
            PlayerSettings.iOS.locationUsageDescription = EditorGUILayout.TextField("For Location:", PlayerSettings.iOS.locationUsageDescription);
            PlayerSettings.iOS.microphoneUsageDescription = EditorGUILayout.TextField("For Microphone:", PlayerSettings.iOS.microphoneUsageDescription);
            GUILayout.Label("From UniNPermissions settings");
        }

        #endregion

        #region Handling settings file

        private const string settingsAssetPath = "Assets/UniNPermission/Editor/Settings.asset";

        private UniNPermissionsSettings LoadSettingsAsset()
        {
            return AssetDatabase.LoadAssetAtPath<UniNPermissionsSettings>(settingsAssetPath);
        }

        private void CreateSettingsAsset()
        {
            var settingsAsset = CreateInstance<UniNPermissionsSettings>();
            AssetDatabase.CreateAsset(settingsAsset, settingsAssetPath);
            AssetDatabase.SaveAssets();
        }

        #endregion

        #region Handling AndroidManifest.xml file

        private const string androidManifestFile = "Assets/Plugins/Android/AndroidManifest2.xml";

        private UniNPermissionsSettings LoadAndroidManifestFile()
        {
            return AssetDatabase.LoadAssetAtPath<UniNPermissionsSettings>(settingsAssetPath);
        }

        private void CreateAndroidManifestFile()
        {
            /*var manifest = new Manifest();
            manifest.permission = new Permission();
            manifest.permission.PermissionName = "android.permission.SEND_SMS";

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("android", "http://schemas.android.com/apk/res/android");

            var serializer = new XmlSerializer(typeof(Manifest));
            var stream = new FileStream(androidManifestFile, FileMode.CreateNew);
            serializer.Serialize(stream, manifest, namespaces);
            stream.Close();*/
        }

        #endregion


        /*[XmlRoot("manifest", Namespace = "http://schemas.android.com/apk/res/android")]
        public class Manifest
        {
            [XmlElement("uses-permission")] public Permission permission;
        }

        public class Permission
        {
            [XmlAttribute("name", Namespace ="http://schemas.android.com/apk/res/android")] public string PermissionName;
        }*/
    }
}

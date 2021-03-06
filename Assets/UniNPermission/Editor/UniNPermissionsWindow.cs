﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
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
                //if (GUILayout.Button("Create AndroidManifest.xml"))
                //    CreateTestAndroidManifestFile();
            }
            else
            {
                var manifest = ReadAndroidManifestFile();
                var saveChanges = false;
                var permissionIndexesToDelete = new List<int>();

                if (manifest.permissions == null)
                    manifest.permissions = new Permission[] {};

                var skipPermission = Array.Find<Metadata>(manifest.application.metadata, metadata => metadata.Name == "unityplayer.SkipPermissionsDialog");
                var skipPermissionWasTrue = skipPermission != null && skipPermission.Value == "true";
                var skipPermissionIsTrue = GUILayout.Toggle(skipPermissionWasTrue, "Request at startup:");
                skipPermission.Value = skipPermissionIsTrue ? "true" : "false";
                if (skipPermissionWasTrue != skipPermissionIsTrue)
                    saveChanges = true;

                for (var i = 0; i < manifest.permissions.Length; i++)
                {
                    var shouldDelete = false;
                    AndroidPermissionGUI(manifest.permissions[i], ref saveChanges, out shouldDelete);
                    if (shouldDelete)
                        permissionIndexesToDelete.Add(i);
                }

                var newPermissions = new List<Permission>(manifest.permissions);

                permissionIndexesToDelete.Sort();
                for (int i = permissionIndexesToDelete.Count - 1; i >= 0; i--)
                {
                    newPermissions.RemoveAt(permissionIndexesToDelete[i]);
                    saveChanges = true;
                }

                if (GUILayout.Button("+"))
                {
                    newPermissions.Add(Permission.PermissionForPermissionName("android.permission.DEFINE_THIS"));
                    saveChanges = true;
                }

                if (saveChanges)
                {
                    manifest.permissions = newPermissions.ToArray();
                    SaveAndroidManifestFile(manifest);
                }
            }
        }

        private void AndroidPermissionGUI(Permission permission, ref bool saveChanges, out bool shouldDelete)
        {
            GUILayout.BeginHorizontal();

            shouldDelete = false;

            var oldPermissionName = permission.name;
            var newPermissionName = GUILayout.TextField(oldPermissionName);
            permission.name = newPermissionName;

            if (oldPermissionName != newPermissionName)
                saveChanges = true;

            if (GUILayout.Button("X", GUILayout.Width(30)))
                shouldDelete = true;

            GUILayout.EndHorizontal();
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

        private const string androidManifestFile = "Assets/Plugins/Android/AndroidManifest.xml";

        private Manifest ReadAndroidManifestFile()
        {
            var serializer = new XmlSerializer(typeof(Manifest));
            var stream = new FileStream(androidManifestFile, FileMode.Open);
            var manifest = serializer.Deserialize(stream) as Manifest;
            stream.Close();

            return manifest;
        }

        private void SaveAndroidManifestFile(Manifest manifest)
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("android", "http://schemas.android.com/apk/res/android");

            var serializer = new XmlSerializer(typeof(Manifest));
            var stream = new StreamWriter(androidManifestFile, false, Encoding.UTF8);
            serializer.Serialize(stream, manifest, namespaces);
            stream.Close();
        }

        #endregion

        [XmlRoot("manifest")]
        public class Manifest
        {
            [XmlAnyAttribute]
            public XmlAttribute[] allAttributes;

            [XmlAnyElement]
            public XmlElement[] allElements;

            [XmlElement("uses-permission")]
            public Permission[] permissions;

            [XmlElement("application")]
            public Application application;
        }

        public class Application
        {
            [XmlAnyAttribute]
            public XmlAttribute[] allAttributes;

            [XmlAnyElement]
            public XmlElement[] allElements;

            [XmlElement("meta-data")]
            public Metadata[] metadata;
        }

        public class Permission
        {
            public static Permission PermissionForPermissionName(string permissionName)
            {
                return new Permission() {name = permissionName};
            }

            [XmlAttribute("name", Namespace = "http://schemas.android.com/apk/res/android")]
            public string name;
        }

        public class Metadata
        {
            [XmlAttribute("name", Namespace = "http://schemas.android.com/apk/res/android")]
            public string Name;

            [XmlAttribute("value", Namespace = "http://schemas.android.com/apk/res/android")]
            public string Value;
        }

    }
}

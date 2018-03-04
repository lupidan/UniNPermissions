using System;
using UnityEngine;

namespace UniNPermissions
{
    public class AndroidPermissions : IPermissions
    {
        private class OnRequestPermissionsResultCallback : AndroidJavaProxy
        {
            private const string OnRequestPermissionsResultCallbackClassPath = "android.support.v4.app.ActivityCompat.OnRequestPermissionsResultCallback";
            private readonly Action<int, string[], int[]> _onPermissionResultCallback;

            public OnRequestPermissionsResultCallback(Action<int, string[], int[]> onPermissionResultCallback) : base(OnRequestPermissionsResultCallbackClassPath)
            {
                this._onPermissionResultCallback = onPermissionResultCallback;
            }

            void onRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
            {
                this._onPermissionResultCallback(requestCode, permissions, grantResults);
            }
        }

        private const string ACTION_APPLICATION_DETAILS_SETTINGS = "android.settings.APPLICATION_DETAILS_SETTINGS";
        private const int PERMISSION_GRANTED = 0;
        private const int PERMISSION_DENIED = -1;

        private readonly string unityPlayerClassPath = null;
        private readonly OnRequestPermissionsResultCallback _callback;
        private readonly AndroidJavaClass _unityPlayerClass;
        private readonly AndroidJavaClass _activityCompatClass;
        private readonly AndroidJavaClass _contextCompatClass;
        private readonly AndroidJavaObject _currentActivity;
        private readonly AndroidJavaClass _uriClass;

        private bool requestedPermissionInSession = false;

        public AndroidPermissions(string unityPlayerClassPath)
        {
            this._callback = new OnRequestPermissionsResultCallback(OnRequestPermissionResult);
            this._unityPlayerClass = new AndroidJavaClass(unityPlayerClassPath);
            this._activityCompatClass = new AndroidJavaClass("android.support.v4.app.ActivityCompat");
            this._contextCompatClass = new AndroidJavaClass("android.support.v4.content.ContextCompat");
            this._currentActivity = this._unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            this._uriClass = new AndroidJavaClass("android.net.Uri");

            this._unityPlayerClass.Call("setOnRequestPermissionsResultCallback", this._callback);
        }

        public PermissionStatus For(string permission)
        {
            int selfPermission = this._contextCompatClass.Call<int>("checkSelfPermission", permission);
            bool shouldDisplayPermission = this._activityCompatClass.Call<bool>("shouldShowRequestPermissionRationale", this._currentActivity, permission);

            if (selfPermission == PERMISSION_GRANTED)
                return PermissionStatus.Authorized;

            if (selfPermission == PERMISSION_GRANTED)
            {
                if (shouldDisplayPermission)
                    return PermissionStatus.NotDeterminedButAsked;

                if (this.requestedPermissionInSession)
                    return PermissionStatus.Denied;

                return PermissionStatus.NotDetermined;
            }

            return PermissionStatus.Unknown;
        }

        public void RequestPermissionFor(string permission, Action<PermissionStatus> callback)
        {
            int requestCode = 234;
            string[] permissions = new string[] {permission};
            this._activityCompatClass.Call("requestPermissions", this._currentActivity, permissions, requestCode);
        }

        public void OpenApplicationSettings()
        {
            string packageName = this._currentActivity.Call<string>("getPackageName");
            string fragment = null;
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
            AndroidJavaObject uri = this._uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, fragment);
            intent = intent.Call<AndroidJavaObject>("setAction", ACTION_APPLICATION_DETAILS_SETTINGS);
            intent = intent.Call<AndroidJavaObject>("setData", uri);
            this._currentActivity.Call("startActivity", intent);
        }

        private void OnRequestPermissionResult(int requestCode, string[] requestedPermissions, int[] requestedPermissionResults)
        {
            this.requestedPermissionInSession = true;
            Debug.Log("Result " + requestCode);
        }


    }
}

using System;
using UnityEngine;

namespace UniNPermissions
{
    public class AndroidPermissions : IPermissions
    {
        private static class ClassName
        {
            public const string PermissionRequestFragment = "com.lupidan.uninpermissionsandroid.PermissionRequestFragment";
            public const string ActivityCompat = "android.support.v4.app.ActivityCompat";
            public const string ContextCompat = "android.support.v4.content.ContextCompat";
            public const string UnityPlayer = "com.unity3d.player.UnityPlayer";
        }

        private static class InterfaceName
        {
            public const string PermissionRequestFragmentResultCallback = "com.lupidan.uninpermissionsandroid.PermissionRequestFragment$ResultCallback";
        }

        private static class MethodName
        {
            public const string CurrentActivity = "currentActivity";
            public const string SetResultCallback = "setResultCallback";
            public const string CheckSelfPermission = "checkSelfPermission";
            public const string ShouldShowRequestPermissionRationale = "shouldShowRequestPermissionRationale";
            public const string NewInstance = "newInstance";
            public const string GetFragmentManager = "getFragmentManager";
            public const string BeginTransaction = "beginTransaction";
            public const string Add = "add";
            public const string Commit = "commit";
            public const string OpenApplicationSettings = "openApplicationSettings";
        }

        private const int PERMISSION_GRANTED = 0;
        private const int PERMISSION_DENIED = -1;

        private class ResultCallback : AndroidJavaProxy
        {
            private readonly Action<int, string, int> _resultCallback;

            public ResultCallback(Action<int, string, int> onPermissionResultCallback) : base(InterfaceName.PermissionRequestFragmentResultCallback)
            {
                this._resultCallback = onPermissionResultCallback;
            }

            void onRequestPermissionResult(int requestCode, string permission, int grantResult)
            {
                this._resultCallback(requestCode, permission, grantResult);
            }
        }

        private readonly AndroidJavaClass _activityCompatClass;
        private readonly AndroidJavaClass _contextCompatClass;
        private readonly AndroidJavaClass _permissionRequestFragmentClass;
        private readonly AndroidJavaObject _currentActivity;

        private Action<PermissionStatus> _permissionResultCallback;
        private bool _requestedPermissionInSession = false;

        public AndroidPermissions(string requestCallbackInterfaceName)
        {
            this._activityCompatClass = new AndroidJavaClass(ClassName.ActivityCompat);
            this._contextCompatClass = new AndroidJavaClass(ClassName.ContextCompat);
            this._permissionRequestFragmentClass = new AndroidJavaClass(ClassName.PermissionRequestFragment);

            var unityPlayer = new AndroidJavaClass(ClassName.UnityPlayer);
            this._currentActivity = unityPlayer.GetStatic<AndroidJavaObject>(MethodName.CurrentActivity);

            var callback = new ResultCallback(OnRequestPermissionResult);
            this._permissionRequestFragmentClass.CallStatic(MethodName.SetResultCallback, callback);
        }

        public PermissionStatus For(string permission)
        {
            var selfPermission = this._contextCompatClass.CallStatic<int>(MethodName.CheckSelfPermission, this._currentActivity, permission);
            var shouldDisplayRationale = this._activityCompatClass.CallStatic<bool>(MethodName.ShouldShowRequestPermissionRationale, this._currentActivity, permission);

            if (selfPermission == PERMISSION_GRANTED)
                return PermissionStatus.Authorized;

            if (selfPermission == PERMISSION_DENIED)
            {
                if (shouldDisplayRationale)
                    return PermissionStatus.NotDeterminedButAsked;

                if (this._requestedPermissionInSession)
                    return PermissionStatus.Denied;

                return PermissionStatus.NotDetermined;
            }

            return PermissionStatus.Unknown;
        }

        public void RequestPermissionFor(string permission, Action<PermissionStatus> callback)
        {
            this._permissionResultCallback = callback;
            int requestCode = 234;
            string[] permissions = new string[] {permission};

            var fragment = this._permissionRequestFragmentClass.CallStatic<AndroidJavaObject>(MethodName.NewInstance, permissions, requestCode);

            var fragmentManager = this._currentActivity.Call<AndroidJavaObject>(MethodName.GetFragmentManager);
            var fragmentTransaction = fragmentManager
                .Call<AndroidJavaObject>(MethodName.BeginTransaction)
                .Call<AndroidJavaObject>(MethodName.Add, 0, fragment)
                .Call<int>(MethodName.Commit);
        }

        public void OpenApplicationSettings()
        {
            this._permissionRequestFragmentClass.CallStatic(MethodName.OpenApplicationSettings, this._currentActivity);
        }

        private void OnRequestPermissionResult(int requestCode, string requestedPermission, int requestedPermissionResult)
        {
            Debug.Log(string.Format("Received requestCode={0} permission={1} result={2}", requestCode, requestedPermission, requestedPermissionResult));
            this._requestedPermissionInSession = true;
            this._permissionResultCallback(For(requestedPermission));
        }
    }
}

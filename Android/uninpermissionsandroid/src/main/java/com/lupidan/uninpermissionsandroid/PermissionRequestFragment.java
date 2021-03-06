package com.lupidan.uninpermissionsandroid;

import android.annotation.TargetApi;
import android.app.Fragment;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.provider.Settings;
import android.support.annotation.NonNull;

@TargetApi(Build.VERSION_CODES.M)
public class PermissionRequestFragment extends Fragment {

    private static final String PERMISSIONS_TO_REQUEST_BUNDLE_KEY = "permissionsToRequest";
    private static final String PERMISSION_REQUESTED_BUNDLE_KEY = "permissionRequested";
    private static final String REQUEST_CODE_BUNDLE_KEY = "requestCode";

    private static ResultCallback sResultCallback;

    public interface ResultCallback
    {
        void onRequestPermissionResult(int requestCode, String permission, int grantResult);
    }

    public static void setResultCallback(ResultCallback resultCallback) {
        sResultCallback = resultCallback;
    }

    public static void openApplicationSettings(Context context)
    {
        Uri appSettingsUri = Uri.fromParts("package", context.getPackageName(), null);
        Intent appSettingsIntent = new Intent();
        appSettingsIntent.setAction(Settings.ACTION_APPLICATION_DETAILS_SETTINGS);
        appSettingsIntent.setData(appSettingsUri);
        context.startActivity(appSettingsIntent);
    }

    public static PermissionRequestFragment newInstance(String[] permissionsToRequest, int requestCode) {
        Bundle arguments = new Bundle();
        arguments.putStringArray(PERMISSIONS_TO_REQUEST_BUNDLE_KEY, permissionsToRequest);
        arguments.putInt(REQUEST_CODE_BUNDLE_KEY, requestCode);

        final PermissionRequestFragment fragment = new PermissionRequestFragment();
        fragment.setArguments(arguments);
        return fragment;
    }

    private String[] mPermissionsToRequest;
    private int mRequestCode;

    @Override
    public void onStart() {
        super.onStart();

        Bundle arguments = getArguments();
        mPermissionsToRequest = arguments.getStringArray(PERMISSIONS_TO_REQUEST_BUNDLE_KEY);
        mRequestCode = arguments.getInt(REQUEST_CODE_BUNDLE_KEY);

        if (!arguments.getBoolean(PERMISSION_REQUESTED_BUNDLE_KEY)) {
            requestPermissions(mPermissionsToRequest, mRequestCode);
            arguments.putBoolean(PERMISSION_REQUESTED_BUNDLE_KEY, true);
        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        for (int i=0; i < permissions.length; ++i)
            sResultCallback.onRequestPermissionResult(requestCode, permissions[i], grantResults[i]);
    }
}

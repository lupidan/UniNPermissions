package com.lupidan.UniNPermission;

import com.unity3d.player.UnityPlayerActivity;
import android.os.Bundle;
import android.util.Log;

public class UniNPermissionPlayer extends UnityPlayerActivity {

	private ActivityCompat.OnRequestPermissionsResultCallback onRequestPermissionsResultCallback;
    public void setOnRequestPermissionsResultCallback(ActivityCompat.OnRequestPermissionsResultCallback onRequestPermissionsResultCallback) {
        this.onRequestPermissionsResultCallback = onRequestPermissionsResultCallback;
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        this.onRequestPermissionsResultCallback.onRequestPermissionsResult(requestCode, permissions, grantResults);
    }
    
}
using UniNPermissions;
using UnityEngine;

public class TestingMenu : MonoBehaviour
{
	private AndroidPermissions _permissions;

	private void Awake()
	{
		this._permissions = new AndroidPermissions();
	}

	public void RequestPermissionButtonPressed()
	{
		CheckAccessToFineLocation();
	}

	private void CheckAccessToFineLocation()
	{
		PermissionStatus status = this._permissions.For("android.permission.ACCESS_FINE_LOCATION");
		Debug.Log("Checking permission is "+status);
		switch (status)
		{
			case PermissionStatus.NotDetermined:
				Debug.Log("Requesting permission");
				this._permissions.RequestPermissionFor("android.permission.ACCESS_FINE_LOCATION", permissionStatus =>
					{
						Debug.Log("Callback received!! "+permissionStatus);
						if (permissionStatus == PermissionStatus.NotDeterminedButAsked)
						{
							CheckAccessToFineLocation();
						}
					});
				break;

			case PermissionStatus.NotDeterminedButAsked:
				Debug.Log("Maybe you should give some explanation as to why you want the permission");
				goto case PermissionStatus.NotDetermined;

			case PermissionStatus.Authorized:
				Debug.Log("Cool!! We have access to the permission!!");
				break;

			case PermissionStatus.Denied:
				Debug.Log("You do not have access to that permission. Opening Application settings.");
				this._permissions.OpenApplicationSettings();
				break;
		}
	}
}

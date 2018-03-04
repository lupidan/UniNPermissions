using UniNPermissions;
using UnityEngine;

public class TestingMenu : MonoBehaviour
{
	private AndroidPermissions _permissions;

	private void Awake()
	{
		this._permissions = new AndroidPermissions("com.lupidan.UniNPermission.UniNPermissionPlayer");
	}

	public void RequestPermissionButtonPressed()
	{
		CheckAccessToFineLocation();
	}

	private void CheckAccessToFineLocation()
	{
		PermissionStatus status = this._permissions.For("android.permission.ACCESS_FINE_LOCATION");
		switch (status)
		{
			case PermissionStatus.NotDetermined:
				this._permissions.RequestPermissionFor("android.permission.ACCESS_FINE_LOCATION", permissionStatus =>
					{
						CheckAccessToFineLocation();
					});
				break;

			case PermissionStatus.NotDeterminedButAsked:
				Debug.Log("Maybe you should give some explanation as to why you want the permission");
				goto case PermissionStatus.NotDetermined;

			case PermissionStatus.Authorized:
				Debug.Log("Cool!! We have access to the permission!!");
				break;

			case PermissionStatus.Denied:
				Debug.Log("You do not have access to that permission.");
				break;
		}
	}
}

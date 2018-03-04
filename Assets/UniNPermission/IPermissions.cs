
using System;

namespace UniNPermissions
{
    public interface IPermissions
    {
        PermissionStatus For(string permission);
        void RequestPermissionFor(string permission, Action<PermissionStatus> callback);
    }
}

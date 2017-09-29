using System;
using Microsoft.AspNetCore.Identity;

namespace Demeter.IdentityComponent.Model
{
    public class DemeterUserLogin : IEquatable<UserLoginInfo>, IEquatable<DemeterUserLogin>
    {
        public string LoginProvider { get; private set; }
        public string ProviderKey { get; private set; }
        public string ProviderDisplayName { get; private set; }

        public DemeterUserLogin(UserLoginInfo loginInfo)
        {
            if (loginInfo == null)
            {
                throw new ArgumentNullException(nameof(loginInfo));
            }

            this.LoginProvider = loginInfo.LoginProvider;
            this.ProviderKey = loginInfo.ProviderKey;
            this.ProviderDisplayName = loginInfo.ProviderDisplayName;
        }

        bool IEquatable<UserLoginInfo>.Equals(UserLoginInfo other) => 
            other.LoginProvider.Equals(this.LoginProvider) && other.ProviderKey.Equals(this.ProviderKey);
        
        bool IEquatable<DemeterUserLogin>.Equals(DemeterUserLogin other) =>
            other.LoginProvider.Equals(this.LoginProvider) && other.ProviderKey.Equals(this.ProviderKey);
    }
}
using System;
using System.Collections.Generic;
using Demeter.IdentityComponent.Model;
using MongoDB.Bson;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Demeter.IdentityComponent
{
    public class DemeterUserIdentity
    {
        private List<DemeterUserClaim> _claims;
        private List<DemeterUserLogin> _logins;
        private Dictionary<String, Object> _profiles;
        private List<string> _roles;

        public string Id { get; private set; }
        public string UserName { get; private set; }
        public string NormalizedUserName { get; private set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; private set; }

        public int AccessFailedCount { get; private set; }
        public Occurence CreatedOn { get; private set; }
        public Occurence DeleteOn { get; private set; }
        public Occurence LockoutEndOn { get; private set; }
        public bool IsLockoutEnabled { get; private set; }

        public IEnumerable<DemeterUserClaim> Claims
        {
            get
            {
                this.EnsureClaims();
                return this._claims;
            }

            private set
            {
                this.EnsureClaims();
                if (value != null)
                {
                    this._claims.AddRange(value);
                }
            }
        }

        public IEnumerable<DemeterUserLogin> Logins
        {
            get
            {
                this.EnsureLogins();
                return this._logins;
            }

            private set
            {
                this.EnsureLogins();
                if (value != null)
                {
                    this._logins.AddRange(value);
                }
            }
        }

        public BsonDocument Profiles
        {
            get
            {
                this.EnsureProfiles();
                return new BsonDocument(this._profiles);
            }
        }

        public IEnumerable<string> Roles
        {
            get
            {
                this.EnsureRoles();
                return this._roles;
            }

            private set
            {
                this.EnsureRoles();
                if (value != null)
                {
                    this._roles.AddRange(value);
                }
            }
        }

        public DemeterUserIdentity(string userName)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            this.UserName = userName;
            this.Id = ObjectId.GenerateNewId().ToString();
            this.CreatedOn = new Occurence();
        }

        public void SetProfile<T>(string key, T profile)
        {
            this.EnsureProfiles();

            this._profiles.Add(key, profile);
        }
        public T GetProfile<T>(string key)
        {
            if (this._profiles == null)
            {
                return default(T);
            }

            if (this._profiles.ContainsKey(key) == false)
            {
                return default(T);
            }

            return (T) this._profiles[key];
        }

        public void SetAccessFailedCount(int accessFailedCount)
            => this.AccessFailedCount = accessFailedCount;

        public void ResetAccessFailedCount() => this.AccessFailedCount = 0;

        public void EnableLockout() => this.IsLockoutEnabled = true;

        public void DisableLockout() => this.IsLockoutEnabled = false;

        public void LockUntil(DateTime lockoutOn) => this.LockoutEndOn = new Occurence(lockoutOn);

        public void SetNormalizedUserName(string normalizedUserName)
            => this.NormalizedUserName = normalizedUserName;
        
        public void SetPasswordHash(string passwordHash) => this.PasswordHash = passwordHash;

        public void SetSecurityStamp(string securityStamp) => this.SecurityStamp = securityStamp;

        public void SetUserName(string userName) => this.UserName = userName;

        public void Delete()
        {
            if (this.DeleteOn != null)
            {
                throw new InvalidOperationException($"User '{this.Id}' has already deleted");
            }

            this.DeleteOn = new Occurence();
        }

        public void AddClaim(Claim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            this.AddClaim(new DemeterUserClaim(claim));
        }

        public void AddClaim(DemeterUserClaim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            this.EnsureClaims();
            this._claims.Add(claim);
        }

        public void RemoveClaim(Claim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            this.RemoveClaim(new DemeterUserClaim(claim.Type, claim.Value));
        }

        public void RemoveClaim(DemeterUserClaim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            this.EnsureClaims();
            this._claims.Remove(claim);
        }

        public void AddLogin(UserLoginInfo login)
        {
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            this.AddLogin(new DemeterUserLogin(login));
        }

        public void AddLogin(DemeterUserLogin login)
        {
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            this.EnsureLogins();
            this._logins.Add(login);
        }

        public void RemoveLogin(UserLoginInfo login)
        {
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            this.RemoveLogin(new DemeterUserLogin(login));
        }

        public void RemoveLogin(DemeterUserLogin login)
        {
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            this.EnsureLogins();
            this._logins.Remove(login);
        }

        public void AddRole(string role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            this.EnsureRoles();
            this._roles.Add(role);
        }

        public void RemoveRole(string role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            this.EnsureRoles();
            this._roles.Remove(role);
        }

        private void EnsureProfiles()
        {
            if (this._profiles == null)
            {
                this._profiles = new Dictionary<string, Object>();
            }
        }

        private void EnsureClaims()
        {
            if (this._claims == null)
            {
                this._claims = new List<DemeterUserClaim>();
            }
        }

        private void EnsureLogins()
        {
            if (this._logins == null)
            {
                this._logins = new List<DemeterUserLogin>();
            }
        }

        private void EnsureRoles()
        {
            if (this._roles == null)
            {
                this._roles = new List<string>();
            }
        }
    }
}
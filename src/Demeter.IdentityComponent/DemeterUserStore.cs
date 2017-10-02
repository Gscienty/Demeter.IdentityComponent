using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Security.Claims;
using Demeter.IdentityComponent.Model;

namespace Demeter.IdentityComponent
{
    public class DemeterUserStore<TUser>
        : IUserStore<TUser>
        , IUserClaimStore<TUser>
        , IUserLockoutStore<TUser>
        , IUserLoginStore<TUser>
        , IUserPasswordStore<TUser>
        , IUserSecurityStampStore<TUser>
        , IUserRoleStore<TUser>
        
        where TUser : DemeterUserIdentity
    {
        private static bool _initialized = false;
        private static object _initializationLock = new object();
        private static object _initializationTarget;

        private readonly IMongoCollection<TUser> _usersCollection;

        public DemeterUserStore(IMongoDatabase database, string usersCollection)
            : this(database, usersCollection, null) { }

        public DemeterUserStore(IMongoDatabase database, string usersCollection,
            IEnumerable<Tuple<CreateIndexOptions, Expression<Func<TUser, object>>>> indices)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (usersCollection == null)
            {
                throw new ArgumentNullException(nameof(usersCollection));
            }

            this._usersCollection = database.GetCollection<TUser>(usersCollection);

            this.EnsureIndeicesCreatedAsync(indices).GetAwaiter().GetResult();
        }

        public Task SetProfile<T>(TUser user, string key, T profile,
            CancellationToken cancellationToken) where T : class, new()
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            user.SetProfile<T>(key, profile);

            return Task.FromResult(0);
        }

        Task IUserClaimStore<TUser>.AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if(claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            foreach(Claim claim in claims)
            {
                user.AddClaim(claim);
            }

            return Task.FromResult(0);
        }

        Task IUserLoginStore<TUser>.AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (login == null)
            {
                 throw new ArgumentNullException(nameof(login));
            }

            if (user.Logins.Any(x => x.Equals(login)))
            {
                throw new InvalidOperationException("login already exist.");
            }

            user.AddLogin(login);

            return Task.FromResult(0);
        }

        async Task<IdentityResult> IUserStore<TUser>.CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await this._usersCollection.InsertOneAsync(user, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            
            return IdentityResult.Success;
        }

        async Task<IdentityResult> IUserStore<TUser>.DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            cancellationToken.ThrowIfCancellationRequested();

            user.Delete();

            var query = Builders<TUser>.Filter.Eq(u => u.Id, user.Id);
            var update = Builders<TUser>.Update.Set(u => u.DeleteOn, user.DeleteOn);

            var result = await this._usersCollection.UpdateOneAsync(
                query,
                update,
                new UpdateOptions { IsUpsert = false },
                cancellationToken);
            return result.IsModifiedCountAvailable && result.ModifiedCount == 1
                ? IdentityResult.Success
                : IdentityResult.Failed();
        }

        Task<TUser> IUserStore<TUser>.FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var query = Builders<TUser>.Filter.And(
                Builders<TUser>.Filter.Eq(u => u.Id, userId),
                Builders<TUser>.Filter.Eq(u => u.DeleteOn, null)
            );

            return this._usersCollection.Find(query).FirstOrDefaultAsync(cancellationToken);
        }

        Task<TUser> IUserLoginStore<TUser>.FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (loginProvider == null)
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }
            if (providerKey == null)
            {
                throw new ArgumentNullException(nameof(providerKey));
            }

            var query = Builders<TUser>.Filter.And(
                Builders<TUser>.Filter.Eq(u => u.DeleteOn, null),
                Builders<TUser>.Filter.ElemMatch(u => u.Logins,
                    Builders<DemeterUserLogin>.Filter.And(
                        Builders<DemeterUserLogin>.Filter.Eq(l => l.ProviderKey, providerKey),
                        Builders<DemeterUserLogin>.Filter.Eq(l => l.LoginProvider, loginProvider)
                    )
                )
            );

            return this._usersCollection.Find(query).FirstOrDefaultAsync(cancellationToken);
        }

        Task<TUser> IUserStore<TUser>.FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            if (normalizedUserName == null)
            {
                throw new ArgumentNullException(nameof(normalizedUserName));
            }

            var query = Builders<TUser>.Filter.And(
                Builders<TUser>.Filter.Eq(u => u.NormalizedUserName, normalizedUserName),
                Builders<TUser>.Filter.Eq(u => u.DeleteOn, null)
            );

            return this._usersCollection.Find(query).FirstOrDefaultAsync(cancellationToken);
        }

        Task<int> IUserLockoutStore<TUser>.GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.AccessFailedCount);
        }

        Task<IList<Claim>> IUserClaimStore<TUser>.GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult<IList<Claim>>(
                user.Claims.Select(c => new Claim(c.Type, c.Value)).ToList()
            );
        }

        Task<bool> IUserLockoutStore<TUser>.GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.IsLockoutEnabled);
        }

        Task<DateTimeOffset?> IUserLockoutStore<TUser>.GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var lockoutEndDate = user.LockoutEndOn != null
                ? new DateTimeOffset(user.LockoutEndOn.Instance)
                : default(DateTimeOffset?);
            return Task.FromResult(lockoutEndDate);
        }

        Task<IList<UserLoginInfo>> IUserLoginStore<TUser>.GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult<IList<UserLoginInfo>>(
                user.Logins
                    .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName))
                    .ToList()
            );
        }

        Task<string> IUserStore<TUser>.GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.NormalizedUserName);
        }

        Task<string> IUserPasswordStore<TUser>.GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PasswordHash);
        }

        Task<string> IUserSecurityStampStore<TUser>.GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.SecurityStamp);
        }

        Task<string> IUserStore<TUser>.GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Id);
        }

        Task<string> IUserStore<TUser>.GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.UserName);
        }

        async Task<IList<TUser>> IUserClaimStore<TUser>.GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            var query = Builders<TUser>.Filter.And(
                Builders<TUser>.Filter.Eq(u => u.DeleteOn, null),
                Builders<TUser>.Filter.ElemMatch(u => u.Claims,
                    Builders<DemeterUserClaim>.Filter.And(
                        Builders<DemeterUserClaim>.Filter.Eq(c => c.Type, claim.Type),
                        Builders<DemeterUserClaim>.Filter.Eq(c => c.Value, claim.Value)
                    )
                )
            );

            var users = await this._usersCollection.Find(query).ToListAsync().ConfigureAwait(false);

            return users;
        }

        Task<bool> IUserPasswordStore<TUser>.HasPasswordAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PasswordHash != null);
        }

        Task<int> IUserLockoutStore<TUser>.IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                 throw new ArgumentNullException(nameof(user));
            }

            int accessFailedCount = user.AccessFailedCount + 1;
            user.SetAccessFailedCount(accessFailedCount);

            return Task.FromResult(accessFailedCount);
        }

        Task IUserClaimStore<TUser>.RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            foreach(Claim claim in claims)
            {
                user.RemoveClaim(claim);
            }

            return Task.FromResult(0);
        }

        Task IUserLoginStore<TUser>.RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (loginProvider == null)
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }
            if (providerKey == null)
            {
                throw new ArgumentNullException(nameof(providerKey));
            }

            UserLoginInfo login = new UserLoginInfo(loginProvider, providerKey, string.Empty);
            DemeterUserLogin removeLogin = user.Logins.FirstOrDefault(x => x.Equals(login));

            if (removeLogin != null)
            {
                user.RemoveLogin(removeLogin);
            }

            return Task.FromResult(0);
        }

        Task IUserClaimStore<TUser>.ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }
            if (newClaim == null)
            {
                throw new ArgumentNullException(nameof(newClaim));
            }

            user.RemoveClaim(claim);
            user.AddClaim(newClaim);

            return Task.FromResult(0);
        }

        Task IUserLockoutStore<TUser>.ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.ResetAccessFailedCount();

            return Task.FromResult(0);
        }

        Task IUserLockoutStore<TUser>.SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (enabled)
            {
                user.EnableLockout();
            }
            else
            {
                user.DisableLockout();
            }

            return Task.FromResult(0);
        }

        Task IUserLockoutStore<TUser>.SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (lockoutEnd != null)
            {
                user.LockUntil(lockoutEnd.Value.UtcDateTime);
            }

            return Task.FromResult(0);
        }

        Task IUserStore<TUser>.SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (normalizedName == null)
            {
                throw new ArgumentNullException(nameof(normalizedName));
            }

            user.SetNormalizedUserName(normalizedName);

            return Task.FromResult(0);
        }

        Task IUserPasswordStore<TUser>.SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (passwordHash == null)
            {
                throw new ArgumentNullException(nameof(passwordHash));
            }

            user.SetPasswordHash(passwordHash);

            return Task.FromResult(0);
        }

        Task IUserSecurityStampStore<TUser>.SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (stamp == null)
            {
                throw new ArgumentNullException(nameof(stamp));
            }

            user.SetSecurityStamp(stamp);

            return Task.FromResult(0);
        }

        Task IUserStore<TUser>.SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            user.SetUserName(userName);

            return Task.FromResult(0);
        }

        async Task<IdentityResult> IUserStore<TUser>.UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var query = Builders<TUser>.Filter.And(
                Builders<TUser>.Filter.Eq(u => u.Id, user.Id),
                Builders<TUser>.Filter.Eq(u => u.DeleteOn, null)
            );

            var result = await this._usersCollection.ReplaceOneAsync(
                query,
                user,
                new UpdateOptions { IsUpsert = false },
                cancellationToken).ConfigureAwait(false);
            
            return result.IsModifiedCountAvailable && result.ModifiedCount == 1
                ? IdentityResult.Success
                : IdentityResult.Failed();
        }

        Task IUserRoleStore<TUser>.AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            user.AddRole(roleName);

            return Task.FromResult(0);
        }

        Task IUserRoleStore<TUser>.RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            user.RemoveRole(roleName);

            return Task.FromResult(0);
        }

        Task<IList<string>> IUserRoleStore<TUser>.GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult<IList<string>>(user.Roles.ToList());
        }

        Task<bool> IUserRoleStore<TUser>.IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            return Task.FromResult(user.Roles.Any(x => x.Equals(roleName)));
        }

        async Task<IList<TUser>> IUserRoleStore<TUser>.GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var query = Builders<TUser>.Filter.And(
                Builders<TUser>.Filter.Eq(u => u.DeleteOn, null),
                Builders<TUser>.Filter.ElemMatch(u => u.Roles,
                    Builders<string>.Filter.Eq(role => role, roleName)
                )
            );

            return await this._usersCollection.Find(query)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        void IDisposable.Dispose() { }

        private async Task EnsureIndeicesCreatedAsync(
            IEnumerable<Tuple<CreateIndexOptions, Expression<Func<TUser, object>>>> indices)
        {
            var obj = LazyInitializer.EnsureInitialized(
                ref _initializationTarget, 
                ref _initialized,
                ref _initializationLock, () => {
                    return this.EnsureIndeicesCreatedImplementAsync(indices);
                });
            if (obj != null)
            {
                var awaitTask = (Task)obj;

                await awaitTask.ConfigureAwait(false);
            }
        }

        private async Task EnsureIndeicesCreatedImplementAsync(
            IEnumerable<Tuple<CreateIndexOptions, Expression<Func<TUser, object>>>> indices)
        {

            List<Task<string>> taskList = new List<Task<string>>();

            taskList.Add(this._usersCollection.Indexes.CreateOneAsync(
                Builders<TUser>.IndexKeys.Ascending(u => u.UserName),
                new CreateIndexOptions
                {
                    Name = "identity_username_unique",
                    Unique = true
                }
            ));

            if (indices != null)
            {
                foreach(var item in indices)
                {
                    taskList.Add(
                        this._usersCollection.Indexes.CreateOneAsync(
                            Builders<TUser>.IndexKeys.Ascending(item.Item2),
                            item.Item1
                        )
                    );
                }
            }


            await Task.WhenAll(taskList).ConfigureAwait(false);
        }
    }
}
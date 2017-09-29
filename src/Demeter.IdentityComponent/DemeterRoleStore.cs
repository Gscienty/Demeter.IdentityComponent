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
    public class DemeterRoleStore<TRole>
        : IRoleStore<TRole>
        , IRoleClaimStore<TRole>
        
        where TRole : DemeterRole
    {
        private static bool _initialized = false;
        private static object _initializationLock = new object();
        private static object _initializationTarget;

        private readonly IMongoCollection<TRole> _roleCollection;

        public DemeterRoleStore(
            IMongoDatabase database,
            string rolesCollection)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }
            if (rolesCollection == null)
            {
                throw new ArgumentNullException(nameof(rolesCollection));
            }

            this._roleCollection = database.GetCollection<TRole>(rolesCollection);

            this.EnsureIndeicesCreatedAsync().GetAwaiter().GetResult();
        }

        Task IRoleClaimStore<TRole>.AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            role.AddClaim(claim);

            return Task.FromResult(0);
        }

        async Task<IdentityResult> IRoleStore<TRole>.CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            cancellationToken.ThrowIfCancellationRequested();

            await this._roleCollection
                .InsertOneAsync(role, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return IdentityResult.Success;
        }

        async Task<IdentityResult> IRoleStore<TRole>.DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            cancellationToken.ThrowIfCancellationRequested();

            role.Delete();

            var query = Builders<TRole>.Filter.Eq(r => r.Id, role.Id);
            var update = Builders<TRole>.Update.Set(r => r.DeleteOn, role.DeleteOn);

            var result = await this._roleCollection.UpdateOneAsync(
                query,
                update,
                new UpdateOptions { IsUpsert = false },
                cancellationToken
            );

            return result.IsModifiedCountAvailable && result.ModifiedCount == 1
                ? IdentityResult.Success
                : IdentityResult.Failed();
        }

        void IDisposable.Dispose() { }

        async Task<TRole> IRoleStore<TRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            if (roleId == null)
            {
                throw new ArgumentNullException(nameof(roleId));
            }

            var query = Builders<TRole>.Filter.And(
                Builders<TRole>.Filter.Eq(r => r.Id, roleId),
                Builders<TRole>.Filter.Eq(r => r.DeleteOn, null)
            );

            return await this._roleCollection
                .Find(query)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        async Task<TRole> IRoleStore<TRole>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (normalizedRoleName == null)
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            var query = Builders<TRole>.Filter.And(
                Builders<TRole>.Filter.Eq(r => r.NormalizedRoleName, normalizedRoleName),
                Builders<TRole>.Filter.Eq(r => r.DeleteOn, null)
            );

            return await this._roleCollection
                .Find(query)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        Task<IList<Claim>> IRoleClaimStore<TRole>.GetClaimsAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult<IList<Claim>>(role.Claims
                .Select(x => new Claim(x.Type, x.Value)).ToList());
        }

        Task<string> IRoleStore<TRole>.GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.NormalizedRoleName);
        }

        Task<string> IRoleStore<TRole>.GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Id);
        }

        Task<string> IRoleStore<TRole>.GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.RoleName);
        }

        Task IRoleClaimStore<TRole>.RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            role.RemoveClaim(claim);
            
            return Task.FromResult(0);
        }

        Task IRoleStore<TRole>.SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            if (normalizedName == null)
            {
                throw new ArgumentNullException(nameof(normalizedName));
            }

            role.SetNormalizedRoleName(normalizedName);

            return Task.FromResult(0);
        }

        Task IRoleStore<TRole>.SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            role.SetRoleName(roleName);

            return Task.FromResult(0);
        }

        async Task<IdentityResult> IRoleStore<TRole>.UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var query = Builders<TRole>.Filter.And(
                Builders<TRole>.Filter.Eq(r => r.Id, role.Id),
                Builders<TRole>.Filter.Eq(r => r.DeleteOn, null)
            );

            cancellationToken.ThrowIfCancellationRequested();

            var result = await this._roleCollection.ReplaceOneAsync(
                query,
                role,
                new UpdateOptions { IsUpsert = false },
                cancellationToken
            ).ConfigureAwait(false);

            return result.IsModifiedCountAvailable && result.ModifiedCount == 1
                ? IdentityResult.Success
                : IdentityResult.Failed();
        }

        private async Task EnsureIndeicesCreatedAsync()
        {
            var obj = LazyInitializer.EnsureInitialized(
                ref _initializationTarget, 
                ref _initialized,
                ref _initializationLock, () => {
                    return this.EnsureIndeicesCreatedImplementAsync();
                });
            if (obj != null)
            {
                var awaitTask = (Task)obj;

                await awaitTask.ConfigureAwait(false);
            }
        }

        private async Task EnsureIndeicesCreatedImplementAsync()
        {

            await this._roleCollection.Indexes.CreateOneAsync(
                Builders<TRole>.IndexKeys.Ascending(u => u.RoleName),
                new CreateIndexOptions
                {
                    Name = "identity_role_unique",
                    Unique = true
                }
            ).ConfigureAwait(false);
        }
    }
}
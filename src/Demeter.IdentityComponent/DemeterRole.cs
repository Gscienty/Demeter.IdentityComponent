using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Demeter.IdentityComponent.Model;
using MongoDB.Bson;

namespace Demeter.IdentityComponent
{
    public class DemeterRole
    {
        private List<DemeterUserClaim> _claims;
        public string Id { get; private set; }
        public string RoleName { get; private set; }
        public string NormalizedRoleName { get; private set; }
        public Occurence DeleteOn { get; private set; }

        public List<DemeterUserClaim> Claims
        {
            get
            {
                this.EnsureClaims();
                return this._claims;
            }
        }

        public DemeterRole(string roleName)
        {
            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            this.RoleName = roleName;
            this.Id = ObjectId.GenerateNewId().ToString();
        }

        public void AddClaim(Claim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }
            this.AddClaim(claim);
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

            DemeterUserClaim removeClaim = this._claims
                .FirstOrDefault(x => x.Type == claim.Type && x.Value == claim.Value);
            if (removeClaim != null)
            {
                this._claims.Remove(removeClaim);
            }
        }

        public void SetNormalizedRoleName(string normalizedRoleName)
        {
            if (normalizedRoleName == null)
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            this.NormalizedRoleName = normalizedRoleName;
        }

        public void SetRoleName(string roleName)
        {
            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            this.RoleName = roleName;
        }

        public void Delete()
        {
            this.DeleteOn = new Occurence();
        }

        private void EnsureClaims()
        {
            if (this._claims == null)
            {
                this._claims = new List<DemeterUserClaim>();
            }
        }
    }
}
using System;
using System.Security.Claims;

namespace Demeter.IdentityComponent.Model
{
    public class DemeterUserClaim : IEquatable<Claim>, IEquatable<DemeterUserClaim>
    {
        public string Type { get; private set; }
        public string Value { get; private set; }

        public DemeterUserClaim(Claim claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            this.Type = claim.Type;
            this.Value = claim.Value;
        }

        public DemeterUserClaim(string claimType, string claimValue)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException(nameof(claimType));
            }
            if (claimValue == null)
            {
                throw new ArgumentNullException(nameof(claimValue));
            }

            this.Type = claimType;
            this.Value = claimValue;
        }

        bool IEquatable<Claim>.Equals(Claim other) =>
            other.Type.Equals(this.Type) && other.Value.Equals(this.Value);
        
        bool IEquatable<DemeterUserClaim>.Equals(DemeterUserClaim other) =>
            other.Type.Equals(this.Type) && other.Value.Equals(this.Value);
    }
}
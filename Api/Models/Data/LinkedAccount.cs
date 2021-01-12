using System;

namespace Api.Models.Data
{
    public class LinkedAccount
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public string UserId { get; set; }

        public string UserDetails { get; set; }

        public string IdentityProvider { get; set; }

        public int StravaAccountId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Profile { get; set; }

        public string TokenType { get; set; }

        public string AccessToken { get; set; }

        public DateTime ExpiresAt { get; set; }

        public string RefreshToken { get; set; }
    }
}

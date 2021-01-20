using System;
using Microsoft.Azure.Cosmos.Table;

namespace Api.Models.Data
{
    public class LinkedAccount : TableEntity
    {
        public string IdpUserId { get; set; }

        public string IdpUserName { get; set; }

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

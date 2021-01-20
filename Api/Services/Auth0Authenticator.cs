using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Api.Models.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services
{
    public interface IAuth0Authenticator
    {
        Task<(ClaimsPrincipal User, SecurityToken ValidatedToken)> AuthenticateAsync(HttpRequest request, ILogger log, CancellationToken cancellationToken);

        string GetUserId(ClaimsPrincipal user);
    }

    /// <summary>
    /// A type that authenticates users against an Auth0 account.
    /// </summary>
    public sealed class Auth0Authenticator : IAuth0Authenticator
    {
        private readonly TokenValidationParameters _parameters;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _manager;
        private readonly JwtSecurityTokenHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="Auth0Authenticator"/> class. In most cases, you should only have one authenticator instance in your application.
        /// </summary>
        /// <param name="configuration">Configuration object.</param>
        public Auth0Authenticator(IConfiguration configuration)
        {
            // The domain of the Auth0 account, e.g., "myauth0test.auth0.com"
            string auth0Domain = configuration["Auth0Domain"];
            if (string.IsNullOrEmpty(auth0Domain))
            {
                throw new Exception("The 'Auth0Domain' cannot be null or empty, ensure it is added to configuration.");
            }

            // The valid audiences for tokens. This must include the "audience" of the access_token request, and may also include a "client id" to enable id_tokens from clients you own.
            string audience = configuration["APIAudience"];
            if (string.IsNullOrEmpty(audience))
            {
                throw new Exception("The 'APIAudience' cannot be null or empty, ensure it is added to configuration.");
            }

            _parameters = new TokenValidationParameters
            {
                ValidIssuer = $"https://{auth0Domain}/",
                ValidAudiences = new string[] { audience },
                ValidateIssuerSigningKey = true,
            };

            _manager = new ConfigurationManager<OpenIdConnectConfiguration>($"https://{auth0Domain}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            _handler = new JwtSecurityTokenHandler();
        }

        /// <summary>
        /// Authenticates the user token. Returns a user principal containing claims from the token and a token that can be used to perform actions on behalf of the user.
        /// Throws an exception if the token fails to authenticate.
        /// This method has an asynchronous signature, but usually completes synchronously.
        /// </summary>
        /// <param name="request">http request.</param>
        /// <param name="log">Instance of logger class.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        public async Task<(ClaimsPrincipal User, SecurityToken ValidatedToken)> AuthenticateAsync(HttpRequest request, ILogger log, CancellationToken cancellationToken)
        {
            try
            {
                AuthenticationHeaderValue header = AuthenticationHeaderValue.Parse(request.Headers["Authorization"]);
                if (header == null || !string.Equals(header.Scheme, "Bearer", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException("Authentication header does not use Bearer token.");
                }

                string token = header.Parameter;

                // Note: ConfigurationManager<T> has an automatic refresh interval of 1 day.
                //   The config is cached in-between refreshes, so this "asynchronous" call actually completes synchronously unless it needs to refresh.
                var config = await _manager.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

                _parameters.IssuerSigningKeys = config.SigningKeys;
                var user = _handler.ValidateToken(token, _parameters, out var validatedToken);

                return (user, validatedToken);
            }
            catch (Exception ex)
            {
                log.LogError("Authorization failed", ex);
                throw new AuthException();
            }
        }

        public string GetUserId(ClaimsPrincipal user)
        {
            // The user's ID is available in the NameIdentifier claim
            string userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            return userId;
        }
    }
}

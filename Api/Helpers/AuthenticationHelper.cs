using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Api.Helpers
{
    public static class AuthenticationHelper
    {
        public static ClientPrincipal GetClientPrincipal(HttpRequest httpRequest, ILogger logger)
        {
            ClientPrincipal principal = null;

            if (EnvironmentHelper.UseMockData)
            {
                principal = new ClientPrincipal()
                {
                    IdentityProvider = "mock_data",
                    UserId = "test123",
                    UserDetails = "Benedict Cumberbatch",
                    UserRoles = new List<string>() { "anonymous", "authenticated" }
                };

                logger.LogInformation($"Returning mock data: user id='{principal?.UserId}', user details='{principal?.UserDetails}', identity provider='{principal.IdentityProvider}'");
            }
            else
            {
                if (httpRequest.Headers.TryGetValue("x-ms-client-principal", out var header))
                {
                    var data = header[0];
                    var decoded = Convert.FromBase64String(data);
                    var json = Encoding.ASCII.GetString(decoded);

                    principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                    logger.LogInformation($"Found user: user id='{principal?.UserId}', user details='{principal?.UserDetails}', identity provider='{principal.IdentityProvider}'");
                }
                else
                {
                    throw new Exception("Cannot find 'x-ms-client-principal' header");
                }
            }

            return principal;
        }
    }
}

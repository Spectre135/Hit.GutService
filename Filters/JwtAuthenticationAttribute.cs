#region using
using Hit.AuthJwt.JwtToken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
#endregion

namespace Hit.Auth.Filters
{

    public class JwtAuthenticationAttribute : Attribute,IAuthenticationFilter
    {
        public string Realm { get; set; }
        public bool AllowMultiple => false;
        public static IPrincipal user;
        private string Role { get; set; }

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = context.Request;
            var authorization = request.Headers.Authorization;

            if (authorization == null || authorization.Scheme != "HitAuthToken")
                return;

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing Jwt Token", request);
                return;
            }

            var token = authorization.Parameter;
            var principal = await AuthenticateJwtToken(token);

            if (principal == null)
                context.ErrorResult = new AuthenticationFailureResult("Invalid token", request);

            //Preverimo ali uporabnik ima pravico, če pravice ne rabimo vrača true;
            if (!JwtManager.HasRole(token,Role))
                context.ErrorResult = new PermissionFailureResult("Permission denied", request);

            context.Principal = principal;


        }

        private static bool ValidateToken(string token, out string username, out string id_delavec)
        {
            username = null;
            id_delavec = null;

            var simplePrinciple = JwtManager.GetPrincipal(token);
            var identity = simplePrinciple?.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            var usernameClaim = identity.FindFirst(ClaimTypes.Name);
            username = usernameClaim?.Value;

            if (string.IsNullOrEmpty(username))
                return false;

            // More validate to check whether username exists in system
            id_delavec = identity.FindFirst(ClaimTypes.Sid).Value;

            return true;
        }

        protected Task<IPrincipal> AuthenticateJwtToken(string token)
        {
            string username;
            string id_delavec;

            if (ValidateToken(token, out username, out id_delavec))
            {
                // based on username to get more information from database in order to build local identity
                var claim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Sid, id_delavec)
                };

                var identity = new ClaimsIdentity(claim, "Jwt");
                user = new ClaimsPrincipal(identity);

                return Task.FromResult(user);
            }

            return Task.FromResult<IPrincipal>(null);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            Challenge(context);
            return Task.FromResult(0);
        }

        private void Challenge(HttpAuthenticationChallengeContext context)
        {
            string parameter = null;

            if (!string.IsNullOrEmpty(Realm))
                parameter = "realm=\"" + Realm + "\"";

            context.ChallengeWith("HitAuthToken", parameter);
        }

        public JwtAuthenticationAttribute(string Role)
        {
            this.Role = Role; 
        }

        public JwtAuthenticationAttribute()
        {
        }

    }
}
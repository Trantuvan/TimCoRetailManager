using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Portal.Authentication
{
    public static class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwT(string jwt)
        {
            var claims = new List<Claim>();
            //jwt = "header.payLoad.verificationInfo"
            //payLoad = data (claims)
            var payLoad = jwt.Split('.')[1];

            var jsonBytes = ParseBase64WithoutPadding(payLoad);

            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            //After Extracting Role keyValuePairs need to be remove so doesn't have duplicate role
            ExtractRolesFromJwT(claims, keyValuePairs);

            //Adding back key and value to Claim except roles
            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));

            return claims;
        }

        private static void ExtractRolesFromJwT(List<Claim> claims, Dictionary<string, object> keyValuePairs)
        {
            //TryGetValue from ClaimTypes.Role, if it exist pull out to obj roles
            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

            if (roles is not null)
            {
                var parseRoles = roles.ToString().Trim().TrimStart('[').TrimEnd(']').Split(',');

                if (parseRoles.Length > 1)
                {
                    foreach (var parseRole in parseRoles)
                    {
                        //adding more info to List<Claim> claims, so whoever call ExtractRolesFromJwT have access
                        //to the same List<Claim> claims will have more claims
                        claims.Add(new Claim(ClaimTypes.Role, parseRole.Trim('"')));
                    }
                }
                else
                {
                    //co 1 role add 1 role from array of string
                    claims.Add(new Claim(ClaimTypes.Role, parseRoles[0]));
                }

                //After Extracting Role keyValuePairs need to be remove 
                keyValuePairs.Remove(ClaimTypes.Role);
            }
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}

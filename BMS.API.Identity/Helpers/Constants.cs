namespace Identity.Helpers
{
    public static class Constants
    {
        public static class Strings
        {
            public static class JwtClaimIdentifiers
            {
                public const string Rol = "rol", Id = "id", Dep = "dep", Des = "des";
                public const string TwoFA = "2fa";
            }

            public static class JwtClaims
            {
                public const string ApiAccess = "api_access";

                public const string TwoFactorAccess = "two_factor_access";
            }
        }
    }
}

using H5;

namespace Dashboard.Api
{
    /// <summary>
    /// Authenticated user info persisted to localStorage. Plain class
    /// (not a record) so JSON.parse / JSON.stringify interop works without
    /// constructor invocation.
    /// </summary>
    public sealed class AuthUser
    {
        /// <summary>Stable user ID.</summary>
        public string UserId { get; set; }

        /// <summary>Display name shown in UI.</summary>
        public string DisplayName { get; set; }

        /// <summary>Email address.</summary>
        public string Email { get; set; }
    }

    /// <summary>
    /// Persistent auth state stored in localStorage.
    /// </summary>
    public static class Auth
    {
        private const string TokenKey = "gatekeeper_token";
        private const string UserKey = "gatekeeper_user";

        /// <summary>Returns the stored bearer token, or null.</summary>
        public static string GetToken() =>
            Script.Write<string>("window.localStorage.getItem('gatekeeper_token')");

        /// <summary>Persists the bearer token.</summary>
        public static void SetToken(string token) =>
            Script.Write("window.localStorage.setItem('gatekeeper_token', token)");

        /// <summary>Returns the stored user, or null.</summary>
        public static AuthUser GetUser()
        {
            var json = Script.Write<string>("window.localStorage.getItem('gatekeeper_user')");
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            var raw = Script.Call<object>("JSON.parse", json);
            return new AuthUser
            {
                UserId = Script.Get<string>(raw, "userId"),
                DisplayName = Script.Get<string>(raw, "displayName"),
                Email = Script.Get<string>(raw, "email"),
            };
        }

        /// <summary>Persists the current user.</summary>
        public static void SetUser(AuthUser user)
        {
            var json = Script.Call<string>("JSON.stringify", user);
            Script.Write("window.localStorage.setItem('gatekeeper_user', json)");
        }

        /// <summary>Clears all auth state.</summary>
        public static void Clear()
        {
            Script.Write("window.localStorage.removeItem('gatekeeper_token')");
            Script.Write("window.localStorage.removeItem('gatekeeper_user')");
        }

        /// <summary>Returns true if a token is present.</summary>
        public static bool IsAuthenticated() => !string.IsNullOrEmpty(GetToken());
    }
}

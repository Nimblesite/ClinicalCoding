using System.Threading.Tasks;
using H5;

namespace Dashboard.Api
{
    /// <summary>
    /// Result returned from a successful Gatekeeper passkey login or registration.
    /// Plain class (not a record) so JSON.parse interop works without ctor invocation.
    /// </summary>
    public sealed class PasskeyAuthResult
    {
        /// <summary>Bearer token for subsequent API calls.</summary>
        public string Token { get; set; }

        /// <summary>Stable user ID.</summary>
        public string UserId { get; set; }

        /// <summary>Display name.</summary>
        public string DisplayName { get; set; }

        /// <summary>Email.</summary>
        public string Email { get; set; }
    }

    /// <summary>
    /// Gatekeeper passkey (WebAuthn) client. The WebAuthn ceremony itself
    /// (challenge encoding, navigator.credentials.get/create, ArrayBuffer
    /// conversions) is performed in inline JS via Script.Write because the
    /// browser WebAuthn API uses ArrayBuffer types that C# cannot model cleanly.
    /// </summary>
    public static class GatekeeperClient
    {
        /// <summary>
        /// Performs a discoverable-credential passkey login. Browser shows the
        /// passkey picker; on success the returned token is persisted to Auth.
        /// </summary>
        public static async Task<PasskeyAuthResult> LoginAsync()
        {
            var baseUrl = ApiClient.GatekeeperBaseUrl;
            var resultJson = await Script.Write<Task<string>>(@"
                (async function() {
                    if (!navigator.credentials || !navigator.credentials.get) {
                        throw new Error('WebAuthn unavailable. Use https or http://localhost.');
                    }
                    var beginRes = await fetch(baseUrl + '/auth/login/begin', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: '{}'
                    });
                    if (!beginRes.ok) {
                        var errBody = await beginRes.json().catch(function(){return{};});
                        throw new Error(errBody.Error || ('Login begin failed: HTTP ' + beginRes.status));
                    }
                    var beginData = await beginRes.json();
                    var opts = JSON.parse(beginData.OptionsJson);
                    opts.challenge = base64UrlDecode(opts.challenge);
                    delete opts.allowCredentials;
                    opts.timeout = 120000;
                    var assertion = await navigator.credentials.get({ publicKey: opts });
                    var assertionData = {
                        id: base64UrlEncode(assertion.rawId),
                        rawId: base64UrlEncode(assertion.rawId),
                        type: assertion.type,
                        response: {
                            authenticatorData: base64UrlEncode(assertion.response.authenticatorData),
                            clientDataJSON: base64UrlEncode(assertion.response.clientDataJSON),
                            signature: base64UrlEncode(assertion.response.signature),
                            userHandle: assertion.response.userHandle ? base64UrlEncode(assertion.response.userHandle) : null
                        }
                    };
                    var completeRes = await fetch(baseUrl + '/auth/login/complete', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            ChallengeId: beginData.ChallengeId,
                            OptionsJson: beginData.OptionsJson,
                            AssertionResponse: assertionData
                        })
                    });
                    if (!completeRes.ok) {
                        var errBody = await completeRes.json().catch(function(){return{};});
                        throw new Error(errBody.Error || ('Login complete failed: HTTP ' + completeRes.status));
                    }
                    return await completeRes.text();

                    function base64UrlDecode(str) {
                        str = str.replace(/-/g, '+').replace(/_/g, '/');
                        while (str.length % 4) str += '=';
                        var bin = atob(str);
                        var bytes = new Uint8Array(bin.length);
                        for (var i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
                        return bytes.buffer;
                    }
                    function base64UrlEncode(buf) {
                        var bytes = new Uint8Array(buf);
                        var bin = '';
                        for (var i = 0; i < bytes.byteLength; i++) bin += String.fromCharCode(bytes[i]);
                        return btoa(bin).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
                    }
                })()
            ");

            return ParseAndPersist(resultJson);
        }

        /// <summary>
        /// Registers a new user with a passkey. Browser prompts the user to
        /// create a credential; on success the returned token is persisted.
        /// </summary>
        public static async Task<PasskeyAuthResult> RegisterAsync(string email, string displayName)
        {
            var baseUrl = ApiClient.GatekeeperBaseUrl;
            var resultJson = await Script.Write<Task<string>>(@"
                (async function() {
                    if (!navigator.credentials || !navigator.credentials.create) {
                        throw new Error('WebAuthn unavailable. Use https or http://localhost.');
                    }
                    var beginRes = await fetch(baseUrl + '/auth/register/begin', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ Email: email, DisplayName: displayName })
                    });
                    if (!beginRes.ok) {
                        var errBody = await beginRes.json().catch(function(){return{};});
                        throw new Error(errBody.Error || ('Register begin failed: HTTP ' + beginRes.status));
                    }
                    var beginData = await beginRes.json();
                    var opts = JSON.parse(beginData.OptionsJson);
                    opts.challenge = base64UrlDecode(opts.challenge);
                    opts.user.id = base64UrlDecode(opts.user.id);
                    if (opts.excludeCredentials) {
                        opts.excludeCredentials = opts.excludeCredentials.map(function(c) {
                            return Object.assign({}, c, { id: base64UrlDecode(c.id) });
                        });
                    }
                    opts.timeout = 120000;
                    var cred = await navigator.credentials.create({ publicKey: opts });
                    var attestation = {
                        id: base64UrlEncode(cred.rawId),
                        rawId: base64UrlEncode(cred.rawId),
                        type: cred.type,
                        response: {
                            attestationObject: base64UrlEncode(cred.response.attestationObject),
                            clientDataJSON: base64UrlEncode(cred.response.clientDataJSON)
                        }
                    };
                    var completeRes = await fetch(baseUrl + '/auth/register/complete', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            ChallengeId: beginData.ChallengeId,
                            OptionsJson: beginData.OptionsJson,
                            AttestationResponse: attestation
                        })
                    });
                    if (!completeRes.ok) {
                        var errBody = await completeRes.json().catch(function(){return{};});
                        throw new Error(errBody.Error || ('Register complete failed: HTTP ' + completeRes.status));
                    }
                    return await completeRes.text();

                    function base64UrlDecode(str) {
                        str = str.replace(/-/g, '+').replace(/_/g, '/');
                        while (str.length % 4) str += '=';
                        var bin = atob(str);
                        var bytes = new Uint8Array(bin.length);
                        for (var i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
                        return bytes.buffer;
                    }
                    function base64UrlEncode(buf) {
                        var bytes = new Uint8Array(buf);
                        var bin = '';
                        for (var i = 0; i < bytes.byteLength; i++) bin += String.fromCharCode(bytes[i]);
                        return btoa(bin).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
                    }
                })()
            ");

            return ParseAndPersist(resultJson);
        }

        /// <summary>
        /// Calls the Gatekeeper logout endpoint to revoke the token server-side
        /// and clears local auth state. Best-effort: local state is always cleared.
        /// </summary>
        public static async Task LogoutAsync()
        {
            var token = Auth.GetToken();
            var baseUrl = ApiClient.GatekeeperBaseUrl;
            if (!string.IsNullOrEmpty(token))
            {
                await Script.Write<Task<object>>(@"
                    fetch(baseUrl + '/auth/logout', {
                        method: 'POST',
                        headers: { 'Authorization': 'Bearer ' + token }
                    }).catch(function(err) {
                        console.warn('[Auth] Logout request failed:', err);
                    })
                ");
            }
            Auth.Clear();
        }

        private static PasskeyAuthResult ParseAndPersist(string json)
        {
            var parsed = Script.Call<PasskeyAuthResult>("JSON.parse", json);
            Auth.SetToken(parsed.Token);
            Auth.SetUser(new AuthUser
            {
                UserId = parsed.UserId,
                DisplayName = parsed.DisplayName,
                Email = parsed.Email,
            });
            return parsed;
        }
    }
}

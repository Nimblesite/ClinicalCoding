using System;
using Dashboard.Api;
using Dashboard.Components;
using Dashboard.React;
using H5;
using static Dashboard.React.Elements;
using static Dashboard.React.Hooks;

namespace Dashboard.Pages
{
    /// <summary>
    /// Mutable state class for the login page (React useState requires a single state object).
    /// </summary>
    public sealed class LoginState
    {
        /// <summary>"login" or "register".</summary>
        public string Mode { get; set; }

        /// <summary>Email field (register mode only).</summary>
        public string Email { get; set; }

        /// <summary>Display name field (register mode only).</summary>
        public string DisplayName { get; set; }

        /// <summary>Whether a request is in flight.</summary>
        public bool Loading { get; set; }

        /// <summary>Last error message, or null.</summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// Login screen with WebAuthn passkey authentication via Gatekeeper.
    /// Discoverable-credential login (no email needed) is the default; users
    /// can switch to register mode to create a new account.
    /// </summary>
    public static class LoginPage
    {
        /// <summary>
        /// Renders the login page. Calls onLogin with the authenticated user
        /// after a successful passkey ceremony.
        /// </summary>
        public static ReactElement Render(Action<AuthUser> onLogin)
        {
            var stateResult = UseState(
                new LoginState
                {
                    Mode = "login",
                    Email = "",
                    DisplayName = "",
                    Loading = false,
                    Error = null,
                }
            );
            var state = stateResult.State;
            var setState = stateResult.SetState;

            void Mutate(Action<LoginState> mutator)
            {
                var next = new LoginState
                {
                    Mode = state.Mode,
                    Email = state.Email,
                    DisplayName = state.DisplayName,
                    Loading = state.Loading,
                    Error = state.Error,
                };
                mutator(next);
                setState(next);
            }

            async void DoLogin()
            {
                Mutate(s =>
                {
                    s.Loading = true;
                    s.Error = null;
                });
                try
                {
                    var result = await GatekeeperClient.LoginAsync();
                    onLogin(
                        new AuthUser
                        {
                            UserId = result.UserId,
                            DisplayName = result.DisplayName,
                            Email = result.Email,
                        }
                    );
                }
                catch (Exception ex)
                {
                    Mutate(s =>
                    {
                        s.Loading = false;
                        s.Error = ex.Message;
                    });
                }
            }

            async void DoRegister()
            {
                Mutate(s =>
                {
                    s.Loading = true;
                    s.Error = null;
                });
                try
                {
                    var result = await GatekeeperClient.RegisterAsync(
                        state.Email,
                        state.DisplayName
                    );
                    onLogin(
                        new AuthUser
                        {
                            UserId = result.UserId,
                            DisplayName = result.DisplayName,
                            Email = result.Email,
                        }
                    );
                }
                catch (Exception ex)
                {
                    Mutate(s =>
                    {
                        s.Loading = false;
                        s.Error = ex.Message;
                    });
                }
            }

            // Expose a test hook for E2E tests to trigger login without WebAuthn
            UseEffect(
                () =>
                {
                    Action<object> triggerLogin = user =>
                    {
                        var authUser = new AuthUser
                        {
                            UserId = Script.Get<string>(user, "userId"),
                            DisplayName = Script.Get<string>(user, "displayName"),
                            Email = Script.Get<string>(user, "email"),
                        };
                        onLogin(authUser);
                    };
                    Script.Write("window.__triggerLogin = triggerLogin");
                },
                () => (Action)(() => Script.Write("delete window.__triggerLogin")),
                new object[0]
            );

            return Div(
                className: "login-page",
                dataTestId: "login-page",
                children: new[] { RenderCard(state, Mutate, DoLogin, DoRegister) }
            );
        }

        private static ReactElement RenderCard(
            LoginState state,
            Action<Action<LoginState>> mutate,
            Action onLogin,
            Action onRegister
        ) =>
            Div(
                className: "login-card",
                children: new[]
                {
                    RenderHeader(state.Mode),
                    state.Error != null
                        ? Div(className: "login-error", children: new[] { Text(state.Error) })
                        : Fragment(),
                    Form(
                        onSubmit: () =>
                        {
                            if (state.Mode == "login")
                                onLogin();
                            else
                                onRegister();
                        },
                        children: state.Mode == "register"
                            ? new[]
                            {
                                RenderField(
                                    "Email",
                                    "email",
                                    state.Email,
                                    state.Loading,
                                    v => mutate(s => s.Email = v),
                                    "email"
                                ),
                                RenderField(
                                    "Display Name",
                                    "text",
                                    state.DisplayName,
                                    state.Loading,
                                    v => mutate(s => s.DisplayName = v),
                                    "displayName"
                                ),
                                RenderSubmit(state),
                            }
                            : new[] { RenderSubmit(state) }
                    ),
                    RenderFooter(state.Mode, mutate),
                }
            );

        private static ReactElement RenderHeader(string mode) =>
            Div(
                className: "login-header",
                children: new[]
                {
                    Img(
                        src: "img/nimblesite-logo.webp",
                        alt: "Nimblesite",
                        className: "login-logo-img"
                    ),
                    H(1, children: new[] { Text("Nimblesite Clinical Coding Platform") }),
                    P(
                        children: new[]
                        {
                            Text(
                                mode == "login"
                                    ? "Sign in with your passkey"
                                    : "Create your account"
                            ),
                        }
                    ),
                }
            );

        private static ReactElement RenderField(
            string label,
            string type,
            string value,
            bool disabled,
            Action<string> onChange,
            string fieldId = null
        ) =>
            Div(
                className: "form-group",
                children: new[]
                {
                    Label(className: "form-label", children: new[] { Text(label) }),
                    Input(
                        className: "form-input",
                        id: fieldId,
                        type: type,
                        value: value,
                        placeholder: label,
                        onChange: onChange,
                        disabled: disabled
                    ),
                }
            );

        private static ReactElement RenderSubmit(LoginState state) =>
            Button(
                className: "btn btn-primary login-btn",
                type: "submit",
                disabled: state.Loading,
                children: new[]
                {
                    Text(
                        state.Loading
                            ? "Please wait..."
                            : (
                                state.Mode == "login"
                                    ? "Sign in with Passkey"
                                    : "Register with Passkey"
                            )
                    ),
                }
            );

        private static ReactElement RenderFooter(string mode, Action<Action<LoginState>> mutate) =>
            Div(
                className: "login-footer",
                children: new[]
                {
                    P(
                        children: new[]
                        {
                            Text(
                                mode == "login"
                                    ? "Don't have an account? "
                                    : "Already have an account? "
                            ),
                            Button(
                                className: "link-btn",
                                type: "button",
                                onClick: () =>
                                    mutate(s =>
                                    {
                                        s.Mode = mode == "login" ? "register" : "login";
                                        s.Error = null;
                                    }),
                                children: new[] { Text(mode == "login" ? "Register" : "Sign in") }
                            ),
                        }
                    ),
                }
            );
    }
}

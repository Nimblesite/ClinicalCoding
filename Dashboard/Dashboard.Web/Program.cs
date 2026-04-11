using System;
using Dashboard.Api;
using Dashboard.React;
using H5;

namespace Dashboard
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point - called when H5 script loads.
        /// </summary>
        public static void Main()
        {
            // Configure API endpoints
            // Default to local development URLs
            var clinicalUrl = GetConfigValue("CLINICAL_API_URL", "http://localhost:5080");
            var schedulingUrl = GetConfigValue("SCHEDULING_API_URL", "http://localhost:5001");

            ApiClient.Configure(clinicalUrl, schedulingUrl);

            // Configure ICD-10 API endpoint
            var icd10Url = GetConfigValue("ICD10_API_URL", "http://localhost:5090");
            ApiClient.ConfigureIcd10(icd10Url);

            // Configure Gatekeeper (auth) endpoint. Tokens are minted by
            // Gatekeeper after a successful passkey ceremony and stored in
            // localStorage by Auth — there is no hardcoded dev JWT.
            var gatekeeperUrl = GetConfigValue("GATEKEEPER_API_URL", "http://localhost:5002");
            ApiClient.ConfigureGatekeeper(gatekeeperUrl);

            // Log startup
            Log("Nimblesite Clinical Coding Platform starting...");
            Log("Clinical API: " + clinicalUrl);
            Log("Scheduling API: " + schedulingUrl);
            Log("ICD-10 API: " + icd10Url);
            Log("Gatekeeper API: " + gatekeeperUrl);

            // Hide loading screen
            HideLoadingScreen();

            // Render the React application.
            // IMPORTANT: hooks (UseState etc.) must run inside a React render phase,
            // so pass App.Render as a function component to React.createElement rather
            // than invoking it eagerly.
            var appComponent = Script.Call<object>(
                "React.createElement",
                (Func<ReactElement>)App.Render
            );
            ReactInterop.RenderApp((ReactElement)appComponent);

            Log("Dashboard initialized successfully!");
        }

        private static string GetConfigValue(string key, string defaultValue)
        {
            // Try to get from window config object if available
            var windowConfig = Script.Get<object>("window", "dashboardConfig");
            if (windowConfig != null)
            {
                var value = Script.Get<string>(windowConfig, key);
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return defaultValue;
        }

        private static void HideLoadingScreen()
        {
            var loadingScreen = Script.Call<object>("document.getElementById", "loading-screen");
            if (loadingScreen != null)
            {
                Script.Write("loadingScreen.classList.add('hidden')");
            }
        }

        private static void Log(string message) =>
            Script.Call<object>("console.log", "[Dashboard] " + message);
    }
}

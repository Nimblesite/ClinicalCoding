using System;
using Dashboard.Api;
using Dashboard.Components;
using Dashboard.Pages;
using Dashboard.React;
using H5;
using static Dashboard.React.Elements;
using static Dashboard.React.Hooks;

namespace Dashboard
{
    /// <summary>
    /// Application state. Plain mutable class for H5 compatibility — records
    /// with init setters depend on IsExternalInit which H5 may not ship.
    /// </summary>
    public sealed class AppState
    {
        /// <summary>Active view identifier.</summary>
        public string ActiveView { get; set; }

        /// <summary>Whether sidebar is collapsed.</summary>
        public bool SidebarCollapsed { get; set; }

        /// <summary>Search query string.</summary>
        public string SearchQuery { get; set; }

        /// <summary>Notification count.</summary>
        public int NotificationCount { get; set; }

        /// <summary>Patient ID being edited (null if not editing).</summary>
        public string EditingPatientId { get; set; }

        /// <summary>Appointment ID being edited (null if not editing).</summary>
        public string EditingAppointmentId { get; set; }

        /// <summary>Whether the user has a valid Gatekeeper token.</summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>Authenticated user, or null when signed out.</summary>
        public AuthUser CurrentUser { get; set; }

        /// <summary>Returns a shallow copy of this state, suitable as the
        /// base for setState mutations.</summary>
        public AppState Clone() => new AppState
        {
            ActiveView = ActiveView,
            SidebarCollapsed = SidebarCollapsed,
            SearchQuery = SearchQuery,
            NotificationCount = NotificationCount,
            EditingPatientId = EditingPatientId,
            EditingAppointmentId = EditingAppointmentId,
            IsAuthenticated = IsAuthenticated,
            CurrentUser = CurrentUser,
        };
    }

    /// <summary>
    /// Main application component.
    /// </summary>
    public static class App
    {
        /// <summary>
        /// Renders the main application.
        /// </summary>
        public static ReactElement Render()
        {
            var stateResult = UseState(
                new AppState
                {
                    ActiveView = "dashboard",
                    SidebarCollapsed = false,
                    SearchQuery = "",
                    NotificationCount = 3,
                    EditingPatientId = null,
                    EditingAppointmentId = null,
                    IsAuthenticated = Auth.IsAuthenticated(),
                    CurrentUser = Auth.GetUser(),
                }
            );

            var state = stateResult.State;
            var setState = stateResult.SetState;

            if (!state.IsAuthenticated)
            {
                return AsComponent(() => LoginPage.Render(user =>
                {
                    var next = state.Clone();
                    next.IsAuthenticated = true;
                    next.CurrentUser = user;
                    next.ActiveView = "dashboard";
                    setState(next);
                }));
            }

            async void HandleLogout()
            {
                await GatekeeperClient.LogoutAsync();
                var next = state.Clone();
                next.IsAuthenticated = false;
                next.CurrentUser = null;
                next.ActiveView = "dashboard";
                next.EditingPatientId = null;
                next.EditingAppointmentId = null;
                setState(next);
            }

            return Div(
                className: "app",
                children: new[]
                {
                    // Sidebar
                    Sidebar.Render(
                        activeView: state.ActiveView,
                        onNavigate: view =>
                        {
                            var newState = state.Clone();
                            newState.ActiveView = view;
                            newState.EditingPatientId = null;
                            newState.EditingAppointmentId = null;
                            setState(newState);
                        },
                        collapsed: state.SidebarCollapsed,
                        onToggle: () =>
                        {
                            var newState = state.Clone();
                            newState.SidebarCollapsed = !state.SidebarCollapsed;
                            setState(newState);
                        },
                        currentUser: state.CurrentUser,
                        onLogout: HandleLogout
                    ),
                    // Main content wrapper
                    Div(
                        className: "main-wrapper",
                        children: new[]
                        {
                            // Header
                            Components.Header.Render(
                                title: GetPageTitle(state.ActiveView),
                                searchQuery: state.SearchQuery,
                                onSearchChange: query =>
                                {
                                    var newState = state.Clone();
                                    newState.SearchQuery = query;
                                    setState(newState);
                                },
                                notificationCount: state.NotificationCount
                            ),
                            // Main content area
                            Main(
                                className: "main-content",
                                children: new[] { RenderPage(state, setState) }
                            ),
                        }
                    ),
                }
            );
        }

        private static string GetPageTitle(string view)
        {
            if (view == "dashboard")
                return "Dashboard";
            if (view == "patients")
                return "Patients";
            if (view == "clinical-coding")
                return "Clinical Coding";
            if (view == "encounters")
                return "Encounters";
            if (view == "conditions")
                return "Conditions";
            if (view == "medications")
                return "Medications";
            if (view == "practitioners")
                return "Practitioners";
            if (view == "appointments")
                return "Appointments";
            if (view == "calendar")
                return "Schedule";
            if (view == "settings")
                return "Settings";
            return "Clinical Coding";
        }

        /// <summary>
        /// Wraps a parameterless render delegate as a React function component element.
        /// Hooks (UseState, UseEffect) require a render-phase context — eagerly invoking
        /// page Render() methods violates the rules of hooks.
        /// </summary>
        private static ReactElement AsComponent(Func<ReactElement> render) =>
            (ReactElement)Script.Call<object>(
                "React.createElement",
                render
            );

        private static ReactElement RenderPage(AppState state, Action<AppState> setState)
        {
            var view = state.ActiveView;

            // Handle editing patient
            if (view == "patients" && state.EditingPatientId != null)
            {
                var editingId = state.EditingPatientId;
                var snapshot = state;
                return AsComponent(() => EditPatientPage.Render(
                    editingId,
                    () =>
                    {
                        var next = snapshot.Clone();
                        next.ActiveView = "patients";
                        next.EditingPatientId = null;
                        next.EditingAppointmentId = null;
                        setState(next);
                    }
                ));
            }

            // Handle editing appointment
            if (
                (view == "appointments" || view == "calendar")
                && state.EditingAppointmentId != null
            )
            {
                var editingId = state.EditingAppointmentId;
                var snapshot = state;
                var returnView = view;
                return AsComponent(() => EditAppointmentPage.Render(
                    editingId,
                    () =>
                    {
                        var next = snapshot.Clone();
                        next.ActiveView = returnView;
                        next.EditingPatientId = null;
                        next.EditingAppointmentId = null;
                        setState(next);
                    }
                ));
            }

            if (view == "dashboard")
                return AsComponent(DashboardPage.Render);
            if (view == "clinical-coding")
                return AsComponent(ClinicalCodingPage.Render);
            if (view == "patients")
            {
                var snapshot = state;
                return AsComponent(() => PatientsPage.Render(patientId =>
                {
                    var next = snapshot.Clone();
                    next.ActiveView = "patients";
                    next.EditingPatientId = patientId;
                    next.EditingAppointmentId = null;
                    setState(next);
                }));
            }
            if (view == "practitioners")
                return AsComponent(PractitionersPage.Render);
            if (view == "appointments")
            {
                var snapshot = state;
                return AsComponent(() => AppointmentsPage.Render(appointmentId =>
                {
                    var next = snapshot.Clone();
                    next.ActiveView = "appointments";
                    next.EditingPatientId = null;
                    next.EditingAppointmentId = appointmentId;
                    setState(next);
                }));
            }
            if (view == "calendar")
            {
                var snapshot = state;
                return AsComponent(() => CalendarPage.Render(appointmentId =>
                {
                    var next = snapshot.Clone();
                    next.ActiveView = "calendar";
                    next.EditingPatientId = null;
                    next.EditingAppointmentId = appointmentId;
                    setState(next);
                }));
            }
            if (view == "encounters")
                return RenderPlaceholderPage("Encounters", "Manage patient encounters and visits");
            if (view == "conditions")
                return RenderPlaceholderPage("Conditions", "View and manage patient conditions");
            if (view == "medications")
                return RenderPlaceholderPage("Medications", "Manage medication requests");
            if (view == "settings")
                return RenderPlaceholderPage("Settings", "Configure application settings");
            return RenderPlaceholderPage("Page Not Found", "The requested page does not exist");
        }

        private static ReactElement RenderPlaceholderPage(string title, string description) =>
            Div(
                className: "page",
                children: new[]
                {
                    Div(
                        className: "page-header",
                        children: new[]
                        {
                            H(2, className: "page-title", children: new[] { Text(title) }),
                            P(className: "page-description", children: new[] { Text(description) }),
                        }
                    ),
                    Div(
                        className: "card",
                        children: new[]
                        {
                            Div(
                                className: "empty-state",
                                children: new[]
                                {
                                    Icons.Clipboard(),
                                    H(
                                        4,
                                        className: "empty-state-title",
                                        children: new[] { Text("Coming Soon") }
                                    ),
                                    P(
                                        className: "empty-state-description",
                                        children: new[] { Text("This page is under development.") }
                                    ),
                                }
                            ),
                        }
                    ),
                }
            );
    }
}

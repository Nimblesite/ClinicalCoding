using System;
using Dashboard.Api;
using Dashboard.Components;
using Dashboard.React;
using static Dashboard.React.Elements;
using static Dashboard.React.Hooks;

namespace Dashboard.Pages
{
    /// <summary>
    /// Dashboard state class.
    /// </summary>
    public class DashboardState
    {
        /// <summary>Patient count.</summary>
        public int PatientCount { get; set; }

        /// <summary>Practitioner count.</summary>
        public int PractitionerCount { get; set; }

        /// <summary>Appointment count.</summary>
        public int AppointmentCount { get; set; }

        /// <summary>Encounter count.</summary>
        public int EncounterCount { get; set; }

        /// <summary>Whether loading.</summary>
        public bool Loading { get; set; }

        /// <summary>Error message if any.</summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// Upcoming appointment sample row.
    /// </summary>
    public class UpcomingAppointment
    {
        /// <summary>Patient display name.</summary>
        public string Patient { get; set; }

        /// <summary>Patient initials for avatar.</summary>
        public string Initials { get; set; }

        /// <summary>Subtitle (treatment + room).</summary>
        public string Subtitle { get; set; }

        /// <summary>Time of day.</summary>
        public string Time { get; set; }

        /// <summary>Meta / countdown.</summary>
        public string Meta { get; set; }
    }

    /// <summary>
    /// Appointment request row.
    /// </summary>
    public class AppointmentRequest
    {
        /// <summary>Patient name.</summary>
        public string Patient { get; set; }

        /// <summary>Requested slot label.</summary>
        public string When { get; set; }

        /// <summary>Whether this is the highlighted primary request.</summary>
        public bool Primary { get; set; }
    }

    /// <summary>
    /// Main dashboard overview page — Clinical Curator design.
    /// </summary>
    public static class DashboardPage
    {
        private static Action<string> _onNavigate;

        /// <summary>
        /// Renders the dashboard page.
        /// </summary>
        public static ReactElement Render(Action<string> onNavigate = null)
        {
            _onNavigate = onNavigate;
            return RenderInternal();
        }

        private static ReactElement RenderInternal()
        {
            var stateResult = UseState(
                new DashboardState
                {
                    PatientCount = 0,
                    PractitionerCount = 0,
                    AppointmentCount = 0,
                    EncounterCount = 0,
                    Loading = true,
                    Error = null,
                }
            );

            var state = stateResult.State;
            var setState = stateResult.SetState;

            UseEffect(
                () =>
                {
                    LoadData(setState);
                },
                new object[0]
            );

            return Div(
                className: "page dashboard-page",
                children: new[]
                {
                    RenderWelcome(),
                    RenderConnectionWarning(state.Error),
                    RenderMetricsRow(state),
                    RenderMainGrid(),
                    RenderQuickActions(),
                }
            );
        }

        private static async void LoadData(Action<DashboardState> setState)
        {
            try
            {
                var patients = await ApiClient.GetPatientsAsync();
                var practitioners = await ApiClient.GetPractitionersAsync();
                var appointments = await ApiClient.GetAppointmentsAsync();

                setState(
                    new DashboardState
                    {
                        PatientCount = patients.Length,
                        PractitionerCount = practitioners.Length,
                        AppointmentCount = appointments.Length,
                        EncounterCount = 0,
                        Loading = false,
                        Error = null,
                    }
                );
            }
            catch (Exception ex)
            {
                setState(
                    new DashboardState
                    {
                        PatientCount = 0,
                        PractitionerCount = 0,
                        AppointmentCount = 0,
                        EncounterCount = 0,
                        Loading = false,
                        Error = ex.Message,
                    }
                );
            }
        }

        private static ReactElement RenderWelcome() =>
            Div(
                className: "dashboard-welcome page-header",
                children: new[]
                {
                    Div(
                        children: new[]
                        {
                            H(
                                2,
                                className: "welcome-title",
                                children: new[] { Text("Welcome back, Dr. Robert!") }
                            ),
                            P(
                                className: "page-description",
                                children: new[]
                                {
                                    Text("Here's what's happening in your department today."),
                                }
                            ),
                        }
                    ),
                    Div(
                        className: "welcome-actions",
                        children: new[]
                        {
                            Div(
                                className: "date-filter",
                                children: new[]
                                {
                                    Icons.Calendar(),
                                    Span(children: new[] { Text("Oct 24, 2023") }),
                                }
                            ),
                        }
                    ),
                }
            );

        private static ReactElement RenderConnectionWarning(string error)
        {
            if (error == null)
            {
                return Text("");
            }
            return Div(
                className: "alert alert-warning",
                children: new[]
                {
                    Div(
                        className: "flex items-center gap-3",
                        children: new[]
                        {
                            Icons.Bell(),
                            Div(
                                children: new[]
                                {
                                    H(
                                        4,
                                        className: "font-semibold",
                                        children: new[] { Text("Connection Warning") }
                                    ),
                                    P(
                                        className: "text-sm",
                                        children: new[]
                                        {
                                            Text(
                                                "Could not connect to API. Make sure Clinical API (5080) and Scheduling API (5001) are running."
                                            ),
                                        }
                                    ),
                                }
                            ),
                        }
                    ),
                }
            );
        }

        private static ReactElement RenderMetricsRow(DashboardState state) =>
            Div(
                className: "dashboard-metrics-row",
                children: new[]
                {
                    RenderTopTreatmentCard(),
                    RenderMetricCard(
                        label: "Satisfaction Rate",
                        value: "98.2",
                        trendLabel: "/ 100",
                        icon: Icons.TrendUp(),
                        accent: "tertiary"
                    ),
                    RenderMetricCard(
                        label: "Total Patients",
                        value: state.Loading ? "-" : state.PatientCount.ToString(),
                        trendLabel: "Active clinical cases",
                        icon: Icons.Users(),
                        accent: "secondary"
                    ),
                    RenderMetricCard(
                        label: "Appointments",
                        value: state.Loading ? "-" : state.AppointmentCount.ToString(),
                        trendLabel: "Scheduled for today",
                        icon: Icons.Calendar(),
                        accent: "primary"
                    ),
                }
            );

        private static ReactElement RenderTopTreatmentCard() =>
            Div(
                className: "metric-card metric-card-rich",
                children: new[]
                {
                    Div(
                        className: "metric-card-header",
                        children: new[]
                        {
                            Span(
                                className: "metric-card-title",
                                children: new[] { Text("Top Treatment") }
                            ),
                            Div(
                                className: "metric-card-icon primary",
                                children: new[] { Icons.Activity() }
                            ),
                        }
                    ),
                    Div(
                        className: "donut-chart-container",
                        children: new[]
                        {
                            Div(
                                style: new
                                {
                                    width = "44px",
                                    height = "44px",
                                    flexShrink = 0,
                                },
                                children: new[]
                                {
                                    Svg(
                                        width: 44,
                                        height: 44,
                                        viewBox: "0 0 36 36",
                                        children: new[]
                                        {
                                            Path(
                                                d: "M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831",
                                                fill: "none",
                                                stroke: "#dbe1ff",
                                                strokeWidth: 4
                                            ),
                                            Path(
                                                d: "M18 2.0845 a 15.9155 15.9155 0 0 1 20.3 15.9155",
                                                fill: "none",
                                                stroke: "#003fab",
                                                strokeWidth: 4
                                            ),
                                        }
                                    ),
                                }
                            ),
                            Div(
                                style: new { minWidth = 0, flex = "1" },
                                children: new[]
                                {
                                    H(
                                        3,
                                        className: "text-xl font-bold",
                                        children: new[] { Text("Cardiology") }
                                    ),
                                    P(
                                        className: "metric-card-breakdown",
                                        children: new[] { Text("+12%") }
                                    ),
                                }
                            ),
                        }
                    ),
                }
            );

        private static ReactElement RenderMetricCard(
            string label,
            string value,
            string trendLabel,
            ReactElement icon,
            string accent
        ) =>
            Div(
                className: "metric-card metric-card-rich",
                children: new[]
                {
                    Div(
                        className: "metric-card-header",
                        children: new[]
                        {
                            Span(className: "metric-card-title", children: new[] { Text(label) }),
                            Div(className: "metric-card-icon " + accent, children: new[] { icon }),
                        }
                    ),
                    Div(
                        className: "metric-card-body",
                        children: new[]
                        {
                            Div(
                                className: "metric-card-value-row",
                                children: new[]
                                {
                                    Span(
                                        className: "metric-card-value",
                                        children: new[] { Text(value) }
                                    ),
                                }
                            ),
                            P(
                                className: "metric-card-breakdown",
                                children: new[] { Text(trendLabel) }
                            ),
                        }
                    ),
                }
            );

        private static ReactElement RenderMainGrid() =>
            Div(
                className: "dashboard-main-grid",
                children: new[] { RenderAppointmentsList(), RenderRequestsColumn() }
            );

        private static ReactElement RenderAppointmentsList()
        {
            var appointments = GetUpcomingAppointments();
            var rows = new ReactElement[appointments.Length];
            for (var i = 0; i < appointments.Length; i++)
            {
                rows[i] = RenderAppointmentRow(appointments[i]);
            }

            return Div(
                className: "dashboard-appointments-section appointments-table",
                children: new[]
                {
                    Div(
                        className: "dashboard-section-header",
                        children: new[]
                        {
                            H(
                                3,
                                className: "dashboard-section-title",
                                children: new[] { Text("Upcoming Appointments") }
                            ),
                            A(
                                href: "#",
                                className: "view-more-link",
                                onClick: () => _onNavigate?.Invoke("appointments"),
                                children: new[] { Text("View Schedule") }
                            ),
                        }
                    ),
                    Div(children: rows),
                }
            );
        }

        private static ReactElement RenderAppointmentRow(UpcomingAppointment apt) =>
            Div(
                className: "row",
                children: new[]
                {
                    Div(
                        className: "patient-cell",
                        children: new[]
                        {
                            Div(
                                className: "avatar avatar-md",
                                children: new[] { Text(apt.Initials) }
                            ),
                            Div(
                                children: new[]
                                {
                                    Div(
                                        className: "font-bold",
                                        children: new[] { Text(apt.Patient) }
                                    ),
                                    Div(
                                        className: "text-xs text-gray-500",
                                        children: new[] { Text(apt.Subtitle) }
                                    ),
                                }
                            ),
                        }
                    ),
                    Div(
                        className: "text-right",
                        children: new[]
                        {
                            Div(className: "font-bold text-sm", children: new[] { Text(apt.Time) }),
                            Div(
                                className: "text-2xs text-gray-400 uppercase tracking-wider",
                                children: new[] { Text(apt.Meta) }
                            ),
                        }
                    ),
                }
            );

        private static ReactElement RenderRequestsColumn()
        {
            var requests = GetAppointmentRequests();
            var cards = new ReactElement[requests.Length + 2];
            cards[0] = Div(
                className: "dashboard-section-header",
                children: new[]
                {
                    H(
                        3,
                        className: "dashboard-section-title",
                        children: new[] { Text("Requests") }
                    ),
                    Span(className: "badge badge-error", children: new[] { Text("3 New") }),
                }
            );
            for (var i = 0; i < requests.Length; i++)
            {
                cards[i + 1] = RenderRequestCard(requests[i]);
            }
            cards[cards.Length - 1] = A(
                href: "#",
                className: "view-all-requests",
                children: new[] { Text("View All Requests (14)") }
            );

            return Div(className: "dashboard-requests-section", children: cards);
        }

        private static ReactElement RenderRequestCard(AppointmentRequest req) =>
            Div(
                className: req.Primary ? "appointment-request" : "appointment-request neutral",
                children: new[]
                {
                    Div(
                        className: "appointment-request-info",
                        children: new[]
                        {
                            Div(className: "icon-wrap", children: new[] { Icons.Users() }),
                            Div(
                                children: new[]
                                {
                                    Div(
                                        className: "appointment-request-name",
                                        children: new[] { Text(req.Patient) }
                                    ),
                                    Div(
                                        className: "appointment-request-time",
                                        children: new[] { Text("Requested for: " + req.When) }
                                    ),
                                }
                            ),
                        }
                    ),
                    Div(
                        className: "appointment-request-actions",
                        children: new[]
                        {
                            Button(
                                className: "btn btn-primary btn-sm flex-1",
                                children: new[] { Text("Approve") }
                            ),
                            Button(
                                className: "btn btn-outline btn-sm flex-1",
                                children: new[] { Text("Decline") }
                            ),
                        }
                    ),
                }
            );

        private static ReactElement RenderQuickActions() =>
            Div(
                className: "quick-actions-grid",
                children: new[]
                {
                    RenderQuickActionCard(
                        variant: "primary",
                        icon: Icons.Code(),
                        title: "Clinical Coding Guide",
                        description: "Review latest ICD-11 updates for cardiology.",
                        cta: "Access Library"
                    ),
                    RenderQuickActionCard(
                        variant: "tertiary",
                        icon: Icons.Users(),
                        title: "On-Call Directory",
                        description: "Direct contact list for emergency department staff.",
                        cta: "View Staff"
                    ),
                    RenderQuickActionCard(
                        variant: "neutral",
                        icon: Icons.Sparkles(),
                        title: "Lab Results",
                        description: "12 pending lab results require your digital signature.",
                        cta: "Review Results"
                    ),
                }
            );

        private static ReactElement RenderQuickActionCard(
            string variant,
            ReactElement icon,
            string title,
            string description,
            string cta
        ) =>
            Button(
                className: "quick-action-btn " + variant,
                children: new[]
                {
                    Div(
                        children: new[]
                        {
                            Div(className: "quick-action-icon", children: new[] { icon }),
                            H(4, children: new[] { Text(title) }),
                            P(children: new[] { Text(description) }),
                        }
                    ),
                    Span(className: "cta", children: new[] { Text(cta) }),
                }
            );

        private static UpcomingAppointment[] GetUpcomingAppointments() =>
            new[]
            {
                new UpcomingAppointment
                {
                    Patient = "John Simmons",
                    Initials = "JS",
                    Subtitle = "Post-Op Consultation • Room 402",
                    Time = "09:30 AM",
                    Meta = "In 15 minutes",
                },
                new UpcomingAppointment
                {
                    Patient = "Sarah Miller",
                    Initials = "SM",
                    Subtitle = "Routine Checkup • Virtual Session",
                    Time = "11:00 AM",
                    Meta = "Duration: 30m",
                },
                new UpcomingAppointment
                {
                    Patient = "Robert King",
                    Initials = "RK",
                    Subtitle = "New Patient Onboarding • Room 102",
                    Time = "01:45 PM",
                    Meta = "Pending History",
                },
            };

        private static AppointmentRequest[] GetAppointmentRequests() =>
            new[]
            {
                new AppointmentRequest
                {
                    Patient = "Emily Watson",
                    When = "Tomorrow, 10:00 AM",
                    Primary = true,
                },
                new AppointmentRequest
                {
                    Patient = "Marcus T.",
                    When = "Friday, 02:30 PM",
                    Primary = false,
                },
            };
    }
}

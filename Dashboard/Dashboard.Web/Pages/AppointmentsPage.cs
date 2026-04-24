using System;
using System.Linq;
using Dashboard.Api;
using Dashboard.Components;
using Dashboard.Models;
using Dashboard.React;
using H5;
using static Dashboard.React.Elements;
using static Dashboard.React.Hooks;

namespace Dashboard.Pages
{
    /// <summary>
    /// Appointments page state class.
    /// </summary>
    public class AppointmentsState
    {
        /// <summary>List of appointments.</summary>
        public Appointment[] Appointments { get; set; }

        /// <summary>Whether loading.</summary>
        public bool Loading { get; set; }

        /// <summary>Error message if any.</summary>
        public string Error { get; set; }

        /// <summary>Current status filter.</summary>
        public string StatusFilter { get; set; }

        /// <summary>Whether the add appointment modal is open.</summary>
        public bool ShowAddModal { get; set; }

        /// <summary>Service type for new appointment.</summary>
        public string NewServiceType { get; set; }
    }

    /// <summary>
    /// Appointments management page.
    /// </summary>
    public static class AppointmentsPage
    {
        /// <summary>
        /// Renders the appointments page.
        /// </summary>
        public static ReactElement Render(Action<string> onEditAppointment) =>
            RenderInternal(onEditAppointment);

        private static ReactElement RenderInternal(Action<string> onEditAppointment)
        {
            var stateResult = UseState(
                new AppointmentsState
                {
                    Appointments = new Appointment[0],
                    Loading = true,
                    Error = null,
                    StatusFilter = null,
                }
            );

            var state = stateResult.State;
            var setState = stateResult.SetState;

            UseEffect(
                () =>
                {
                    LoadAppointments(setState);
                },
                new object[0]
            );

            ReactElement content;
            if (state.Loading)
            {
                content = RenderLoadingList();
            }
            else if (state.Error != null)
            {
                content = RenderError(state.Error);
            }
            else if (state.Appointments.Length == 0)
            {
                content = RenderEmpty();
            }
            else
            {
                content = RenderAppointmentList(
                    state.Appointments,
                    state.StatusFilter,
                    onEditAppointment
                );
            }

            return Div(
                className: "page",
                children: new[]
                {
                    // Page header
                    Div(
                        className: "page-header flex justify-between items-center",
                        children: new[]
                        {
                            Div(
                                children: new[]
                                {
                                    H(
                                        2,
                                        className: "page-title",
                                        children: new[] { Text("Appointments") }
                                    ),
                                    P(
                                        className: "page-description",
                                        children: new[]
                                        {
                                            Text(
                                                "Manage scheduled appointments from the Scheduling domain"
                                            ),
                                        }
                                    ),
                                }
                            ),
                            Button(
                                className: "btn btn-primary",
                                dataTestId: "add-appointment-btn",
                                onClick: () =>
                                    setState(
                                        new AppointmentsState
                                        {
                                            Appointments = state.Appointments,
                                            Loading = false,
                                            Error = state.Error,
                                            StatusFilter = state.StatusFilter,
                                            ShowAddModal = true,
                                            NewServiceType = "",
                                        }
                                    ),
                                children: new[] { Icons.Plus(), Text("New Appointment") }
                            ),
                        }
                    ),
                    // Add modal
                    state.ShowAddModal
                        ? RenderAddModal(state, setState)
                        : Text(""),
                    // Filters
                    Div(
                        className: "card mb-6",
                        children: new[]
                        {
                            Div(
                                className: "tabs",
                                children: new[]
                                {
                                    RenderTab(
                                        "All",
                                        null,
                                        state.StatusFilter,
                                        s => FilterByStatus(s, state, setState)
                                    ),
                                    RenderTab(
                                        "Booked",
                                        "booked",
                                        state.StatusFilter,
                                        s => FilterByStatus(s, state, setState)
                                    ),
                                    RenderTab(
                                        "Arrived",
                                        "arrived",
                                        state.StatusFilter,
                                        s => FilterByStatus(s, state, setState)
                                    ),
                                    RenderTab(
                                        "Fulfilled",
                                        "fulfilled",
                                        state.StatusFilter,
                                        s => FilterByStatus(s, state, setState)
                                    ),
                                    RenderTab(
                                        "Cancelled",
                                        "cancelled",
                                        state.StatusFilter,
                                        s => FilterByStatus(s, state, setState)
                                    ),
                                }
                            ),
                        }
                    ),
                    // Content
                    content,
                }
            );
        }

        private static async void LoadAppointments(Action<AppointmentsState> setState)
        {
            try
            {
                var appointments = await ApiClient.GetAppointmentsAsync();
                setState(
                    new AppointmentsState
                    {
                        Appointments = appointments,
                        Loading = false,
                        Error = null,
                        StatusFilter = null,
                    }
                );
            }
            catch (Exception ex)
            {
                setState(
                    new AppointmentsState
                    {
                        Appointments = new Appointment[0],
                        Loading = false,
                        Error = ex.Message,
                        StatusFilter = null,
                    }
                );
            }
        }

        private static void FilterByStatus(
            string status,
            AppointmentsState currentState,
            Action<AppointmentsState> setState
        ) =>
            setState(
                new AppointmentsState
                {
                    Appointments = currentState.Appointments,
                    Loading = currentState.Loading,
                    Error = currentState.Error,
                    StatusFilter = status,
                }
            );

        private static ReactElement RenderTab(
            string label,
            string status,
            string currentFilter,
            Action<string> onSelect
        )
        {
            var isActive = status == currentFilter;
            return Button(
                className: "tab " + (isActive ? "active" : ""),
                onClick: () => onSelect(status),
                children: new[] { Text(label) }
            );
        }

        private static ReactElement RenderError(string message) =>
            Div(
                className: "card",
                style: new { borderLeft = "4px solid var(--error)" },
                children: new[]
                {
                    Div(
                        className: "flex items-center gap-3 p-4",
                        children: new[]
                        {
                            Icons.X(),
                            Text("Error loading appointments: " + message),
                        }
                    ),
                }
            );

        private static ReactElement RenderEmpty() =>
            Div(
                className: "card",
                children: new[]
                {
                    Div(
                        className: "empty-state",
                        children: new[]
                        {
                            Icons.Calendar(),
                            H(
                                4,
                                className: "empty-state-title",
                                children: new[] { Text("No Appointments") }
                            ),
                            P(
                                className: "empty-state-description",
                                children: new[]
                                {
                                    Text(
                                        "No appointments scheduled. Create a new appointment to get started."
                                    ),
                                }
                            ),
                            Button(
                                className: "btn btn-primary mt-4",
                                children: new[] { Icons.Plus(), Text("New Appointment") }
                            ),
                        }
                    ),
                }
            );

        private static ReactElement RenderLoadingList() =>
            Div(
                className: "data-list",
                children: Enumerable
                    .Range(0, 5)
                    .Select(i =>
                        Div(
                            className: "card",
                            children: new[]
                            {
                                Div(
                                    className: "flex items-center gap-4",
                                    children: new[]
                                    {
                                        Div(
                                            className: "skeleton",
                                            style: new
                                            {
                                                width = "60px",
                                                height = "60px",
                                                borderRadius = "var(--radius-lg)",
                                            }
                                        ),
                                        Div(
                                            className: "flex-1",
                                            children: new[]
                                            {
                                                Div(
                                                    className: "skeleton",
                                                    style: new { width = "200px", height = "20px" }
                                                ),
                                                Div(
                                                    className: "skeleton mt-2",
                                                    style: new { width = "150px", height = "16px" }
                                                ),
                                            }
                                        ),
                                        Div(
                                            className: "skeleton",
                                            style: new { width = "100px", height = "32px" }
                                        ),
                                    }
                                ),
                            }
                        )
                    )
                    .ToArray()
            );

        private static ReactElement RenderAppointmentList(
            Appointment[] appointments,
            string statusFilter,
            Action<string> onEditAppointment
        )
        {
            var filtered =
                statusFilter == null
                    ? appointments
                    : appointments.Where(a => a.Status == statusFilter).ToArray();

            var rows = filtered.Select(a => RenderAppointmentRow(a, onEditAppointment)).ToArray();

            return Table(className: "data-table", children: rows);
        }

        private static ReactElement RenderAppointmentRow(
            Appointment appointment,
            Action<string> onEditAppointment
        ) =>
            Tr(
                className: "appointment-row",
                children: new[]
                {
                    Td(children: new[] { Text(FormatTime(appointment.StartTime)) }),
                    Td(children: new[] { Text(appointment.ServiceType ?? "Appointment") }),
                    Td(children: new[] { RenderStatusBadge(appointment.Status) }),
                    Td(children: new[] { RenderPriorityBadge(appointment.Priority) }),
                    Td(children: new[] { Text(FormatReference(appointment.PatientReference)) }),
                    Td(
                        children: new[]
                        {
                            Button(
                                className: "btn btn-secondary btn-sm",
                                onClick: () =>
                                {
                                    Script.Write(
                                        "window.location.hash = '#appointments/edit/' + appointment.Id"
                                    );
                                    onEditAppointment(appointment.Id);
                                },
                                children: new[] { Icons.Edit() }
                            ),
                        }
                    ),
                }
            );

        private static ReactElement RenderStatusBadge(string status)
        {
            string badgeClass;
            if (status == "booked")
                badgeClass = "badge-primary";
            else if (status == "arrived")
                badgeClass = "badge-teal";
            else if (status == "fulfilled")
                badgeClass = "badge-success";
            else if (status == "cancelled")
                badgeClass = "badge-error";
            else if (status == "noshow")
                badgeClass = "badge-warning";
            else
                badgeClass = "badge-gray";

            return Span(className: "badge " + badgeClass, children: new[] { Text(status) });
        }

        private static ReactElement RenderPriorityBadge(string priority)
        {
            if (priority == "routine")
                return Text("");

            string badgeClass;
            if (priority == "urgent")
                badgeClass = "badge-warning";
            else if (priority == "asap")
                badgeClass = "badge-error";
            else if (priority == "stat")
                badgeClass = "badge-error";
            else
                badgeClass = "badge-gray";

            return Span(
                className: "badge " + badgeClass,
                children: new[] { Text(priority.ToUpper()) }
            );
        }

        private static string FormatTime(string dateTime)
        {
            if (string.IsNullOrEmpty(dateTime))
                return "N/A";
            // Simple time extraction - in real app use proper date parsing
            if (dateTime.Length > 16)
                return dateTime.Substring(11, 5);
            return dateTime;
        }

        private static string FormatReference(string reference)
        {
            // Extract ID from reference like "Patient/abc-123" -> "abc-123"
            var parts = reference.Split('/');
            if (parts.Length > 1)
            {
                var id = parts[1];
                var length = Math.Min(8, id.Length);
                return id.Substring(0, length) + "...";
            }
            return reference;
        }

        private static ReactElement RenderAddModal(
            AppointmentsState state,
            Action<AppointmentsState> setState
        ) =>
            Div(
                className: "modal",
                children: new[]
                {
                    Div(
                        className: "card",
                        style: new
                        {
                            padding = "24px",
                            maxWidth = "500px",
                            margin = "100px auto",
                        },
                        children: new[]
                        {
                            H(3, children: new[] { Text("New Appointment") }),
                            Div(
                                className: "mt-4",
                                children: new[]
                                {
                                    Input(
                                        className: "input",
                                        placeholder: "Service Type",
                                        value: state.NewServiceType ?? "",
                                        dataTestId: "appointment-service-type",
                                        onChange: v =>
                                            setState(
                                                new AppointmentsState
                                                {
                                                    Appointments = state.Appointments,
                                                    Loading = false,
                                                    Error = state.Error,
                                                    StatusFilter = state.StatusFilter,
                                                    ShowAddModal = true,
                                                    NewServiceType = v,
                                                }
                                            )
                                    ),
                                }
                            ),
                            Div(
                                className: "flex gap-2 mt-4",
                                children: new[]
                                {
                                    Button(
                                        className: "btn btn-primary",
                                        dataTestId: "submit-appointment",
                                        onClick: () =>
                                            CreateAppointment(state.NewServiceType, setState),
                                        children: new[] { Text("Create") }
                                    ),
                                    Button(
                                        className: "btn btn-secondary",
                                        onClick: () =>
                                            setState(
                                                new AppointmentsState
                                                {
                                                    Appointments = state.Appointments,
                                                    Loading = false,
                                                    Error = state.Error,
                                                    StatusFilter = state.StatusFilter,
                                                    ShowAddModal = false,
                                                }
                                            ),
                                        children: new[] { Text("Cancel") }
                                    ),
                                }
                            ),
                        }
                    ),
                }
            );

        private static async void CreateAppointment(
            string serviceType,
            Action<AppointmentsState> setState
        )
        {
            try
            {
                var now = Script.Write<string>("new Date().toISOString()");
                var appointment = new
                {
                    ServiceCategory = "General",
                    ServiceType = serviceType,
                    Priority = "routine",
                    Start = now,
                    End = now,
                    PatientReference = "Patient/1",
                    PractitionerReference = "Practitioner/1",
                };
                await ApiClient.CreateAppointmentAsync(appointment);
                LoadAppointments(setState);
            }
            catch (Exception ex)
            {
                setState(
                    new AppointmentsState
                    {
                        Appointments = new Appointment[0],
                        Loading = false,
                        Error = ex.Message,
                        ShowAddModal = false,
                    }
                );
            }
        }
    }
}

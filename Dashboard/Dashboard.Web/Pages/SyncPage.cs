using System;
using System.Linq;
using Dashboard.Api;
using Dashboard.Models;
using Dashboard.React;
using H5;
using static Dashboard.React.Elements;
using static Dashboard.React.Hooks;

namespace Dashboard.Pages
{
    /// <summary>
    /// Sync page state class.
    /// </summary>
    public class SyncState
    {
        /// <summary>All sync records merged from both services.</summary>
        public SyncRecord[] Records { get; set; }

        /// <summary>Whether loading.</summary>
        public bool Loading { get; set; }

        /// <summary>Clinical service status.</summary>
        public string ClinicalStatus { get; set; }

        /// <summary>Scheduling service status.</summary>
        public string SchedulingStatus { get; set; }

        /// <summary>Current service filter: all, clinical, scheduling.</summary>
        public string ServiceFilter { get; set; }

        /// <summary>Current action filter: all, or numeric operation.</summary>
        public string ActionFilter { get; set; }

        /// <summary>Current search query.</summary>
        public string SearchQuery { get; set; }
    }

    /// <summary>
    /// Sync record row model.
    /// </summary>
    public class SyncRecord
    {
        /// <summary>Source service: clinical or scheduling.</summary>
        public string Service { get; set; }

        /// <summary>Record identifier.</summary>
        public string Id { get; set; }

        /// <summary>Entity type.</summary>
        public string EntityType { get; set; }

        /// <summary>Operation code as string.</summary>
        public string Operation { get; set; }

        /// <summary>Timestamp.</summary>
        public string Timestamp { get; set; }

        /// <summary>Payload preview or data summary.</summary>
        public string Payload { get; set; }
    }

    /// <summary>
    /// Sync dashboard page showing cross-service sync activity.
    /// </summary>
    public static class SyncPage
    {
        /// <summary>
        /// Renders the sync page.
        /// </summary>
        public static ReactElement Render()
        {
            var stateResult = UseState(
                new SyncState
                {
                    Records = new SyncRecord[0],
                    Loading = true,
                    ClinicalStatus = "Connecting...",
                    SchedulingStatus = "Connecting...",
                    ServiceFilter = "all",
                    ActionFilter = "all",
                    SearchQuery = "",
                }
            );

            var state = stateResult.State;
            var setState = stateResult.SetState;

            UseEffect(
                () =>
                {
                    LoadSyncData(setState);
                },
                new object[0]
            );

            return Div(
                className: "page sync-page",
                dataTestId: "sync-page",
                children: new[]
                {
                    RenderHeader(),
                    RenderServiceStatus(state),
                    RenderFilters(state, setState),
                    RenderRecordsTable(state),
                }
            );
        }

        private static async void LoadSyncData(Action<SyncState> setState)
        {
            var clinicalStatus = "Unavailable";
            var schedulingStatus = "Unavailable";
            SyncRecord[] records;

            try
            {
                var clinicalJson = await ApiClient.FetchClinicalSyncRecordsAsync();
                var schedulingJson = await ApiClient.FetchSchedulingSyncRecordsAsync();

                var clinicalRecords = ParseRecords(clinicalJson, "clinical");
                var schedulingRecords = ParseRecords(schedulingJson, "scheduling");

                if (clinicalRecords != null)
                    clinicalStatus = "Connected";
                if (schedulingRecords != null)
                    schedulingStatus = "Connected";

                records = (clinicalRecords ?? new SyncRecord[0])
                    .Concat(schedulingRecords ?? new SyncRecord[0])
                    .ToArray();
            }
            catch (Exception)
            {
                records = new SyncRecord[0];
            }

            setState(
                new SyncState
                {
                    Records = records,
                    Loading = false,
                    ClinicalStatus = clinicalStatus,
                    SchedulingStatus = schedulingStatus,
                    ServiceFilter = "all",
                    ActionFilter = "all",
                    SearchQuery = "",
                }
            );
        }

        private static SyncRecord[] ParseRecords(string json, string service)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            var parsed = Script.Call<object>("JSON.parse", json);
            var items = Script.Get<object>(parsed, "Items") ?? parsed;
            var length = Script.Write<int>("(items && items.length) || 0");
            var result = new SyncRecord[length];
            for (var i = 0; i < length; i++)
            {
                var item = Script.Write<object>("items[i]");
                result[i] = new SyncRecord
                {
                    Service = service,
                    Id = Script.Write<string>("String(item.Id || item.SyncId || item.id || i)"),
                    EntityType = Script.Write<string>(
                        "String(item.EntityType || item.TableName || item.entity_type || 'Unknown')"
                    ),
                    Operation = Script.Write<string>(
                        "String(item.Operation !== undefined ? item.Operation : (item.Action !== undefined ? item.Action : '0'))"
                    ),
                    Timestamp = Script.Write<string>(
                        "String(item.Timestamp || item.CreatedAt || item.timestamp || '')"
                    ),
                    Payload = Script.Write<string>(
                        "JSON.stringify(item.Payload || item.Data || item).substring(0, 200)"
                    ),
                };
            }
            return result;
        }

        private static ReactElement RenderHeader() =>
            Div(
                className: "page-header mb-6",
                children: new[]
                {
                    H(2, className: "page-title", children: new[] { Text("Sync Dashboard") }),
                    P(
                        className: "page-description",
                        children: new[] { Text("Monitor and manage sync operations") }
                    ),
                }
            );

        private static ReactElement RenderServiceStatus(SyncState state) =>
            Div(
                className: "flex gap-4 mb-6",
                children: new[]
                {
                    Div(
                        className: "card flex-1",
                        dataTestId: "service-status-clinical",
                        children: new[]
                        {
                            H(4, children: new[] { Text("Clinical.Api") }),
                            P(children: new[] { Text(state.ClinicalStatus ?? "Unknown") }),
                        }
                    ),
                    Div(
                        className: "card flex-1",
                        dataTestId: "service-status-scheduling",
                        children: new[]
                        {
                            H(4, children: new[] { Text("Scheduling.Api") }),
                            P(children: new[] { Text(state.SchedulingStatus ?? "Unknown") }),
                        }
                    ),
                }
            );

        private static ReactElement RenderFilters(SyncState state, Action<SyncState> setState) =>
            Div(
                className: "card mb-4",
                children: new[]
                {
                    Div(
                        className: "flex gap-4 items-center",
                        children: new[]
                        {
                            Input(
                                className: "input",
                                placeholder: "Search records...",
                                value: state.SearchQuery ?? "",
                                dataTestId: "sync-search",
                                onChange: v => UpdateSearch(state, setState, v)
                            ),
                            Select(
                                className: "input",
                                value: state.ServiceFilter ?? "all",
                                dataTestId: "service-filter",
                                onChange: v => UpdateServiceFilter(state, setState, v),
                                children: new[]
                                {
                                    Option("all", "All Services"),
                                    Option("clinical", "Clinical"),
                                    Option("scheduling", "Scheduling"),
                                }
                            ),
                            Select(
                                className: "input",
                                value: state.ActionFilter ?? "all",
                                dataTestId: "action-filter",
                                onChange: v => UpdateActionFilter(state, setState, v),
                                children: new[]
                                {
                                    Option("all", "All Actions"),
                                    Option("0", "Insert"),
                                    Option("1", "Update"),
                                    Option("2", "Delete"),
                                }
                            ),
                        }
                    ),
                }
            );

        private static void UpdateSearch(
            SyncState state,
            Action<SyncState> setState,
            string value
        ) => setState(WithSearch(state, value));

        private static void UpdateServiceFilter(
            SyncState state,
            Action<SyncState> setState,
            string value
        ) => setState(WithServiceFilter(state, value));

        private static void UpdateActionFilter(
            SyncState state,
            Action<SyncState> setState,
            string value
        ) => setState(WithActionFilter(state, value));

        private static SyncState WithSearch(SyncState state, string value) =>
            new SyncState
            {
                Records = state.Records,
                Loading = state.Loading,
                ClinicalStatus = state.ClinicalStatus,
                SchedulingStatus = state.SchedulingStatus,
                ServiceFilter = state.ServiceFilter,
                ActionFilter = state.ActionFilter,
                SearchQuery = value,
            };

        private static SyncState WithServiceFilter(SyncState state, string value) =>
            new SyncState
            {
                Records = state.Records,
                Loading = state.Loading,
                ClinicalStatus = state.ClinicalStatus,
                SchedulingStatus = state.SchedulingStatus,
                ServiceFilter = value,
                ActionFilter = state.ActionFilter,
                SearchQuery = state.SearchQuery,
            };

        private static SyncState WithActionFilter(SyncState state, string value) =>
            new SyncState
            {
                Records = state.Records,
                Loading = state.Loading,
                ClinicalStatus = state.ClinicalStatus,
                SchedulingStatus = state.SchedulingStatus,
                ServiceFilter = state.ServiceFilter,
                ActionFilter = value,
                SearchQuery = state.SearchQuery,
            };

        private static ReactElement RenderRecordsTable(SyncState state)
        {
            var filtered = FilterRecords(state);
            return Div(
                className: "card",
                children: new[]
                {
                    H(3, className: "mb-4", children: new[] { Text("Sync Records") }),
                    RenderTable(filtered),
                }
            );
        }

        private static SyncRecord[] FilterRecords(SyncState state)
        {
            var records = state.Records ?? new SyncRecord[0];
            if (!string.IsNullOrEmpty(state.ServiceFilter) && state.ServiceFilter != "all")
            {
                records = records.Where(r => r.Service == state.ServiceFilter).ToArray();
            }
            if (!string.IsNullOrEmpty(state.ActionFilter) && state.ActionFilter != "all")
            {
                records = records.Where(r => r.Operation == state.ActionFilter).ToArray();
            }
            if (!string.IsNullOrEmpty(state.SearchQuery))
            {
                var q = state.SearchQuery;
                records = records
                    .Where(r =>
                        (r.Payload != null && r.Payload.Contains(q))
                        || (r.Id != null && r.Id.Contains(q))
                        || (r.EntityType != null && r.EntityType.Contains(q))
                    )
                    .ToArray();
            }
            return records;
        }

        private static ReactElement RenderTable(SyncRecord[] records)
        {
            var headerRow = Tr(
                children: new[]
                {
                    Th(children: new[] { Text("Service") }),
                    Th(children: new[] { Text("Entity") }),
                    Th(children: new[] { Text("Operation") }),
                    Th(children: new[] { Text("Timestamp") }),
                    Th(children: new[] { Text("Data") }),
                }
            );

            var bodyRows = records.Select(RenderRecordRow).ToArray();

            return Script.Call<ReactElement>(
                "React.createElement",
                "table",
                Script.Write<object>(
                    "{ className: 'data-table', 'data-testid': 'sync-records-table' }"
                ),
                Script.Call<ReactElement>("React.createElement", "thead", null, headerRow),
                Script.Call<ReactElement>(
                    "React.createElement",
                    "tbody",
                    null,
                    bodyRows.Length > 0
                        ? (object)bodyRows
                        : Tr(children: new[] { Td(children: new[] { Text("No records") }) })
                )
            );
        }

        private static ReactElement RenderRecordRow(SyncRecord record)
        {
            var props = Script.Write<object>(
                "{ 'data-service': record.Service, 'data-action': record.Operation }"
            );
            return Script.Call<ReactElement>(
                "React.createElement",
                "tr",
                props,
                Td(children: new[] { Text(record.Service) }),
                Td(children: new[] { Text(record.EntityType) }),
                Td(children: new[] { Text(record.Operation) }),
                Td(children: new[] { Text(record.Timestamp) }),
                Td(children: new[] { Text(record.Payload ?? "") })
            );
        }
    }
}

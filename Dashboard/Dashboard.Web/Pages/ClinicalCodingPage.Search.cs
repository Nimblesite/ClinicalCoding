using System;
using Dashboard.Components;
using Dashboard.React;
using static Dashboard.React.Elements;

namespace Dashboard.Pages
{
    /// <summary>Clinical coding — search console (tabs, input, options).</summary>
    public static partial class ClinicalCodingPage
    {
        private static ReactElement RenderSearchSection(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        ) =>
            Div(
                className: "coding-console",
                children: new[]
                {
                    RenderSearchTabs(state, setState),
                    RenderSearchInput(state, setState),
                    RenderSearchOptions(state, setState),
                }
            );

        private static ReactElement RenderSearchTabs(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        ) =>
            Div(
                className: "coding-tabs",
                children: new[]
                {
                    RenderTab(
                        "AI Search",
                        state.SearchMode == "semantic",
                        () => SetSearchMode(state, setState, "semantic")
                    ),
                    RenderTab(
                        "Keyword Search",
                        state.SearchMode == "keyword",
                        () => SetSearchMode(state, setState, "keyword")
                    ),
                    RenderTab(
                        "Code Lookup",
                        state.SearchMode == "lookup",
                        () => SetSearchMode(state, setState, "lookup")
                    ),
                }
            );

        private static ReactElement RenderTab(string label, bool isActive, Action onClick) =>
            Button(
                className: "coding-tab " + (isActive ? "active" : ""),
                onClick: onClick,
                children: new[] { Text(label) }
            );

        private static ReactElement RenderSearchInput(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        ) =>
            Div(
                className: "coding-search-field",
                children: new[]
                {
                    Span(className: "coding-search-icon", children: new[] { Icons.Search() }),
                    Input(
                        className: "coding-search-input",
                        type: "text",
                        placeholder: GetPlaceholder(state.SearchMode),
                        value: state.SearchQuery,
                        onChange: q => UpdateQuery(state, setState, q),
                        onKeyDown: k =>
                        {
                            if (k == "Enter")
                                ExecuteSearch(state, setState);
                        }
                    ),
                    Button(
                        className: "btn btn-primary coding-analyze-btn",
                        onClick: () => ExecuteSearch(state, setState),
                        children: new[] { Text(state.Loading ? "Searching..." : "Search") }
                    ),
                }
            );

        private static ReactElement RenderSearchOptions(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            if (state.SearchMode != "semantic")
                return Text("");

            return Div(
                className: "coding-options",
                children: new[]
                {
                    Label(
                        className: "coding-option-checkbox",
                        children: new[]
                        {
                            Input(
                                className: "checkbox",
                                type: "checkbox",
                                value: state.IncludeAchi ? "true" : "",
                                onChange: _ => ToggleAchi(state, setState)
                            ),
                            Span(children: new[] { Text("Include ACHI procedure codes") }),
                        }
                    ),
                    Div(className: "coding-options-divider"),
                    Div(
                        className: "coding-verified",
                        children: new[] { Icons.Sparkles(), Text(" Verified AI-Powered Engine") }
                    ),
                }
            );
        }

        private static string GetPlaceholder(string mode)
        {
            if (mode == "keyword")
                return "Search by code, description, or keywords (e.g. 'diabetes', 'fracture')";
            if (mode == "semantic")
                return "Describe symptoms or diagnosis in natural language...";
            return "Enter exact ICD-10 code or prefix (e.g. 'O9A.', 'E11', 'J18.9')";
        }
    }
}

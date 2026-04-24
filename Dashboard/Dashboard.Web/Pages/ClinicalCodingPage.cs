using System;
using Dashboard.Api;
using Dashboard.Components;
using Dashboard.Models;
using Dashboard.React;
using static Dashboard.React.Elements;
using static Dashboard.React.Hooks;

namespace Dashboard.Pages
{
    /// <summary>
    /// Clinical coding page state.
    /// </summary>
    public class ClinicalCodingState
    {
        /// <summary>Current search query.</summary>
        public string SearchQuery { get; set; }

        /// <summary>Active search mode (keyword, semantic, lookup).</summary>
        public string SearchMode { get; set; }

        /// <summary>ICD-10 search results.</summary>
        public Icd10Code[] Icd10Results { get; set; }

        /// <summary>ACHI search results.</summary>
        public AchiCode[] AchiResults { get; set; }

        /// <summary>Semantic search results.</summary>
        public SemanticSearchResult[] SemanticResults { get; set; }

        /// <summary>Selected code for detail view.</summary>
        public Icd10Code SelectedCode { get; set; }

        /// <summary>Whether loading.</summary>
        public bool Loading { get; set; }

        /// <summary>Error message if any.</summary>
        public string Error { get; set; }

        /// <summary>Whether to include ACHI in semantic search.</summary>
        public bool IncludeAchi { get; set; }

        /// <summary>Copied code for feedback.</summary>
        public string CopiedCode { get; set; }
    }

    /// <summary>
    /// Clinical coding page — Clinical Curator design.
    /// Implementation split across partial classes by concern.
    /// </summary>
    public static partial class ClinicalCodingPage
    {
        /// <summary>
        /// Renders the clinical coding page.
        /// </summary>
        public static ReactElement Render()
        {
            var stateResult = UseState(InitialState());
            var state = stateResult.State;
            var setState = stateResult.SetState;

            return Div(
                className: "page clinical-coding-page",
                children: new[]
                {
                    RenderHeader(),
                    RenderSearchSection(state, setState),
                    RenderContent(state, setState),
                }
            );
        }

        private static ClinicalCodingState InitialState() =>
            new ClinicalCodingState
            {
                SearchQuery = "",
                SearchMode = "semantic",
                Icd10Results = new Icd10Code[0],
                AchiResults = new AchiCode[0],
                SemanticResults = new SemanticSearchResult[0],
                SelectedCode = null,
                Loading = false,
                Error = null,
                IncludeAchi = false,
                CopiedCode = null,
            };

        private static ReactElement RenderHeader() =>
            Div(
                className: "coding-hero",
                children: new[]
                {
                    Span(
                        className: "coding-eyebrow",
                        children: new[] { Icons.Sparkles(), Text(" Clinical Intelligence") }
                    ),
                    H(
                        1,
                        className: "coding-hero-title",
                        children: new[] { Text("Diagnostic Coding Search") }
                    ),
                    P(
                        className: "coding-hero-subtitle",
                        children: new[]
                        {
                            Text(
                                "Harness natural language processing to map clinical documentation to ICD-10-AM and ACHI codes."
                            ),
                        }
                    ),
                }
            );

        private static ReactElement RenderContent(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            if (state.Loading)
                return RenderLoading();

            if (state.Error != null)
                return RenderError(state.Error);

            if (state.SelectedCode != null)
                return RenderCodeDetail(state, setState);

            if (state.SemanticResults.Length > 0)
                return RenderSemanticResults(state, setState);

            if (state.Icd10Results.Length > 0)
                return RenderKeywordResults(state, setState);

            if (state.SearchMode == "lookup" && !string.IsNullOrWhiteSpace(state.SearchQuery))
                return RenderNoResults(state.SearchQuery);

            return RenderEmptyState(state);
        }

        private static ReactElement RenderLoading() =>
            Div(
                className: "card",
                children: new[]
                {
                    Div(
                        className: "flex items-center justify-center p-12",
                        children: new[] { Div(className: "coding-loading-spinner") }
                    ),
                }
            );

        private static ReactElement RenderError(string error) =>
            Div(
                className: "card coding-alert coding-alert-error",
                children: new[]
                {
                    Div(
                        className: "flex items-center gap-3 p-4",
                        children: new[]
                        {
                            Icons.X(),
                            Div(
                                children: new[]
                                {
                                    H(
                                        4,
                                        className: "font-semibold",
                                        children: new[] { Text("Search Error") }
                                    ),
                                    P(
                                        className: "text-sm text-gray-600",
                                        children: new[] { Text(error) }
                                    ),
                                }
                            ),
                        }
                    ),
                }
            );
    }
}

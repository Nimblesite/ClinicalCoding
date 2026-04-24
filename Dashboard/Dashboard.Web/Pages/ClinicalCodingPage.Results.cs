using System;
using Dashboard.Api;
using Dashboard.Components;
using Dashboard.Models;
using Dashboard.React;
using static Dashboard.React.Elements;

namespace Dashboard.Pages
{
    /// <summary>Clinical coding — results lists, code rows, empty state.</summary>
    public static partial class ClinicalCodingPage
    {
        private static ReactElement RenderEmptyState(ClinicalCodingState state)
        {
            var title = GetEmptyTitle(state.SearchMode);
            var description = GetEmptyDescription(state.SearchMode);

            return Div(
                className: "card",
                children: new[]
                {
                    Div(
                        className: "empty-state",
                        children: new[]
                        {
                            Div(
                                className: "empty-state-icon coding-empty-icon",
                                children: new[] { Icons.Code() }
                            ),
                            H(4, className: "empty-state-title", children: new[] { Text(title) }),
                            P(
                                className: "empty-state-description",
                                children: new[] { Text(description) }
                            ),
                        }
                    ),
                }
            );
        }

        private static string GetEmptyTitle(string mode)
        {
            if (mode == "semantic")
                return "AI-Powered Code Search";
            if (mode == "lookup")
                return "Direct Code Lookup";
            return "ICD-10-AM Code Search";
        }

        private static string GetEmptyDescription(string mode)
        {
            if (mode == "semantic")
                return "Describe symptoms in natural language and let AI find the right codes.";
            if (mode == "lookup")
                return "Enter an ICD-10 code or prefix to find matching codes.";
            return "Search diagnosis codes by keyword, description, or code fragment.";
        }

        private static ReactElement RenderNoResults(string query) =>
            Div(
                className: "card",
                children: new[]
                {
                    Div(
                        className: "empty-state",
                        children: new[]
                        {
                            Div(
                                className: "empty-state-icon coding-empty-icon coding-empty-icon-muted",
                                children: new[] { Icons.Search() }
                            ),
                            H(
                                4,
                                className: "empty-state-title",
                                children: new[] { Text("No codes found") }
                            ),
                            P(
                                className: "empty-state-description",
                                children: new[]
                                {
                                    Text(
                                        "No ICD-10 codes match '"
                                            + query
                                            + "'. Try a different code or use keyword search."
                                    ),
                                }
                            ),
                        }
                    ),
                }
            );

        private static ReactElement RenderResultsHeader(int count, bool isAi) =>
            Div(
                className: "coding-results-header",
                children: new[]
                {
                    H(
                        2,
                        className: "coding-results-title",
                        children: new[]
                        {
                            Text(count + (isAi ? " AI-Matched Results" : " results found")),
                        }
                    ),
                }
            );

        private static ReactElement RenderKeywordResults(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            var rows = new ReactElement[state.Icd10Results.Length];
            for (int i = 0; i < state.Icd10Results.Length; i++)
                rows[i] = RenderKeywordCard(state.Icd10Results[i], state, setState);

            var children = new ReactElement[rows.Length + 1];
            children[0] = RenderResultsHeader(state.Icd10Results.Length, isAi: false);
            for (int i = 0; i < rows.Length; i++)
                children[i + 1] = rows[i];

            return Div(className: "coding-results", children: children);
        }

        private static ReactElement RenderKeywordCard(
            Icd10Code code,
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        ) =>
            Div(
                className: "coding-result-card",
                onClick: () => SelectCode(code, state, setState),
                children: new[]
                {
                    Div(
                        className: "coding-result-code",
                        children: new[]
                        {
                            Span(
                                className: "code-chip code-chip-icd",
                                children: new[] { Text(code.Code) }
                            ),
                            Span(
                                className: "code-type-label",
                                children: new[] { Text("ICD-10-AM") }
                            ),
                        }
                    ),
                    Div(
                        className: "coding-result-body",
                        children: new[]
                        {
                            H(
                                3,
                                className: "coding-result-title",
                                children: new[] { Text(code.ShortDescription ?? "") }
                            ),
                            P(
                                className: "coding-result-desc",
                                children: new[]
                                {
                                    Text(code.LongDescription ?? code.ShortDescription ?? ""),
                                }
                            ),
                        }
                    ),
                    Div(
                        className: "coding-result-meta",
                        children: new[]
                        {
                            code.Billable
                                ? Span(
                                    className: "badge badge-success",
                                    children: new[] { Text("Billable") }
                                )
                                : Span(
                                    className: "badge badge-gray",
                                    children: new[] { Text("Non-billable") }
                                ),
                        }
                    ),
                }
            );

        private static ReactElement RenderSemanticResults(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            var rows = new ReactElement[state.SemanticResults.Length];
            for (int i = 0; i < state.SemanticResults.Length; i++)
                rows[i] = RenderSemanticCard(state.SemanticResults[i], state, setState);

            var children = new ReactElement[rows.Length + 1];
            children[0] = RenderResultsHeader(state.SemanticResults.Length, isAi: true);
            for (int i = 0; i < rows.Length; i++)
                children[i + 1] = rows[i];

            return Div(className: "coding-results", children: children);
        }

        private static ReactElement RenderSemanticCard(
            SemanticSearchResult result,
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            var pct = (int)(result.Confidence * 100);
            var isAchi = result.CodeType == "ACHI";

            return Div(
                className: "coding-result-card",
                onClick: () => LookupSemanticCode(result.Code, state, setState),
                children: new[]
                {
                    Div(
                        className: "coding-result-code",
                        children: new[]
                        {
                            Span(
                                className: isAchi
                                    ? "code-chip code-chip-achi"
                                    : "code-chip code-chip-icd",
                                children: new[] { Text(result.Code) }
                            ),
                            Span(
                                className: "code-type-label",
                                children: new[] { Text(isAchi ? "ACHI" : "ICD-10-AM") }
                            ),
                        }
                    ),
                    Div(
                        className: "coding-result-body",
                        children: new[]
                        {
                            H(
                                3,
                                className: "coding-result-title",
                                children: new[] { Text(result.Description ?? "") }
                            ),
                            P(
                                className: "coding-result-desc",
                                children: new[]
                                {
                                    Text(result.LongDescription ?? result.Description ?? ""),
                                }
                            ),
                        }
                    ),
                    Div(
                        className: "coding-result-meta",
                        children: new[]
                        {
                            Div(
                                className: "ai-match",
                                children: new[]
                                {
                                    Span(
                                        className: "ai-match-label",
                                        children: new[] { Text("AI Match") }
                                    ),
                                    Span(
                                        className: "ai-match-value",
                                        children: new[] { Text(pct + "%") }
                                    ),
                                }
                            ),
                            Div(
                                className: "confidence-bar",
                                children: new[]
                                {
                                    Div(
                                        className: "confidence-bar-fill",
                                        style: new { width = pct + "%" }
                                    ),
                                }
                            ),
                        }
                    ),
                }
            );
        }
    }
}

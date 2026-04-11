using System;
using Dashboard.Api;
using Dashboard.Components;
using Dashboard.Models;
using Dashboard.React;
using static Dashboard.React.Elements;

namespace Dashboard.Pages
{
    /// <summary>Clinical coding — selected code detail view.</summary>
    public static partial class ClinicalCodingPage
    {
        private static ReactElement RenderCodeDetail(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            var code = state.SelectedCode;

            return Div(
                children: new[]
                {
                    Button(
                        className: "btn btn-ghost mb-4",
                        onClick: () => ClearSelection(state, setState),
                        children: new[] { Icons.ChevronLeft(), Text("Back to results") }
                    ),
                    Div(
                        className: "card coding-detail-card",
                        children: new[]
                        {
                            RenderDetailHeader(code, state, setState),
                            RenderDetailMeta(code),
                            RenderDetailSection("Full Description", code.LongDescription),
                            RenderDetailSection("Inclusion Terms", code.InclusionTerms),
                            RenderDetailSection("Exclusion Terms", code.ExclusionTerms),
                            RenderDetailSection("Code Also", code.CodeAlso),
                            RenderDetailSection("Code First", code.CodeFirst),
                        }
                    ),
                }
            );
        }

        private static ReactElement RenderDetailHeader(
            Icd10Code code,
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        ) =>
            Div(
                className: "flex items-start justify-between mb-6",
                children: new[]
                {
                    Div(
                        children: new[]
                        {
                            Div(
                                className: "flex items-center gap-3 mb-2",
                                children: new[]
                                {
                                    Span(
                                        className: "code-chip code-chip-icd code-chip-lg",
                                        children: new[] { Text(code.Code) }
                                    ),
                                    code.Billable
                                        ? Span(
                                            className: "badge badge-success",
                                            children: new[] { Icons.Check(), Text("Billable") }
                                        )
                                        : Span(
                                            className: "badge badge-gray",
                                            children: new[] { Text("Non-billable") }
                                        ),
                                }
                            ),
                            H(
                                2,
                                className: "text-xl font-semibold mt-4",
                                children: new[] { Text(code.ShortDescription ?? "") }
                            ),
                        }
                    ),
                    Button(
                        className: "btn btn-primary",
                        onClick: () => CopyCode(code.Code, state, setState),
                        children: new[]
                        {
                            state.CopiedCode == code.Code ? Icons.Check() : Icons.Copy(),
                            Text(state.CopiedCode == code.Code ? "Copied!" : "Copy Code"),
                        }
                    ),
                }
            );

        private static ReactElement RenderDetailMeta(Icd10Code code) =>
            Div(
                className: "coding-detail-meta",
                children: new[]
                {
                    RenderDetailItem(
                        "Chapter",
                        code.ChapterNumber + " - " + (code.ChapterTitle ?? "")
                    ),
                    RenderDetailItem("Block", code.BlockCode ?? ""),
                    RenderDetailItem("Category", code.CategoryCode ?? ""),
                }
            );

        private static ReactElement RenderDetailItem(string label, string value) =>
            Div(
                children: new[]
                {
                    Span(
                        className: "coding-detail-section-title",
                        children: new[] { Text(label) }
                    ),
                    P(className: "font-medium", children: new[] { Text(value) }),
                }
            );

        private static ReactElement RenderDetailSection(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return Text("");

            return Div(
                className: "coding-detail-section",
                children: new[]
                {
                    H(
                        4,
                        className: "coding-detail-section-title",
                        children: new[] { Text(title) }
                    ),
                    Div(
                        className: "coding-detail-section-body",
                        children: new[] { P(children: new[] { Text(content) }) }
                    ),
                }
            );
        }
    }
}

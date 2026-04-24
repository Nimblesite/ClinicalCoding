using System;
using System.Collections.Generic;
using Dashboard.Api;
using Dashboard.Models;

namespace Dashboard.Pages
{
    /// <summary>Clinical coding — state mutations and async actions.</summary>
    public static partial class ClinicalCodingPage
    {
        private static ClinicalCodingState Clone(ClinicalCodingState s) =>
            new ClinicalCodingState
            {
                SearchQuery = s.SearchQuery,
                SearchMode = s.SearchMode,
                Icd10Results = s.Icd10Results,
                AchiResults = s.AchiResults,
                SemanticResults = s.SemanticResults,
                SelectedCode = s.SelectedCode,
                Loading = s.Loading,
                Error = s.Error,
                IncludeAchi = s.IncludeAchi,
                CopiedCode = s.CopiedCode,
            };

        private static void SetSearchMode(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState,
            string mode
        )
        {
            var next = Clone(state);
            next.SearchMode = mode;
            next.Icd10Results = new Icd10Code[0];
            next.AchiResults = new AchiCode[0];
            next.SemanticResults = new SemanticSearchResult[0];
            next.SelectedCode = null;
            next.Loading = false;
            next.Error = null;
            next.CopiedCode = null;
            setState(next);
        }

        private static void UpdateQuery(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState,
            string query
        )
        {
            var next = Clone(state);
            next.SearchQuery = query;
            setState(next);
        }

        private static void ToggleAchi(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            var next = Clone(state);
            next.IncludeAchi = !state.IncludeAchi;
            setState(next);
        }

        private static void CopyCode(
            string code,
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            H5.Script.Call<object>("navigator.clipboard.writeText", code);
            var next = Clone(state);
            next.CopiedCode = code;
            setState(next);
        }

        private static void ClearSelection(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            var next = Clone(state);
            next.SelectedCode = null;
            next.Loading = false;
            next.Error = null;
            next.CopiedCode = null;
            setState(next);
        }

        private static async void ExecuteSearch(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            if (string.IsNullOrWhiteSpace(state.SearchQuery))
                return;

            var loading = Clone(state);
            loading.Loading = true;
            loading.Error = null;
            loading.Icd10Results = new Icd10Code[0];
            loading.SemanticResults = new SemanticSearchResult[0];
            loading.SelectedCode = null;
            loading.CopiedCode = null;
            setState(loading);

            try
            {
                if (state.SearchMode == "keyword")
                    await DoKeywordSearch(state, setState);
                else if (state.SearchMode == "semantic")
                    await DoSemanticSearch(state, setState);
                else
                    await DoLookup(state, setState);
            }
            catch (Exception ex)
            {
                var err = Clone(state);
                err.Loading = false;
                err.Error = ex.Message;
                setState(err);
            }
        }

        private static async System.Threading.Tasks.Task DoKeywordSearch(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            var results = await ApiClient.SearchIcd10CodesAsync(
                query: state.SearchQuery,
                limit: 50
            );
            var next = Clone(state);
            next.Icd10Results = results;
            next.Loading = false;
            setState(next);
        }

        private static async System.Threading.Tasks.Task DoSemanticSearch(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            var results = await ApiClient.SemanticSearchAsync(
                query: state.SearchQuery,
                limit: 20,
                includeAchi: state.IncludeAchi
            );
            var next = Clone(state);
            next.SemanticResults = results;
            next.Loading = false;
            setState(next);
        }

        private static async System.Threading.Tasks.Task DoLookup(
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            var allResults = await ApiClient.SearchIcd10CodesAsync(
                query: state.SearchQuery,
                limit: 100
            );
            var query = state.SearchQuery.ToUpper();
            var matches = new List<Icd10Code>();
            foreach (var c in allResults)
            {
                if (c.Code != null && c.Code.ToUpper().StartsWith(query))
                    matches.Add(c);
            }

            var next = Clone(state);
            next.Loading = false;
            if (matches.Count == 1)
            {
                var full = await ApiClient.GetIcd10CodeAsync(code: matches[0].Code);
                next.SelectedCode = full;
            }
            else
            {
                next.Icd10Results = matches.ToArray();
            }
            setState(next);
        }

        private static async void SelectCode(
            Icd10Code code,
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            var loading = Clone(state);
            loading.Loading = true;
            loading.SelectedCode = null;
            setState(loading);

            try
            {
                var fullCode = await ApiClient.GetIcd10CodeAsync(code: code.Code);
                var next = Clone(state);
                next.SelectedCode = fullCode;
                next.Loading = false;
                setState(next);
            }
            catch (Exception ex)
            {
                var err = Clone(state);
                err.Loading = false;
                err.Error = "Failed to load code details: " + ex.Message;
                setState(err);
            }
        }

        private static async void LookupSemanticCode(
            string code,
            ClinicalCodingState state,
            Action<ClinicalCodingState> setState
        )
        {
            var loading = Clone(state);
            loading.SearchQuery = code;
            loading.SearchMode = "lookup";
            loading.Loading = true;
            loading.Icd10Results = new Icd10Code[0];
            loading.SemanticResults = new SemanticSearchResult[0];
            loading.SelectedCode = null;
            loading.CopiedCode = null;
            setState(loading);

            try
            {
                var result = await ApiClient.GetIcd10CodeAsync(code: code);
                var next = Clone(loading);
                next.SelectedCode = result;
                next.Loading = false;
                setState(next);
            }
            catch (Exception ex)
            {
                var err = Clone(loading);
                err.Loading = false;
                err.Error = ex.Message;
                setState(err);
            }
        }
    }
}

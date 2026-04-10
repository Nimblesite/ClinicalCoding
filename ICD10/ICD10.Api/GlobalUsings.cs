global using System;
global using System.Collections.Immutable;
global using Generated;
global using Microsoft.Extensions.Logging;
global using Nimblesite.Sql.Model;
global using Npgsql;
global using Outcome;
global using GetAchiBlocksError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiBlocks>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiBlocks>,
    Nimblesite.Sql.Model.SqlError
>;
// GetAchiBlocks query result type aliases
global using GetAchiBlocksOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiBlocks>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiBlocks>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetAchiCodeByCodeError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiCodeByCode>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiCodeByCode>,
    Nimblesite.Sql.Model.SqlError
>;
// GetAchiCodeByCode query result type aliases
global using GetAchiCodeByCodeOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiCodeByCode>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiCodeByCode>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetAchiCodesByBlockError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiCodesByBlock>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiCodesByBlock>,
    Nimblesite.Sql.Model.SqlError
>;
// GetAchiCodesByBlock query result type aliases
global using GetAchiCodesByBlockOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiCodesByBlock>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetAchiCodesByBlock>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetBlocksByChapterError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetBlocksByChapter>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetBlocksByChapter>,
    Nimblesite.Sql.Model.SqlError
>;
// GetBlocksByChapter query result type aliases
global using GetBlocksByChapterOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetBlocksByChapter>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetBlocksByChapter>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetCategoriesByBlockError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetCategoriesByBlock>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetCategoriesByBlock>,
    Nimblesite.Sql.Model.SqlError
>;
// GetCategoriesByBlock query result type aliases
global using GetCategoriesByBlockOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetCategoriesByBlock>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetCategoriesByBlock>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetChaptersError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetChapters>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetChapters>,
    Nimblesite.Sql.Model.SqlError
>;
// GetChapters query result type aliases
global using GetChaptersOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetChapters>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetChapters>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetCodeByCodeError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetCodeByCode>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetCodeByCode>,
    Nimblesite.Sql.Model.SqlError
>;
// GetCodeByCode query result type aliases
global using GetCodeByCodeOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetCodeByCode>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetCodeByCode>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetCodesByCategoryError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetCodesByCategory>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetCodesByCategory>,
    Nimblesite.Sql.Model.SqlError
>;
// GetCodesByCategory query result type aliases
global using GetCodesByCategoryOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetCodesByCategory>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetCodesByCategory>,
    Nimblesite.Sql.Model.SqlError
>;
global using SearchAchiCodesError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.SearchAchiCodes>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.SearchAchiCodes>,
    Nimblesite.Sql.Model.SqlError
>;
// SearchAchiCodes query result type aliases
global using SearchAchiCodesOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.SearchAchiCodes>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.SearchAchiCodes>,
    Nimblesite.Sql.Model.SqlError
>;
global using SearchIcd10CodesError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.SearchIcd10Codes>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.SearchIcd10Codes>,
    Nimblesite.Sql.Model.SqlError
>;
// SearchIcd10Codes query result type aliases
global using SearchIcd10CodesOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.SearchIcd10Codes>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.SearchIcd10Codes>,
    Nimblesite.Sql.Model.SqlError
>;

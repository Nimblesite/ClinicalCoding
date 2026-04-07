global using System;
global using Generated;
global using Microsoft.Extensions.Logging;
global using Nimblesite.Sql.Model;
global using Nimblesite.Sync.Core;
global using Nimblesite.Sync.Postgres;
global using Npgsql;
global using Outcome;
global using GetConditionsError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetConditionsByPatient>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetConditionsByPatient>,
    Nimblesite.Sql.Model.SqlError
>;
// GetConditionsByPatient query result type aliases
global using GetConditionsOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetConditionsByPatient>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetConditionsByPatient>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetEncountersError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetEncountersByPatient>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetEncountersByPatient>,
    Nimblesite.Sql.Model.SqlError
>;
// GetEncountersByPatient query result type aliases
global using GetEncountersOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetEncountersByPatient>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetEncountersByPatient>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetMedicationsError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetMedicationsByPatient>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetMedicationsByPatient>,
    Nimblesite.Sql.Model.SqlError
>;
// GetMedicationsByPatient query result type aliases
global using GetMedicationsOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetMedicationsByPatient>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetMedicationsByPatient>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetPatientByIdError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetPatientById>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetPatientById>,
    Nimblesite.Sql.Model.SqlError
>;
// GetPatientById query result type aliases
global using GetPatientByIdOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetPatientById>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetPatientById>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetPatientsError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetPatients>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetPatients>,
    Nimblesite.Sql.Model.SqlError
>;
// GetPatients query result type aliases
global using GetPatientsOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetPatients>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetPatients>,
    Nimblesite.Sql.Model.SqlError
>;
global using InsertError = Outcome.Result<int, Nimblesite.Sql.Model.SqlError>.Error<
    int,
    Nimblesite.Sql.Model.SqlError
>;
// Insert result type aliases
global using InsertOk = Outcome.Result<int, Nimblesite.Sql.Model.SqlError>.Ok<
    int,
    Nimblesite.Sql.Model.SqlError
>;
global using SearchPatientsError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.SearchPatients>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.SearchPatients>,
    Nimblesite.Sql.Model.SqlError
>;
// SearchPatients query result type aliases
global using SearchPatientsOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.SearchPatients>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.SearchPatients>,
    Nimblesite.Sql.Model.SqlError
>;
// Sync result type aliases
global using StringSyncError = Outcome.Result<string, Nimblesite.Sync.Core.SyncError>.Error<
    string,
    Nimblesite.Sync.Core.SyncError
>;
global using StringSyncOk = Outcome.Result<string, Nimblesite.Sync.Core.SyncError>.Ok<
    string,
    Nimblesite.Sync.Core.SyncError
>;
global using SyncLogListError = Outcome.Result<
    System.Collections.Generic.IReadOnlyList<Nimblesite.Sync.Core.SyncLogEntry>,
    Nimblesite.Sync.Core.SyncError
>.Error<
    System.Collections.Generic.IReadOnlyList<Nimblesite.Sync.Core.SyncLogEntry>,
    Nimblesite.Sync.Core.SyncError
>;
global using SyncLogListOk = Outcome.Result<
    System.Collections.Generic.IReadOnlyList<Nimblesite.Sync.Core.SyncLogEntry>,
    Nimblesite.Sync.Core.SyncError
>.Ok<
    System.Collections.Generic.IReadOnlyList<Nimblesite.Sync.Core.SyncLogEntry>,
    Nimblesite.Sync.Core.SyncError
>;
// Update result type aliases
global using UpdateOk = Outcome.Result<int, Nimblesite.Sql.Model.SqlError>.Ok<
    int,
    Nimblesite.Sql.Model.SqlError
>;

global using System;
global using Generated;
global using Microsoft.Extensions.Logging;
global using Nimblesite.Sql.Model;
global using Nimblesite.Sync.Core;
global using Nimblesite.Sync.Postgres;
global using Npgsql;
global using Outcome;
// Sync result type aliases
global using BoolSyncError = Outcome.Result<bool, Nimblesite.Sync.Core.SyncError>.Error<
    bool,
    Nimblesite.Sync.Core.SyncError
>;
global using GetAllPractitionersError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAllPractitioners>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetAllPractitioners>,
    Nimblesite.Sql.Model.SqlError
>;
// GetAllPractitioners query result type aliases
global using GetAllPractitionersOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAllPractitioners>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetAllPractitioners>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetAppointmentByIdError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentById>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentById>,
    Nimblesite.Sql.Model.SqlError
>;
// GetAppointmentById query result type aliases
global using GetAppointmentByIdOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentById>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentById>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetAppointmentsByPatientError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentsByPatient>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentsByPatient>,
    Nimblesite.Sql.Model.SqlError
>;
// GetAppointmentsByPatient query result type aliases
global using GetAppointmentsByPatientOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentsByPatient>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentsByPatient>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetAppointmentsByPractitionerError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentsByPractitioner>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentsByPractitioner>,
    Nimblesite.Sql.Model.SqlError
>;
// GetAppointmentsByPractitioner query result type aliases
global using GetAppointmentsByPractitionerOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentsByPractitioner>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetAppointmentsByPractitioner>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetPractitionerByIdError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetPractitionerById>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetPractitionerById>,
    Nimblesite.Sql.Model.SqlError
>;
// GetPractitionerById query result type aliases
global using GetPractitionerByIdOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetPractitionerById>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetPractitionerById>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetUpcomingAppointmentsError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetUpcomingAppointments>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetUpcomingAppointments>,
    Nimblesite.Sql.Model.SqlError
>;
// GetUpcomingAppointments query result type aliases
global using GetUpcomingAppointmentsOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetUpcomingAppointments>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetUpcomingAppointments>,
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
global using SearchPractitionersError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.SearchPractitionersBySpecialty>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.SearchPractitionersBySpecialty>,
    Nimblesite.Sql.Model.SqlError
>;
// SearchPractitionersBySpecialty query result type aliases
global using SearchPractitionersOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.SearchPractitionersBySpecialty>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.SearchPractitionersBySpecialty>,
    Nimblesite.Sql.Model.SqlError
>;
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

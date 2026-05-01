#pragma warning disable IDE0005 // Using directive is unnecessary (some are unused but needed for tests)

global using System;
global using System.Collections.Generic;
global using System.Data;
global using System.Globalization;
global using System.Text;
global using System.Linq;
global using System.Text.Json;
global using Fido2NetLib;
global using Fido2NetLib.Objects;
global using Gatekeeper.Api;
global using Generated;
global using Microsoft.Extensions.Logging;
global using Nimblesite.Sql.Model;
global using Npgsql;
global using Outcome;
global using CheckResourceGrantOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.CheckResourceGrant>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.CheckResourceGrant>,
    Nimblesite.Sql.Model.SqlError
>;
// Insert result type alias
global using GetChallengeByIdOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetChallengeById>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetChallengeById>,
    Nimblesite.Sql.Model.SqlError
>;
// Additional query result type aliases
global using GetCredentialByIdOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetCredentialById>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetCredentialById>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetSessionRevokedError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetSessionRevoked>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetSessionRevoked>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetSessionRevokedOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetSessionRevoked>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetSessionRevoked>,
    Nimblesite.Sql.Model.SqlError
>;
// Query result type aliases
global using GetUserByEmailOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetUserByEmail>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetUserByEmail>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetUserByIdOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetUserById>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetUserById>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetUserCredentialsError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetUserCredentials>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetUserCredentials>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetUserCredentialsOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetUserCredentials>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetUserCredentials>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetUserPermissionsOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetUserPermissions>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetUserPermissions>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetUserRolesOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetUserRoles>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetUserRoles>,
    Nimblesite.Sql.Model.SqlError
>;

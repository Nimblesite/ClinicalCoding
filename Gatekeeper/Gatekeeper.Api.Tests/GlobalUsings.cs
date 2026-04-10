#pragma warning disable IDE0005 // Using directive is unnecessary

global using System.Net;
global using System.Net.Http.Json;
global using System.Text.Json;
global using Generated;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Nimblesite.Sql.Model;
global using Xunit;
global using GetPermissionByCodeError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetPermissionByCode>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetPermissionByCode>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetPermissionByCodeOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetPermissionByCode>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetPermissionByCode>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetRolePermissionsError = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetRolePermissions>,
    Nimblesite.Sql.Model.SqlError
>.Error<
    System.Collections.Immutable.ImmutableList<Generated.GetRolePermissions>,
    Nimblesite.Sql.Model.SqlError
>;
global using GetRolePermissionsOk = Outcome.Result<
    System.Collections.Immutable.ImmutableList<Generated.GetRolePermissions>,
    Nimblesite.Sql.Model.SqlError
>.Ok<
    System.Collections.Immutable.ImmutableList<Generated.GetRolePermissions>,
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

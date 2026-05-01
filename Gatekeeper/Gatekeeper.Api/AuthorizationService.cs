namespace Gatekeeper.Api;

/// <summary>
/// Evaluates authorization decisions against RBAC grants and resource-level grants.
/// </summary>
public static class AuthorizationService
{
    /// <summary>
    /// Checks if a user has a specific permission, optionally scoped to a resource.
    /// </summary>
    public static async Task<(bool Allowed, string Reason)> CheckPermissionAsync(
        IDbConnection conn,
        string userId,
        string permissionCode,
        string? resourceType,
        string? resourceId,
        string now
    )
    {
        var npgsql = conn as NpgsqlConnection
            ?? throw new InvalidOperationException("AuthorizationService requires NpgsqlConnection at the generated layer.");

        if (!string.IsNullOrEmpty(resourceType) && !string.IsNullOrEmpty(resourceId))
        {
            var grantResult = await npgsql
                .CheckResourceGrantAsync(userId, resourceType, resourceId, permissionCode, now)
                .ConfigureAwait(false);
            if (grantResult is CheckResourceGrantOk grantOk && grantOk.Value.Count > 0)
                return (true, $"resource-grant:{resourceType}/{resourceId}");
        }

        var permResult = await npgsql.GetUserPermissionsAsync(userId, now).ConfigureAwait(false);
        var permissions = permResult is GetUserPermissionsOk ok ? ok.Value : [];

        foreach (var perm in permissions)
        {
            if (perm.code is null)
                continue;
            if (!PermissionMatches(perm.code, permissionCode))
                continue;

            var scopeType = ToStringValue(perm.scope_type);
            var scopeValue = ToStringValue(perm.scope_value);
            var scopeMatches = scopeType switch
            {
                null or "" or "all" => true,
                "record" => scopeValue == resourceId,
                _ => false,
            };

            if (!scopeMatches)
                continue;

            var source = perm.source_name != perm.code ? $"role:{perm.source_name}" : "direct-grant";
            return (true, $"{source} grants {perm.code}");
        }

        return (false, "no matching permission");
    }

    private static string? ToStringValue(object? value) =>
        value switch
        {
            null => null,
            string s => s,
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            _ => value.ToString(),
        };

    private static bool PermissionMatches(string granted, string target)
    {
        if (granted == target)
            return true;
        if (granted.EndsWith(":*", StringComparison.Ordinal))
            return target.StartsWith(granted[..^1], StringComparison.Ordinal);
        if (granted is "*:*" or "*")
            return true;
        return false;
    }
}

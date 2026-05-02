-- name: GetUserById
SELECT id, display_name, email, created_at, last_login_at, is_active, token_version, failed_login_count, locked_until, metadata
FROM gk_user
WHERE id = @id

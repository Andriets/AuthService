-- Seed script for AuthDB
-- Safe to run multiple times: inserts are skipped if the row already exists.

-- Default Tenant
INSERT INTO tenants (id, name, created_at, updated_at)
VALUES ('00000000-0000-0000-0000-000000000001', 'Default', NOW(), NOW())
ON CONFLICT (id) DO NOTHING;

-- Admin User
INSERT INTO users (id, email, first_name, last_name, password_hash, is_active, created_at, updated_at)
VALUES ('00000000-0000-0000-0000-000000000002', 'admin@default.local', 'Admin', NULL, NULL, TRUE, NOW(), NULL)
ON CONFLICT (id) DO NOTHING;

-- Associate admin user with the default tenant
INSERT INTO tenant_users (tenant_id, user_id, created_at)
VALUES ('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000002', NOW())
ON CONFLICT (tenant_id, user_id) DO NOTHING;

-- This file will be executed automatically when the PostgreSQL container starts for the first time
-- The database 'dashboarddb' is already created by the POSTGRES_DB environment variable

-- Connect to the dashboard database
\c dashboarddb;

-- Display connection info
SELECT 'Database initialization complete!' as status;

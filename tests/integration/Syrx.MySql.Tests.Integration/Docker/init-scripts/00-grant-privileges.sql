-- Grant additional privileges to syrx_user for integration testing
-- This script runs before other init scripts to ensure proper permissions

-- Grant comprehensive privileges to syrx_user on the syrx database
GRANT ALL PRIVILEGES ON syrx.* TO 'syrx_user'@'%';

-- Grant additional privileges that might be needed for integration tests
-- These are needed for procedure creation/dropping and other operations
GRANT CREATE ROUTINE ON syrx.* TO 'syrx_user'@'%';
GRANT ALTER ROUTINE ON syrx.* TO 'syrx_user'@'%';
GRANT DROP ON syrx.* TO 'syrx_user'@'%';
GRANT EXECUTE ON syrx.* TO 'syrx_user'@'%';
-- Grant SYSTEM_USER privilege needed for dropping procedures in MySQL 8.0
GRANT SYSTEM_USER ON *.* TO 'syrx_user'@'%';

-- Flush privileges to ensure changes take effect
FLUSH PRIVILEGES;

SELECT 'Additional privileges granted to syrx_user.' as message;
-- Create tables for Syrx MySQL integration tests

-- Create the main poco table used in most tests
CREATE TABLE IF NOT EXISTS `poco` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(50) NOT NULL,
    `Value` DECIMAL(18, 2) NOT NULL,
    `Modified` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) DEFAULT CHARSET=utf8;

-- Create identity_test table for identity testing
CREATE TABLE IF NOT EXISTS `identity_test` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(50) NOT NULL,
    `Value` DECIMAL(18, 2) NOT NULL,
    `Modified` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) DEFAULT CHARSET=utf8;

-- Create bulk_insert table for bulk operations
CREATE TABLE IF NOT EXISTS `bulk_insert` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(50) NOT NULL,
    `Value` DECIMAL(18, 2) NOT NULL,
    `Modified` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) DEFAULT CHARSET=utf8;

-- Create distributed_transaction table for distributed transaction tests
CREATE TABLE IF NOT EXISTS `distributed_transaction` (
    `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(50) NOT NULL,
    `Value` DECIMAL(18, 2) NOT NULL,
    `Modified` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) DEFAULT CHARSET=utf8;

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_poco_name ON `poco`(`Name`);
CREATE INDEX IF NOT EXISTS idx_identity_test_name ON `identity_test`(`Name`);
CREATE INDEX IF NOT EXISTS idx_bulk_insert_name ON `bulk_insert`(`Name`);
CREATE INDEX IF NOT EXISTS idx_distributed_transaction_name ON `distributed_transaction`(`Name`);

SELECT 'All test tables created successfully.' as message;
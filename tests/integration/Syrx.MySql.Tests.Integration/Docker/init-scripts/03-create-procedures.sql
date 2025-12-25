-- Create stored procedures for Syrx MySQL integration tests

-- Drop existing procedures if they exist
DROP PROCEDURE IF EXISTS usp_create_table;
DROP PROCEDURE IF EXISTS usp_identity_tester;
DROP PROCEDURE IF EXISTS usp_bulk_insert;
DROP PROCEDURE IF EXISTS usp_bulk_insert_and_return;
DROP PROCEDURE IF EXISTS usp_clear_table;

-- Create table creation procedure
DELIMITER //
CREATE PROCEDURE `usp_create_table` (IN name VARCHAR(255))
BEGIN
    SET @drop_template = CONCAT("DROP TABLE IF EXISTS `", name, "`;");
    PREPARE drop_stmt FROM @drop_template;
    EXECUTE drop_stmt;
    DEALLOCATE PREPARE drop_stmt;

    SET @create_template = CONCAT("CREATE TABLE IF NOT EXISTS `", name, "` (
        `Id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
        `Name` VARCHAR(50) NOT NULL,
        `Value` DECIMAL(18, 2) NOT NULL,
        `Modified` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    ) DEFAULT CHARSET=utf8;");
    PREPARE create_stmt FROM @create_template;
    EXECUTE create_stmt;
    DEALLOCATE PREPARE create_stmt;
END //
DELIMITER ;

-- Create identity tester procedure
DELIMITER //
CREATE PROCEDURE usp_identity_tester(
    IN p_name VARCHAR(50),
    IN p_value DECIMAL(18, 2)
)
BEGIN
    INSERT INTO identity_test (name, value, modified)
    VALUES (p_name, p_value, UTC_TIMESTAMP());

    SELECT LAST_INSERT_ID() as Id;
END //
DELIMITER ;

-- Create bulk insert procedure
DELIMITER //
CREATE PROCEDURE usp_bulk_insert(IN p_path VARCHAR(255))
BEGIN
    SET SESSION sql_mode = 'NO_AUTO_VALUE_ON_ZERO';
    SET @command = CONCAT('LOAD DATA INFILE ''', p_path, ''' INTO TABLE bulk_insert FIELDS TERMINATED BY '','' LINES TERMINATED BY ''\\n'';');
    PREPARE stmt FROM @command;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
END //
DELIMITER ;

-- Create bulk insert and return procedure
DELIMITER //
CREATE PROCEDURE usp_bulk_insert_and_return(IN p_path VARCHAR(255))
BEGIN
    SET SESSION sql_mode = 'NO_AUTO_VALUE_ON_ZERO';
    SET @command = CONCAT('LOAD DATA INFILE ''', p_path, ''' INTO TABLE bulk_insert FIELDS TERMINATED BY '','' LINES TERMINATED BY ''\\n'';');
    PREPARE stmt FROM @command;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

    SELECT * FROM bulk_insert;
END //
DELIMITER ;

-- Create table clearing procedure
DELIMITER //
CREATE PROCEDURE usp_clear_table(IN name VARCHAR(255))
BEGIN
    SET @sql = CONCAT('TRUNCATE TABLE ', name);
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
END //
DELIMITER ;

SELECT 'All stored procedures created successfully.' as message;
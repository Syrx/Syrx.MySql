-- Seed test data for Syrx MySQL integration tests

-- Clear existing data
TRUNCATE TABLE `poco`;
TRUNCATE TABLE `identity_test`;
TRUNCATE TABLE `bulk_insert`;
TRUNCATE TABLE `distributed_transaction`;

-- Insert test data into poco table (enough records for multi-mapping tests)
INSERT INTO `poco` (`Name`, `Value`, `Modified`) VALUES
    ('Test Record 1', 100.50, DATE_SUB(NOW(), INTERVAL 1 DAY)),
    ('Test Record 2', 200.75, DATE_SUB(NOW(), INTERVAL 2 HOUR)),
    ('Test Record 3', 300.25, DATE_SUB(NOW(), INTERVAL 30 MINUTE)),
    ('Test Record 4', 400.00, DATE_SUB(NOW(), INTERVAL 15 MINUTE)),
    ('Test Record 5', 500.99, DATE_SUB(NOW(), INTERVAL 5 MINUTE)),
    ('Test Record 6', 600.33, DATE_SUB(NOW(), INTERVAL 3 MINUTE)),
    ('Test Record 7', 700.66, DATE_SUB(NOW(), INTERVAL 2 MINUTE)),
    ('Test Record 8', 800.88, DATE_SUB(NOW(), INTERVAL 1 MINUTE)),
    ('Test Record 9', 900.11, DATE_SUB(NOW(), INTERVAL 30 SECOND)),
    ('Test Record 10', 1000.00, DATE_SUB(NOW(), INTERVAL 10 SECOND)),
    ('Test Record 11', 1100.22, DATE_SUB(NOW(), INTERVAL 5 SECOND)),
    ('Test Record 12', 1200.44, DATE_SUB(NOW(), INTERVAL 3 SECOND)),
    ('Test Record 13', 1300.55, DATE_SUB(NOW(), INTERVAL 2 SECOND)),
    ('Test Record 14', 1400.77, DATE_SUB(NOW(), INTERVAL 1 SECOND)),
    ('Test Record 15', 1500.88, NOW()),
    ('Test Record 16', 1600.99, NOW()),
    ('Test Record 17', 1700.11, NOW()),
    ('Test Record 18', 1800.22, NOW()),
    ('Test Record 19', 1900.33, NOW()),
    ('Test Record 20', 2000.44, NOW());

-- Insert a few test records for identity testing
INSERT INTO `identity_test` (`Name`, `Value`, `Modified`) VALUES
    ('Identity Test 1', 50.25, NOW()),
    ('Identity Test 2', 75.50, NOW());

-- Verify data was inserted
SELECT CONCAT('Inserted ', COUNT(*), ' test records into poco table.') as message FROM `poco`;

SELECT 'Database seeding completed successfully.' as message;
SELECT 'Syrx MySQL test database is ready for integration tests.' as message;
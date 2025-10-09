-- Data cleanup script to prepare for schema migration
-- Run this to clean up existing data before applying the new schema

-- First, let's see what we have
SELECT 'Users Count: ' || COUNT(*) FROM "Users";
SELECT 'Offers Count: ' || COUNT(*) FROM "Offers";
SELECT 'Bookings Count: ' || COUNT(*) FROM "Bookings";

-- Check for orphaned offers (offers without valid users)
SELECT 'Orphaned Offers: ' || COUNT(*) 
FROM "Offers" o 
LEFT JOIN "Users" u ON o."CreatedBy" = u."Id" 
WHERE u."Id" IS NULL;

-- Check for orphaned bookings (bookings without valid users or offers)
SELECT 'Orphaned Bookings (User): ' || COUNT(*) 
FROM "Bookings" b 
LEFT JOIN "Users" u ON b."UserId" = u."Id" 
WHERE u."Id" IS NULL;

SELECT 'Orphaned Bookings (Offer): ' || COUNT(*) 
FROM "Bookings" b 
LEFT JOIN "Offers" o ON b."OfferId" = o."Id" 
WHERE o."Id" IS NULL;

-- Clean up orphaned data
DELETE FROM "Bookings" 
WHERE "UserId" NOT IN (SELECT "Id" FROM "Users");

DELETE FROM "Bookings" 
WHERE "OfferId" NOT IN (SELECT "Id" FROM "Offers");

DELETE FROM "Offers" 
WHERE "CreatedBy" NOT IN (SELECT "Id" FROM "Users");

-- Set default values for required new fields
-- (This will be handled in the migration, but good to prepare)

-- Report final counts
SELECT 'Final Users Count: ' || COUNT(*) FROM "Users";
SELECT 'Final Offers Count: ' || COUNT(*) FROM "Offers";
SELECT 'Final Bookings Count: ' || COUNT(*) FROM "Bookings";
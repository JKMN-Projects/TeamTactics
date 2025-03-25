DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'team_tactics'
        AND table_name = 'user_account'
        AND column_name IN ('first_name', 'last_name')
    ) THEN
        ALTER TABLE team_tactics.user_account
        DROP COLUMN first_name, 
        DROP COLUMN last_name;
        
        RAISE NOTICE 'Columns first_name and last_name dropped successfully';
    ELSE
        RAISE NOTICE 'One or both columns do not exist in the table';
    END IF;
END $$;
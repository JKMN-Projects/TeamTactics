ALTER TABLE team_tactics.user_tournament 
        ADD CONSTRAINT user_tournament_invite_code_unique UNIQUE (invite_code);

-- Function to generate unique invite codes
CREATE OR REPLACE FUNCTION team_tactics.generate_unique_invite_code() 
RETURNS varchar(6) AS $$
DECLARE
    chars text := 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
    result varchar(6);
    i integer;
    done boolean := false;
    attempts integer := 0;
    max_attempts integer := 10;
BEGIN
    WHILE NOT done AND attempts < max_attempts LOOP
        result := '';
        
        -- Generate a random 6-character code
        FOR i IN 1..6 LOOP
            result := result || substr(chars, ceil(random() * length(chars))::integer, 1);
        END LOOP;
        
        -- Check if this code exists
        done := NOT EXISTS(
            SELECT 1 FROM team_tactics.user_tournament WHERE invite_code = result
        );
        
        attempts := attempts + 1;
    END LOOP;
    
    -- If we couldn't find a unique code after max attempts, raise an error
    IF NOT done THEN
        RAISE EXCEPTION 'Could not generate a unique invite code after % attempts', max_attempts;
    END IF;
    
    RETURN result;
END;
$$ LANGUAGE plpgsql;

ALTER TABLE team_tactics.user_tournament 
    ALTER COLUMN invite_code SET DEFAULT team_tactics.generate_unique_invite_code();
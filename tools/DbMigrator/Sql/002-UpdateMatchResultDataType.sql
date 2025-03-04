ALTER TABLE team_tactics.match_result 
    RENAME COLUMN match_date TO timestamp;

ALTER TABLE team_tactics.match_result 
    ALTER COLUMN timestamp TYPE timestamptz;
ALTER TABLE team_tactics.player 
    RENAME COLUMN position_id TO player_position_id;

ALTER TABLE team_tactics.player
    ADD COLUMN birthdate date NOT NULL;
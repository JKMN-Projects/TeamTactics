ALTER TABLE team_tactics.player_user_team
	DROP CONSTRAINT player_user_team_user_team_id_fkey;

-- Then add it back with the CASCADE option
ALTER TABLE team_tactics.player_user_team
	ADD CONSTRAINT player_user_team_user_team_id_fkey
	FOREIGN KEY (user_team_id) 
	REFERENCES team_tactics.user_team(id)
	ON DELETE CASCADE;
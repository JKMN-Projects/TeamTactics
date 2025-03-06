ALTER TABLE team_tactics.user_team
	DROP CONSTRAINT user_team_user_tournament_id_fkey;

-- Then add it back with the CASCADE option
ALTER TABLE team_tactics.user_team
	ADD CONSTRAINT user_team_user_tournament_id_fkey
	FOREIGN KEY (user_tournament_id) 
	REFERENCES team_tactics.user_tournament(id)
	ON DELETE CASCADE;
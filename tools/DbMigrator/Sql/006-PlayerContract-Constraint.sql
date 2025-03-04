
CREATE UNIQUE INDEX idx_player_single_active_contract
	ON team_tactics.player_contract (player_id)
	WHERE active = true;
ALTER TABLE team_tactics.point_category
	ADD COLUMN external_id text NOT NULL DEFAULT '';

UPDATE team_tactics.point_category
SET external_id = CASE
    WHEN name = 'Goal' THEN 'goals'
    WHEN name = 'Assist' THEN 'assists'
    WHEN name = 'Penalty scored' THEN 'pens_made'
    WHEN name = 'Penalty attempt' THEN 'pens_att'
    WHEN name = 'Shot' THEN 'shots'
    WHEN name = 'Shot on target' THEN 'shots_on_target'
    WHEN name = 'Yellow card' THEN 'cards_yellow'
    WHEN name = 'Red card' THEN 'cards_red'
    WHEN name = 'Offside' THEN 'offsides'
    WHEN name = 'Foul committed' THEN 'fouls'
    WHEN name = 'Foul drawn' THEN 'fouled'
    WHEN name = 'Cross' THEN 'crosses'
    WHEN name = 'Own goal' THEN 'own_goals'
    WHEN name = 'Tackles won' THEN 'tackles_won'
    WHEN name = 'Interceptions' THEN 'interceptions'
    WHEN name = 'Blocked shots' THEN 'blocks'
    WHEN name = 'Aerial duels won' THEN 'aerials_won'
    WHEN name = 'Saves' THEN 'gk_saves'
    WHEN name = 'Save percentage' THEN 'gk_save_pct'
    WHEN name = 'Goal against' THEN 'gk_goals_against'
    WHEN name = 'Clean Sheet' THEN 'gk_clean_sheet'
    WHEN name = 'Appearance' THEN 'appearance'
END
WHERE name IN (
    'Goal', 'Assist', 'Penalty scored', 'Penalty attempt', 'Shot', 
    'Shot on target', 'Yellow card', 'Red card', 'Offside', 
    'Foul committed', 'Foul drawn', 'Cross', 'Own goal', 
    'Tackles won', 'Interceptions', 'Blocked shots', 'Aerial duels won', 
    'Saves', 'Save percentage', 'Goal against', 'Clean Sheet', 'Appearance'
);
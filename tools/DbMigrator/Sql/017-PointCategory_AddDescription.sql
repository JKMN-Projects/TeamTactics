ALTER TABLE team_tactics.point_category
	ADD COLUMN description text NOT NULL DEFAULT '';

UPDATE team_tactics.point_category
SET description = CASE
    WHEN name = 'Goal' THEN 'Goals personally scored'
    WHEN name = 'Assist' THEN 'Goals assisted in'
    WHEN name = 'Penalty scored' THEN 'Goals from penalties'
    WHEN name = 'Penalty attempt' THEN 'Penalties attempted on goal'
    WHEN name = 'Shot' THEN 'Shots made'
    WHEN name = 'Shot on target' THEN 'Shots made in goal frame'
    WHEN name = 'Yellow card' THEN 'Yellow cards drawn from actions'
    WHEN name = 'Red card' THEN 'Red cards drawn from actions'
    WHEN name = 'Offside' THEN 'Offside passes made'
    WHEN name = 'Foul committed' THEN 'Fouls committed on opposing players'
    WHEN name = 'Foul drawn' THEN 'Fouls drawn from opposing players'
    WHEN name = 'Cross' THEN 'Crosses made into the area of the goal'
    WHEN name = 'Own goal' THEN 'Goals made on own goal'
    WHEN name = 'Tackles won' THEN 'Tackles won on opposing players'
    WHEN name = 'Interceptions' THEN 'Intercepts made on the ball'
    WHEN name = 'Blocked shots' THEN 'Blocked shots on goal'
    WHEN name = 'Aerial duels won' THEN 'Control of ball won over opposing players'
    WHEN name = 'Saves' THEN 'Goal-stopping saves by goalkeeper'
    WHEN name = 'Save percentage' THEN 'Percentage of shots saved on target'
    WHEN name = 'Goal against' THEN 'Goals not stopped'
    WHEN name = 'Clean Sheet' THEN 'Managed to play game without any shots passing'
    WHEN name = 'Appearance' THEN 'Made an appearance this game'
END
WHERE name IN (
    'Goal', 'Assist', 'Penalty scored', 'Penalty attempt', 'Shot', 
    'Shot on target', 'Yellow card', 'Red card', 'Offside', 
    'Foul committed', 'Foul drawn', 'Cross', 'Own goal', 
    'Tackles won', 'Interceptions', 'Blocked shots', 'Aerial duels won', 
    'Saves', 'Save percentage', 'Goal against', 'Clean Sheet', 'Appearance'
);
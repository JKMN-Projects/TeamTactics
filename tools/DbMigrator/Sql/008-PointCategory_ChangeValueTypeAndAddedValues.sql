CREATE UNIQUE INDEX idx_point_category_name_active
    ON team_tactics.point_category (name)
    WHERE active = true;

ALTER TABLE team_tactics.point_category
    ALTER COLUMN point_amount TYPE decimal(6,3);

INSERT INTO team_tactics.point_category (name, point_amount, active)
VALUES 
    ('Goal', 5, true),
    ('Assist', 3, true),
    ('Penalty scored', 2, true),
    ('Penalty attempt', 1, true),
    ('Shot', 1, true),
    ('Shot on target', 2, true),
    ('Yellow card', -2, true),
    ('Red card', -5, true),
    ('Offside', -1, true),
    ('Foul committed', -1, true),
    ('Foul drawn', 1, true),
    ('Cross', 1, true),
    ('Own goal', -3, true),
    ('Tackles won', 2, true),
    ('Interceptions', 2, true),
    ('Blocked shots', 1, true),
    ('Aerial duels won', 1, true),

    ('Saves', 2, true),
    ('Save percentage', 0.05, true),
    ('Goal against', -2, true),
    ('Clean Sheet', 7, true),

    ('Appearance', 2, true);
    
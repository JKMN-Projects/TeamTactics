ALTER TABLE team_tactics.bulletin
	ADD COLUMN user_account_id int NOT NULL REFERENCES team_tactics.user_account(id),
	ADD COLUMN last_edited_time timestamptz;

ALTER TABLE team_tactics.bulletin
	RENAME COLUMN timestamp TO created_time;
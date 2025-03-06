ALTER TABLE team_tactics.club
	ADD COLUMN url_name text;

ALTER TABLE team_tactics.match_result
	ADD COLUMN url_name text,
	ADD COLUMN external_id varchar(10);
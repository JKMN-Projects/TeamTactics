-- Create schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS team_tactics;

-- Drop tables in reverse order of dependencies
DROP TABLE IF EXISTS team_tactics.coach_attribute CASCADE;
DROP TABLE IF EXISTS team_tactics.bulletin CASCADE;
DROP TABLE IF EXISTS team_tactics.match_player_point CASCADE;
DROP TABLE IF EXISTS team_tactics.point_category CASCADE;
DROP TABLE IF EXISTS team_tactics.match_result CASCADE;
DROP TABLE IF EXISTS team_tactics.club_competition CASCADE;
DROP TABLE IF EXISTS team_tactics.player_contract CASCADE;
DROP TABLE IF EXISTS team_tactics.player_user_team CASCADE;
DROP TABLE IF EXISTS team_tactics.player CASCADE;
DROP TABLE IF EXISTS team_tactics.club CASCADE;
DROP TABLE IF EXISTS team_tactics.player_position CASCADE;
DROP TABLE IF EXISTS team_tactics.user_team_user_tournament CASCADE;
DROP TABLE IF EXISTS team_tactics.user_team CASCADE;
DROP TABLE IF EXISTS team_tactics.user_tournament CASCADE;
DROP TABLE IF EXISTS team_tactics.competition CASCADE;
DROP TABLE IF EXISTS team_tactics.user_account CASCADE;

-- User and account tables
CREATE TABLE team_tactics.user_account (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    first_name      text NOT NULL,
    last_name       text NOT NULL,
    username        text NOT NULL,
    email           text NOT NULL,
    password_hash   text NOT NULL
);

-- Competition related tables
CREATE TABLE team_tactics.competition (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            text NOT NULL,
    start_date      date,
    end_date        date,
    external_id     varchar(10) NOT NULL
);

CREATE INDEX idx_competition_external_id ON team_tactics.competition(external_id);

CREATE TABLE team_tactics.user_tournament (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            text NOT NULL,
    description     text,
    user_account_id int NOT NULL REFERENCES team_tactics.user_account(id),
    competition_id  int NOT NULL REFERENCES team_tactics.competition(id),
    invite_code     varchar(6) NOT NULL
);

CREATE TABLE team_tactics.user_team (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            text NOT NULL,
    status          int NOT NULL,
    user_account_id int NOT NULL REFERENCES team_tactics.user_account(id),
    competition_id  int NOT NULL REFERENCES team_tactics.competition(id),
    locked_date     date
);

CREATE TABLE team_tactics.user_team_user_tournament (
    id                  int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    accept              boolean,
    user_team_id        int REFERENCES team_tactics.user_team(id),
    user_tournament_id  int REFERENCES team_tactics.user_tournament(id)
);

-- Player and club related tables
CREATE TABLE team_tactics.player_position (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            text NOT NULL
);

CREATE TABLE team_tactics.club (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            text NOT NULL,
    external_id     varchar(10) NOT NULL
);

CREATE INDEX idx_club_external_id ON team_tactics.club(external_id);

CREATE TABLE team_tactics.player (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    first_name      text NOT NULL,
    last_name       text NOT NULL,
    external_id     varchar(10) NOT NULL,
    club_id         int NOT NULL REFERENCES team_tactics.club(id),
    position_id     int NOT NULL REFERENCES team_tactics.player_position(id)
);

CREATE INDEX idx_player_external_id ON team_tactics.player(external_id);

CREATE TABLE team_tactics.player_user_team (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    captain         boolean NOT NULL,
    user_team_id    int NOT NULL REFERENCES team_tactics.user_team(id),
    player_id       int NOT NULL REFERENCES team_tactics.player(id)
);

CREATE TABLE team_tactics.player_contract (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    active          boolean NOT NULL,
    club_id         int NOT NULL REFERENCES team_tactics.club(id),
    player_id       int NOT NULL REFERENCES team_tactics.player(id)
);

CREATE TABLE team_tactics.club_competition (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    club_id         int NOT NULL REFERENCES team_tactics.club(id),
    competition_id  int NOT NULL REFERENCES team_tactics.competition(id)
);

-- Match and scoring related tables
CREATE TABLE team_tactics.match_result (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    home_club_score int NOT NULL,
    away_club_score int NOT NULL,
    home_club_id    int NOT NULL REFERENCES team_tactics.club(id),
    away_club_id    int NOT NULL REFERENCES team_tactics.club(id),
    match_date      date NOT NULL,
    competition_id  int NOT NULL REFERENCES team_tactics.competition(id)
);

CREATE TABLE team_tactics.point_category (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            text NOT NULL,
    point_amount    int NOT NULL,
    active          boolean NOT NULL
);

CREATE TABLE team_tactics.match_player_point (
    id                  int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    occurrences         int NOT NULL,
    match_result_id     int NOT NULL REFERENCES team_tactics.match_result(id),
    point_category_id   int NOT NULL REFERENCES team_tactics.point_category(id),
    player_id           int NOT NULL REFERENCES team_tactics.player(id)
);

-- Communication tables
CREATE TABLE team_tactics.bulletin (
    id                  int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    text                text NOT NULL,
    timestamp           timestamptz NOT NULL,
    user_tournament_id  int NOT NULL REFERENCES team_tactics.user_tournament(id)
);
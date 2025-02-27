-- Drop tables in reverse order of dependencies
DROP TABLE IF EXISTS coach_attribute CASCADE;
DROP TABLE IF EXISTS bulletin CASCADE;
DROP TABLE IF EXISTS match_player_point CASCADE;
DROP TABLE IF EXISTS point_category CASCADE;
DROP TABLE IF EXISTS match_result CASCADE;
DROP TABLE IF EXISTS club_competition CASCADE;
DROP TABLE IF EXISTS player_contract CASCADE;
DROP TABLE IF EXISTS player_user_team CASCADE;
DROP TABLE IF EXISTS player CASCADE;
DROP TABLE IF EXISTS club CASCADE;
DROP TABLE IF EXISTS player_position CASCADE;
DROP TABLE IF EXISTS player_position CASCADE;
DROP TABLE IF EXISTS user_team_user_tournament CASCADE;
DROP TABLE IF EXISTS user_team CASCADE;
DROP TABLE IF EXISTS user_tournament CASCADE;
DROP TABLE IF EXISTS competition CASCADE;
DROP TABLE IF EXISTS user_account CASCADE;

-- User and account tables
CREATE TABLE user_account (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    first_name      varchar(100) NOT NULL,
    last_name       varchar(100) NOT NULL,
    username        varchar(100) NOT NULL,
    email           varchar(100) NOT NULL,
    password_hash   varchar(100) NOT NULL
);

-- Competition related tables
CREATE TABLE competition (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            varchar(100) NOT NULL,
    start_date      date,
    end_date        date,
    external_id     varchar(10) NOT NULL
);

CREATE TABLE user_tournament (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            varchar(100) NOT NULL,
    description     varchar(250),
    user_account_id int NOT NULL REFERENCES user_account(id),
    competition_id  int NOT NULL REFERENCES competition(id),
    invite_code     varchar(6) NOT NULL
);

CREATE TABLE user_team (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            varchar(100) NOT NULL,
    status          int NOT NULL,
    user_account_id int NOT NULL REFERENCES user_account(id),
    competition_id  int NOT NULL REFERENCES competition(id),
    locked_date     date
);

CREATE TABLE user_team_user_tournament (
    id                  int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    accept              boolean,
    user_team_id        int REFERENCES user_team(id),
    user_tournament_id  int REFERENCES user_tournament(id)
);

-- Player and club related tables
CREATE TABLE player_position (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            varchar(250) NOT NULL
);

CREATE TABLE club (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            varchar(250) NOT NULL,
    external_id     varchar(10) NOT NULL
);

CREATE TABLE player (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            varchar(250) NOT NULL,
    external_id     varchar(10) NOT NULL,
    club_id         int NOT NULL REFERENCES club(id),
    position_id     int NOT NULL REFERENCES player_position(id)
);

CREATE TABLE player_user_team (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    captain         boolean NOT NULL,
    user_team_id    int NOT NULL REFERENCES user_team(id),
    player_id       int NOT NULL REFERENCES player(id)
);

CREATE TABLE player_contract (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    active          boolean NOT NULL,
    club_id         int NOT NULL REFERENCES club(id),
    player_id       int NOT NULL REFERENCES player(id)
);

CREATE TABLE club_competition (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    club_id         int NOT NULL REFERENCES club(id),
    competition_id  int NOT NULL REFERENCES competition(id)
);

-- Match and scoring related tables
CREATE TABLE match_result (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    home_club_score int NOT NULL,
    away_club_score int NOT NULL,
    home_club_id    int NOT NULL REFERENCES club(id),
    away_club_id    int NOT NULL REFERENCES club(id),
    match_date      date NOT NULL,
    competition_id  int NOT NULL REFERENCES competition(id)
);

CREATE TABLE point_category (
    id              int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name            varchar(250) NOT NULL,
    point_amount    int NOT NULL,
    active          boolean NOT NULL
);

CREATE TABLE match_player_point (
    id                  int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    occurrences         int NOT NULL,
    match_result_id     int NOT NULL REFERENCES match_result(id),
    point_category_id   int NOT NULL REFERENCES point_category(id),
    player_id           int NOT NULL REFERENCES player(id)
);

-- Communication tables
CREATE TABLE bulletin (
    id                  int NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    text                text NOT NULL,
    timestamp           timestamptz NOT NULL,
    user_tournament_id  int NOT NULL REFERENCES user_tournament(id)
);
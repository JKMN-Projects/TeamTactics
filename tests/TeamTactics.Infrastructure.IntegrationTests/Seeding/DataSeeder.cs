using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Matches;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Users;

namespace TeamTactics.Infrastructure.IntegrationTests.Seeding
{
    public sealed class DataSeeder
    {
        private readonly IDbConnection _dbConnection;

        public DataSeeder(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        #region Seed User
        public async Task<User> SeedRandomUserAsync()
        {
            User user = new UserFaker()
                .Generate();
            await SeedUserAsync(user);
            return user;
        }

        public async Task<int> SeedUserAsync(User user)
        {
            var faker = new Faker();
            var parameters = new DynamicParameters();
            parameters.Add("Email", user.Email);
            parameters.Add("Username", user.Username);
            parameters.Add("Salt", Convert.ToBase64String(faker.Random.Bytes(32)));
            parameters.Add("PasswordHash", Convert.ToBase64String(faker.Random.Bytes(32)));
            string sql = $@"
                INSERT INTO team_tactics.user_account (email, username, salt, password_hash)
                VALUES ({string.Join(", ", parameters.ParameterNames.Select(p => "@" + p))})
                RETURNING id";
            int id = await _dbConnection.QuerySingleAsync<int>(sql, parameters);
            user.SetId(id);
            return id;
        }
        #endregion

        #region Seed Competition
        public async Task<Competition> SeedRandomCompetitionAsync()
        {
            Competition competition = new CompetitionFaker()
                .Generate();
            await SeedCompetitionAsync(competition);
            return competition;
        }

        public async Task<int> SeedCompetitionAsync(Competition competition)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Name", competition.Name);
            parameters.Add("StartDate", competition.StartDate);
            parameters.Add("EndDate", competition.EndDate);
            parameters.Add("ExternalId", Guid.NewGuid().ToString().Substring(0, 10));

            string sql = $@"
                INSERT INTO team_tactics.competition (name, start_date, end_date, external_id)
                VALUES ({string.Join(", ", parameters.ParameterNames.Select(p => "@" + p))})
                RETURNING id"
            ;

            int id = await _dbConnection.QuerySingleAsync<int>(sql, parameters);
            competition.SetId(id);
            return id;
        }
        #endregion

        #region Seed Club
        public async Task<Club> SeedRandomClubAsync()
        {
            Club club = new ClubFaker()
                .Generate();
            await SeedClubAsync(club);
            return club;
        }

        public async Task<int> SeedClubAsync(Club club)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Name", club.Name);
            parameters.Add("ExternalId", club.ExternalId);
            string sql = $@"
                INSERT INTO team_tactics.club (name, external_id)
                VALUES ({string.Join(", ", parameters.ParameterNames.Select(p => "@" + p))})
                RETURNING id";

            int id = await _dbConnection.QuerySingleAsync<int>(sql, parameters);
            club.SetId(id);
            return id;
        }

        public async Task<IEnumerable<Club>> SeedRandomClubsAsync(int count)
        {
            using var transaction = _dbConnection.BeginTransaction();
            try
            {
                List<Club> clubs = new List<Club>();
                for (int i = 0; i < count; i++)
                {
                    Club club = new ClubFaker()
                        .Generate();
                    await SeedClubAsync(club);
                    clubs.Add(club);
                }
                transaction.Commit();
                return clubs;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        #endregion

        #region Seed Player
        public async Task<Player> SeedRandomPlayerAsync()
        {
            Player player = new PlayerFaker()
                .Generate();
            await SeedPlayerAsync(player);
            return player;
        }

        public async Task<int> SeedPlayerAsync(Player player)
        {
            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("FirstName", player.FirstName);
                parameters.Add("LastName", player.LastName);
                parameters.Add("ExternalId", player.ExternalId);
                parameters.Add("PositionId", player.PositionId);
                parameters.Add("DateOfBirth", player.BirthDate);
                string sql = $@"
                    INSERT INTO team_tactics.player (first_name, last_name, external_id, player_position_id, birthdate)
                    VALUES ({string.Join(", ", parameters.ParameterNames.Select(p => "@" + p))})
                    RETURNING id";
                int id = await _dbConnection.QuerySingleAsync<int>(sql, parameters);
                player.SetId(id);

                List<string> contractValues = [];
                var contractParameters = new DynamicParameters();
                for (int i = 0; i < player.PlayerContracts.Count; i++)
                {
                    var contract = player.PlayerContracts.ElementAt(i);
                    contractParameters.Add($"ClubId{i}", contract.ClubId);
                    contractParameters.Add($"PlayerId{i}", id);
                    contractParameters.Add($"Active{i}", contract.Active);
                    contractValues.Add($"(@ClubId{i}, @PlayerId{i}, @Active{i})");
                }
                string contractSql = $@"
                    INSERT INTO team_tactics.player_contract (club_id, player_id, active)
                    VALUES {string.Join(',', contractValues)}";

                await _dbConnection.ExecuteAsync(contractSql, contractParameters);
                player.SetId(id);
                transaction.Commit();
                return id;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<Player>> SeedClubPlayersAsync(Club club, int count = 11)
        {
            List<Player> players = new List<Player>();
            for (int i = 0; i < count; i++)
            {
                int positionId = i - (i / 4 * 4) + 1;
                Player player = new PlayerFaker(clubs: club)
                        .RuleFor(p => p.PositionId, positionId)
                        .Generate();
                await SeedPlayerAsync(player);
                players.Add(player);
            }
            return players;
        }
        #endregion

        #region Seed Matches
        public async Task<IEnumerable<Match>> SeedRandomMatchesAsync(int competitionId, IEnumerable<Club>? clubs = null)
        {
            clubs ??= await SeedRandomClubsAsync(10);

            // Generate matches between clubs
            List<Match> matches = new List<Match>();
            foreach (var homeClub in clubs)
            {
                foreach (var awayClub in clubs)
                {
                    if (homeClub == awayClub)
                    {
                        continue;
                    }
                    Match match = new MatchFaker(
                        competitionId: competitionId,
                        homeTeam: homeClub,
                        awayTeam: awayClub)
                        .Generate();
                    await SeedMatchAsync(match);
                    matches.Add(match);
                }
            }
            return matches;
        }

        public async Task<int> SeedMatchAsync(Match match)
        {
            var parameters = new DynamicParameters();
            parameters.Add("HomeClubId", match.HomeClubId);
            parameters.Add("AwayClubId", match.AwayClubId);
            parameters.Add("HomeClubScore", match.HomeClubScore);
            parameters.Add("AwayClubScore", match.AwayClubScore);
            parameters.Add("CompetitionId", match.CompetitionId);
            parameters.Add("UtcTimestamp", match.Timestamp);
            string sql = $@"
                INSERT INTO team_tactics.match_result (home_club_id, away_club_id, home_club_score, away_club_score, competition_id, timestamp)
                VALUES ({string.Join(", ", parameters.ParameterNames.Select(p => "@" + p))})
                RETURNING id";
            int id = await _dbConnection.QuerySingleAsync<int>(sql, parameters);
            match.SetId(id);
            return id;
        }
        #endregion

        #region Seed MatchPlayerPoints
        public async Task<IEnumerable<MatchPlayerPoint>> SeedRandomMatchPlayerPoints(Match match, IEnumerable<Player> homeTeamPlayers, IEnumerable<Player> awayTeamPlayers)
        {
            // 1. Assign playerPoints based on score of match. Use these to seed MatchPlayerPoints
            const int goalCategoryId = 1;
            const int assistCategoryId = 2;
            const int shotOnTargetCategoryId = 6;
            const int goalAgainstCategoryId = 20;
            const int cleanSheetCategoryId = 21;
            const int appearanceCategoryId = 22;

            const int defenderPositionId = 3;
            const int goalkeeperPositionId = 4;

            Faker faker = new Faker();
            List<MatchPlayerPoint> playerPoints = new List<MatchPlayerPoint>();

            var assignPointsAction = (int teamScore, IEnumerable<Player> players) =>
            {
                for (int i = 0; i < match.HomeClubScore; i++)
                {
                    // Assign goal to player
                    var scoringPlayer = faker.PickRandom(players);
                    playerPoints.Add(new MatchPlayerPoint(
                        matchId: match.Id,
                        playerId: scoringPlayer.Id,
                        pointCategoryId: goalCategoryId,
                        occurrences: 1
                    ));
                    // Assign shot on target to player
                    playerPoints.Add(new MatchPlayerPoint(
                        matchId: match.Id,
                        playerId: scoringPlayer.Id,
                        pointCategoryId: shotOnTargetCategoryId,
                        occurrences: 1
                    ));

                    // Assign assist to another player on the team
                    var assistingPlayer = faker.PickRandom(players.Where(p => p != scoringPlayer));
                    playerPoints.Add(new MatchPlayerPoint(
                        matchId: match.Id,
                        playerId: assistingPlayer.Id,
                        pointCategoryId: assistCategoryId,
                        occurrences: 1
                    ));

                    // Assign appearance to all players
                    foreach (var player in players)
                    {
                        playerPoints.Add(new MatchPlayerPoint(
                            matchId: match.Id,
                            playerId: player.Id,
                            pointCategoryId: appearanceCategoryId,
                            occurrences: 1
                        ));
                    }
                }
            };

            assignPointsAction(match.HomeClubScore, homeTeamPlayers);
            assignPointsAction(match.AwayClubScore, awayTeamPlayers);

            var assignCleanSheetPoints = (IEnumerable<Player> players) =>
            {
                // Clean sheet for goalkeeper and defenders
                var goalkeeper = players.First(p => p.PositionId == goalkeeperPositionId);
                playerPoints.Add(new MatchPlayerPoint(
                    matchId: match.Id,
                    playerId: goalkeeper.Id,
                    pointCategoryId: cleanSheetCategoryId,
                    occurrences: 1
                ));
                foreach (var defender in players.Where(p => p.PositionId == defenderPositionId))
                {
                    playerPoints.Add(new MatchPlayerPoint(
                        matchId: match.Id,
                        playerId: defender.Id,
                        pointCategoryId: cleanSheetCategoryId,
                        occurrences: 1
                    ));
                }
            };

            var assignGoalAgainstPoints = (int concededGoals, IEnumerable<Player> players) =>
            {
                // Goal against for goalkeeper and defenders
                var goalkeeper = players.First(p => p.PositionId == goalkeeperPositionId);
                playerPoints.Add(new MatchPlayerPoint(
                    matchId: match.Id,
                    playerId: goalkeeper.Id,
                    pointCategoryId: goalAgainstCategoryId,
                    occurrences: concededGoals
                ));
                foreach (var defender in players.Where(p => p.PositionId == defenderPositionId))
                {
                    playerPoints.Add(new MatchPlayerPoint(
                        matchId: match.Id,
                        playerId: defender.Id,
                        pointCategoryId: goalAgainstCategoryId,
                        occurrences: concededGoals
                    ));
                }
            };

            if (match.HomeClubScore == 0)
            {
                assignCleanSheetPoints(awayTeamPlayers);
            }
            else
            {
                assignGoalAgainstPoints(match.HomeClubScore, awayTeamPlayers);
            }

            if (match.AwayClubScore == 0)
            {
                assignCleanSheetPoints(homeTeamPlayers);
            }
            else
            {
                assignGoalAgainstPoints(match.AwayClubScore, homeTeamPlayers);
            }

            // Group by player and point category and increase occurrences
            IEnumerable<MatchPlayerPoint> groupedPlayerPoints = playerPoints
                .GroupBy(p => new { p.PlayerId, p.PointCategoryId })
                .Select(g => new MatchPlayerPoint(
                    matchId: match.Id,
                    playerId: g.Key.PlayerId,
                    pointCategoryId: g.Key.PointCategoryId,
                    occurrences: g.Count()
                ));

            foreach (var playerPoint in groupedPlayerPoints)
            {
                await SeedMatchPlayerPointAsync(playerPoint);
            }

            return groupedPlayerPoints;
        }

        public async Task<int> SeedMatchPlayerPointAsync(MatchPlayerPoint playerPoint)
        {
            var parameters = new DynamicParameters();
            parameters.Add("MatchResultId", playerPoint.MatchId);
            parameters.Add("PlayerId", playerPoint.PlayerId);
            parameters.Add("PointCategoryId", playerPoint.PointCategoryId);
            parameters.Add("Occurrences", playerPoint.Occurrences);
            string sql = $@"
                INSERT INTO team_tactics.match_player_point (match_result_id, player_id, point_category_id, occurrences)
                VALUES ({string.Join(", ", parameters.ParameterNames.Select(p => "@" + p))})
                RETURNING id";
            int id = await _dbConnection.QuerySingleAsync<int>(sql, parameters);
            playerPoint.SetId(id);
            return id;
        }
        #endregion
    }

    public static class DataSeederExtensions
    {
        public sealed record CompetitionSeedResult(
            Competition Competition,
            IReadOnlyCollection<ClubSeedResult> Clubs);
        public sealed record ClubSeedResult(
            Club Club,
            IReadOnlyCollection<Player> Players);

        public static async Task<CompetitionSeedResult> SeedFullCompetitionAsync(this DataSeeder dataSeeder)
        {
            Competition competition = await dataSeeder.SeedRandomCompetitionAsync();

            List<ClubSeedResult> seededClubs = [];
            var clubs = await dataSeeder.SeedRandomClubsAsync(10);
            foreach (var club in clubs)
            {
                var players = await dataSeeder.SeedClubPlayersAsync(club);
                seededClubs.Add(new ClubSeedResult(club, players.ToList()));
            }

            return new CompetitionSeedResult(competition, seededClubs);
        }

        public sealed record MatchSeedResult(
            Match Match,
            IReadOnlyCollection<MatchPlayerPoint> PlayerPoints);

        public static async Task<IEnumerable<MatchSeedResult>> SeedCompetitionMatchesAsync(this DataSeeder dataSeeder, CompetitionSeedResult competitionSeedResult)
        {
            Competition competition = competitionSeedResult.Competition;
            IEnumerable<ClubSeedResult> seededClubs = competitionSeedResult.Clubs;

            List<MatchSeedResult> matchSeedResults = [];
            IEnumerable<Match> matches = await dataSeeder.SeedRandomMatchesAsync(competition.Id, clubs: seededClubs.Select(x => x.Club));
            foreach (var match in matches)
            {
                var homeTeamPlayers = seededClubs.First(c => c.Club.Id == match.HomeClubId).Players;
                var awayTeamPlayers = seededClubs.First(c => c.Club.Id == match.AwayClubId).Players;
                var playerPoints = await dataSeeder.SeedRandomMatchPlayerPoints(match, homeTeamPlayers, awayTeamPlayers);
                matchSeedResults.Add(new MatchSeedResult(match, playerPoints.ToList()));
            }
            return matchSeedResults;
        }
    }
}

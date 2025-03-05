using Dapper;
using System.Data;
using System.Data.Common;

namespace TeamTactics.Infrastructure.Database.Scraper
{
    public class PointCategory(int id, string name, double pointAmount, bool active)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public double PointAmount { get; set; } = pointAmount;
        public bool Active { get; set; } = active;
    }

    public class PlayerPosition(int id, string name)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
    }

    public class Club(string name, string externalId)
    {
        public int Id { get; set; }
        public string Name { get; set; } = name;
        public string ExternalId { get; set; } = externalId;
    }

    public class Player(string firstName, string lastName, DateOnly birthdate, string externalId, int positionId)
    {
        public int Id { get; set; }
        public string FirstName { get; private set; } = firstName;
        public string LastName { get; private set; } = lastName;
        public DateOnly BirthDate { get; private set; } = birthdate;
        public string ExternalId { get; private set; } = externalId;
        public int PositionId { get; private set; } = positionId;
    }

    public class PlayerContract(bool active, int clubId, int playerId)
    {
        public int Id { get; set; }
        public bool Active { get; set; } = active;
        public int ClubId { get; set; } = clubId;
        public int PlayerId { get; set; } = playerId;
    }

    public class MatchResult
    {
        public int Id { get; set; }
        public int HomeClubScore { get; set; }
        public int AwayClubScore { get; set; }
        public DateTime Timestamp { get; set; }
        public string UrlName { get; set; }
        public string ExternalId { get; set; }
        public int HomeClubId { get; set; }
        public int AwayClubId { get; set; }
        public int CompetitionId { get; set; }

    }

    public class MatchPlayerPoint
    {
        public int Id { get; set; }
        public int Occurrences { get; set; }
        public int MatchId { get; set; }
        public int PointCategoryId { get; set; }
        public int PlayerId { get; set; }
    }

    public class ScraperRepository(IDbConnection dbConnection)
    {
        private IDbConnection _dbConnection = dbConnection;

        public async Task<IEnumerable<PlayerPosition>> GetPlayerPositions()
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            const string sql = @"
        SELECT id, name 
        FROM team_tactics.player_position
        ORDER BY id";

            return await _dbConnection.QueryAsync<PlayerPosition>(sql);
        }

        public async Task<IEnumerable<PointCategory>> GetPointCategories()
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            const string sql = @"
        SELECT id, name, point_amount, active 
        FROM team_tactics.point_category
        WHERE active = true
        ORDER BY id";

            return await _dbConnection.QueryAsync<PointCategory>(sql);
        }

        public async Task<IEnumerable<Club>> InsertClubsAndClubCompetitionsBulk(IEnumerable<Club> clubs, string competitionExternalId)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    // First, get the competition ID using the external ID
                    string getCompetitionIdSql = @"
                SELECT id FROM team_tactics.competition 
                WHERE external_id = @ExternalId";

                    int? competitionId = await _dbConnection.QuerySingleOrDefaultAsync<int?>(
                        getCompetitionIdSql,
                        new { ExternalId = competitionExternalId },
                        transaction);

                    if (competitionId == null)
                        throw new Exception($"Competition with external ID '{competitionExternalId}' not found");

                    // Build a bulk insert query for clubs
                    var valuesClauses = new List<string>();
                    var parameters = new DynamicParameters();
                    int i = 0;

                    var clubsList = clubs.ToList(); // Convert to list for easier indexing

                    foreach (var club in clubsList)
                    {
                        valuesClauses.Add($"(@Name{i}, @ExternalId{i})");
                        parameters.Add($"Name{i}", club.Name);
                        parameters.Add($"ExternalId{i}", club.ExternalId);
                        i++;
                    }

                    string sql = $@"
        INSERT INTO team_tactics.club (name, external_id) 
        VALUES {string.Join(", ", valuesClauses)}
        RETURNING id";

                    var ids = (await _dbConnection.QueryAsync<int>(sql, parameters)).ToList();

                    // Assign IDs to the original club objects
                    for (int j = 0; j < clubsList.Count; j++)
                    {
                        clubsList[j].Id = ids[j];
                    }

                    // Now insert club_competition entries
                    var competitionValuesClauses = new List<string>();
                    var competitionParameters = new DynamicParameters();
                    int k = 0;

                    foreach (var clubId in ids)
                    {
                        competitionValuesClauses.Add($"(@ClubId{k}, @CompetitionId{k})");
                        competitionParameters.Add($"ClubId{k}", clubId);
                        competitionParameters.Add($"CompetitionId{k}", competitionId);
                        k++;
                    }

                    string clubCompetitionInsertSql = $@"
                INSERT INTO team_tactics.club_competition (club_id, competition_id) 
                VALUES {string.Join(", ", competitionValuesClauses)}";

                    await _dbConnection.ExecuteAsync(clubCompetitionInsertSql, competitionParameters, transaction);


                    transaction.Commit();
                    return clubsList;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<Player>> InsertPlayersBulk(IEnumerable<Player> players)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();


            // Build a bulk insert query with multiple VALUES clauses
            var valuesClauses = new List<string>();
            var parameters = new DynamicParameters();
            int i = 0;

            var playersList = players.ToList(); // Convert to list for easier indexing

            foreach (var player in playersList)
            {
                valuesClauses.Add($"(@FirstName{i}, @LastName{i}, @ExternalId{i}, @PositionId{i})");
                parameters.Add($"FirstName{i}", player.FirstName);
                parameters.Add($"LastName{i}", player.LastName);
                parameters.Add($"ExternalId{i}", player.ExternalId);
                parameters.Add($"PositionId{i}", player.PositionId);
                i++;
            }

            string sql = $@"
        INSERT INTO team_tactics.player (first_name, last_name, external_id, position_id) 
        VALUES {string.Join(", ", valuesClauses)}
        RETURNING id";

            var ids = (await _dbConnection.QueryAsync<int>(sql, parameters)).ToList();

            // Assign IDs to the original player objects
            for (int j = 0; j < playersList.Count; j++)
            {
                playersList[j].Id = ids[j];
            }

            return playersList;
        }

        public async Task<IEnumerable<PlayerContract>> InsertPlayerContractsBulk(IEnumerable<PlayerContract> contracts)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    // For each new active contract, deactivate any existing active contracts for the same player
                    foreach (var contract in contracts.Where(c => c.Active))
                    {
                        const string deactivateExistingSql = @"
                    UPDATE team_tactics.player_contract
                    SET active = false
                    WHERE player_id = @PlayerId AND active = true";

                        await _dbConnection.ExecuteAsync(deactivateExistingSql,
                            new { contract.PlayerId },
                            transaction);
                    }

                    // Build a bulk insert query with multiple VALUES clauses
                    var valuesClauses = new List<string>();
                    var parameters = new DynamicParameters();
                    int i = 0;

                    var contractsList = contracts.ToList(); // Convert to list for easier indexing

                    foreach (var contract in contractsList)
                    {
                        valuesClauses.Add($"(@ClubId{i}, @PlayerId{i}, @Active{i})");
                        parameters.Add($"ClubId{i}", contract.ClubId);
                        parameters.Add($"PlayerId{i}", contract.PlayerId);
                        parameters.Add($"Active{i}", contract.Active);
                        i++;
                    }

                    string sql = $@"
                INSERT INTO team_tactics.player_contract (club_id, player_id, active) 
                VALUES {string.Join(", ", valuesClauses)}
                RETURNING id";

                    var ids = (await _dbConnection.QueryAsync<int>(sql, parameters, transaction)).ToList();

                    // Assign IDs to the original contract objects
                    for (int j = 0; j < contractsList.Count; j++)
                    {
                        contractsList[j].Id = ids[j];
                    }

                    transaction.Commit();
                    return contractsList;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<MatchResult>> InsertMatchResultsBulk(IEnumerable<MatchResult> matchResults)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    var valuesClauses = new List<string>();
                    var parameters = new DynamicParameters();
                    int i = 0;

                    var matchResultsList = matchResults.ToList();

                    foreach (var match in matchResultsList)
                    {
                        valuesClauses.Add($"(@HomeClubScore{i}, @AwayClubScore{i}, @HomeClubId{i}, @AwayClubId{i}, @Timestamp{i}, @CompetitionId{i}, @UrlName{i}, @ExternalId{i})");
                        parameters.Add($"HomeClubScore{i}", match.HomeClubScore);
                        parameters.Add($"AwayClubScore{i}", match.AwayClubScore);
                        parameters.Add($"HomeClubId{i}", match.HomeClubId);
                        parameters.Add($"AwayClubId{i}", match.AwayClubId);
                        parameters.Add($"Timestamp{i}", match.Timestamp);
                        parameters.Add($"CompetitionId{i}", match.CompetitionId);
                        parameters.Add($"UrlName{i}", match.UrlName);
                        parameters.Add($"ExternalId{i}", match.ExternalId);
                        i++;
                    }

                    string sql = $@"
                INSERT INTO team_tactics.match_result (
                    home_club_score, away_club_score, 
                    home_club_id, away_club_id, 
                    timestamp, competition_id,
                    url_name, external_id)
                VALUES {string.Join(", ", valuesClauses)}
                RETURNING id";

                    var ids = (await _dbConnection.QueryAsync<int>(sql, parameters, transaction)).ToList();

                    for (int j = 0; j < matchResultsList.Count; j++)
                    {
                        matchResultsList[j].Id = ids[j];
                    }

                    transaction.Commit();
                    return matchResultsList;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<MatchPlayerPoint>> InsertMatchPlayerPointsBulk(IEnumerable<MatchPlayerPoint> matchPlayerPoints)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            using (var transaction = _dbConnection.BeginTransaction())
            {
                try
                {
                    var valuesClauses = new List<string>();
                    var parameters = new DynamicParameters();
                    int i = 0;

                    var pointsList = matchPlayerPoints.ToList();

                    foreach (var point in pointsList)
                    {
                        valuesClauses.Add($"(@Occurences{i}, @MatchId{i}, @PointCategoryId{i}, @PlayerId{i})");
                        parameters.Add($"Occurences{i}", point.Occurrences);
                        parameters.Add($"MatchId{i}", point.MatchId);
                        parameters.Add($"PointCategoryId{i}", point.PointCategoryId);
                        parameters.Add($"PlayerId{i}", point.PlayerId);
                        i++;
                    }

                    string sql = $@"
                INSERT INTO team_tactics.match_player_point (
                    occurrences, match_result_id, 
                    point_category_id, player_id)
                VALUES {string.Join(", ", valuesClauses)}
                RETURNING id";

                    var ids = (await _dbConnection.QueryAsync<int>(sql, parameters, transaction)).ToList();

                    for (int j = 0; j < pointsList.Count; j++)
                    {
                        pointsList[j].Id = ids[j];
                    }

                    transaction.Commit();
                    return pointsList;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}

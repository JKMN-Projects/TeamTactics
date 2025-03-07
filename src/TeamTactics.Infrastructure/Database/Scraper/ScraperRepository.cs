using Dapper;
using System.Data;
using System.Data.Common;
using TeamTactics.Application.Scraper;
using TeamTactics.Domain.Competitions;

namespace TeamTactics.Infrastructure.Database.Scraper;

public class ScraperRepository(IDbConnection dbConnection) : IScraperRepository
{
    private IDbConnection _dbConnection = dbConnection;

    public async Task<IEnumerable<PlayerPositionScrape>> GetPlayerPositions()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        const string sql = @"
        SELECT id, name 
        FROM team_tactics.player_position
        ORDER BY id";

        return await _dbConnection.QueryAsync<PlayerPositionScrape>(sql);
    }

    public async Task<IEnumerable<PointCategoryScrape>> GetPointCategories()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        const string sql = @"
        SELECT id, name, point_amount, active 
        FROM team_tactics.point_category
        WHERE active = true
        ORDER BY id";

        return await _dbConnection.QueryAsync<PointCategoryScrape>(sql);
    }

    public async Task<IEnumerable<ClubScrape>> InsertClubsAndClubCompetitionsBulk(IEnumerable<ClubScrape> clubs, string competitionExternalId)
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

    public async Task<IEnumerable<PlayerScrape>> InsertPlayersBulk(IEnumerable<PlayerScrape> players)
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
            valuesClauses.Add($"(@FirstName{i}, @LastName{i}, @ExternalId{i}, @PositionId{i}, @Birthdate{i})");
            parameters.Add($"FirstName{i}", player.FirstName);
            parameters.Add($"LastName{i}", player.LastName);
            parameters.Add($"ExternalId{i}", player.ExternalId);
            parameters.Add($"PositionId{i}", player.PositionId);
            parameters.Add($"Birthdate{i}", player.BirthDate.ToDateTime(time: TimeOnly.MinValue));
            i++;
        }

        string sql = $@"
        INSERT INTO team_tactics.player (first_name, last_name, external_id, player_position_id, birthdate) 
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

    public async Task<IEnumerable<PlayerContractScrape>> InsertPlayerContractsBulk(IEnumerable<PlayerContractScrape> contracts)
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
    public async Task<MatchResultScrape?> GetLatestMatch(int competitionId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        const string sql = @"
        SELECT  id, home_club_score, away_club_score, home_club_id, away_club_id, ""timestamp"", competition_id
        FROM team_tactics.match_result
        WHERE competition_id = @Id
        ORDER BY ""timestamp"" DESC
        LIMIT 1";


        var parameters = new DynamicParameters();
        parameters.Add("Id", competitionId);

        return await _dbConnection.QueryFirstOrDefaultAsync<MatchResultScrape>(sql,parameters);

    }
    public async Task<IEnumerable<MatchResultScrape>> InsertMatchResultsBulk(IEnumerable<MatchResultScrape> matchResults)
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
            catch(Exception e)
            {
                
                transaction.Rollback();
                throw;
            }
        }
    }

    public async Task<IEnumerable<MatchPlayerPointScrape>> InsertMatchPlayerPointsBulk(IEnumerable<MatchPlayerPointScrape> matchPlayerPoints)
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

    public async Task<IEnumerable<ClubScrape>> GetClubs(int competitionId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        const string sql = @"
        SELECT c.name, c.external_id, c.id
        FROM team_tactics.club c
        JOIN team_tactics.club_competition cc ON c.id = cc.club_id
        WHERE cc.competition_id = @CompetitionId
        ORDER BY c.id";


        var parameters = new DynamicParameters();
        parameters.Add("CompetitionId", competitionId);
        return await _dbConnection.QueryAsync<ClubScrape>(sql,parameters);
    }

    public async Task<IEnumerable<MatchResultScrape>> GetFixturesWithNoStats()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        const string sql = @"
        SELECT 
            mr.id AS Id ,
            mr.home_club_score AS HomeClubScore,
            mr.away_club_score AS AwayClubScore,
            mr.home_club_id AS HomeClubId,
            mr.away_club_id AS AwayClubId, 
            mr.""timestamp"" AS Timestamp,
            mr.competition_id AS CompetitionId,
            mr.url_name AS UrlName,
            mr.external_id AS ExternalId
        FROM team_tactics.match_result mr
        LEFT JOIN team_tactics.match_player_point mpp ON mr.id = mpp.match_result_id
        WHERE mpp.match_result_id IS NULL;";



        return await _dbConnection.QueryAsync<MatchResultScrape>(sql);

    }
    public async Task<ClubScrape?> GetClubById(int clubId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();


        const string sql = @"
        SELECT c.name, c.external_id, c.id
        FROM team_tactics.club c
        WHERE c.id = @ClubId
        LIMIT 1";


        var parameters = new DynamicParameters();
        parameters.Add("ClubId", clubId);
        return await _dbConnection.QueryFirstOrDefaultAsync<ClubScrape>(sql, parameters);


    }

    public async Task<PlayerScrape?> GetPlayerIdByExternalIdAndClubId(string externalPlayerId, int clubId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();


        const string sql = @"SELECT 
            p.id, 
            p.first_name AS firstName, 
            p.last_name AS lastName, 
            p.external_id AS externalId, 
            p.player_position_id AS positionId, 
            p.birthdate
        FROM 
            team_tactics.player p
        INNER JOIN 
            team_tactics.player_contract pc ON p.id = pc.player_id
        WHERE 
            pc.club_id = @ClubId 
            AND p.external_id = @ExternalPlayerId
            AND pc.active = true
        LIMIT 1
        ";


        var parameters = new DynamicParameters();
        parameters.Add("ExternalPlayerId", externalPlayerId);
        parameters.Add("ClubId", clubId);
        return await _dbConnection.QueryFirstOrDefaultAsync<PlayerScrape>(sql, parameters);
    }
}

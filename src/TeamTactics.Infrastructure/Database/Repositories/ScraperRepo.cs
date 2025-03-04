using Dapper;
using System.Data.Common;
using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Players;

namespace TeamTactics.Infrastructure.Database.Repositories
{
    public class ScraperRepo
    {
        private DbConnection _dbConnection;

        public ScraperRepo(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Club>> InsertClubsBulk(IEnumerable<Club> clubs)
        {
            if (_dbConnection.State != System.Data.ConnectionState.Open)
                await _dbConnection.OpenAsync();

            // Build a bulk insert query with multiple VALUES clauses
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

            return clubsList;
        }

        public async Task<IEnumerable<PlayerPosition>> GetPlayerPositions()
        {
            if (_dbConnection.State != System.Data.ConnectionState.Open)
                await _dbConnection.OpenAsync();

            const string sql = @"
        SELECT id, name 
        FROM team_tactics.player_position
        ORDER BY id";

            return await _dbConnection.QueryAsync<PlayerPosition>(sql);
        }

        public async Task<IEnumerable<Player>> InsertPlayersBulk(IEnumerable<Player> players)
        {
            if (_dbConnection.State != System.Data.ConnectionState.Open)
                await _dbConnection.OpenAsync();

            // Build a bulk insert query with multiple VALUES clauses
            var valuesClauses = new List<string>();
            var parameters = new DynamicParameters();
            int i = 0;

            var playersList = players.ToList(); // Convert to list for easier indexing

            foreach (var player in playersList)
            {
                valuesClauses.Add($"(@FirstName{i}, @LastName{i}, @ExternalId{i}, @ClubId{i}, @PositionId{i})");
                parameters.Add($"FirstName{i}", player.FirstName);
                parameters.Add($"LastName{i}", player.LastName);
                parameters.Add($"ExternalId{i}", player.ExternalId);
                parameters.Add($"ClubId{i}", player.ClubId);
                parameters.Add($"PositionId{i}", player.PositionId);
                i++;
            }

            string sql = $@"
        INSERT INTO team_tactics.player (first_name, last_name, external_id, club_id, position_id) 
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
    }
}

using Dapper;
using System.Data;
using TeamTactics.Application.Teams;
using TeamTactics.Application.Matches;

namespace TeamTactics.Infrastructure.Database.Repositories;

class MatchRepository(IDbConnection dbConnection) : IMatchRepository
{
    private IDbConnection _dbConnection = dbConnection;

    public Task<IEnumerable<Match>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Match?> FindByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<Match?> GetMatchesByTournamentIdAsync(int tournamentId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @$"
	SELECT 
	    hc.name AS ""homeClubName"",
	    ac.name AS ""awayClubName"",
	    mr.home_club_score AS ""homeClubScore"",
	    mr.away_club_score  AS ""awayClubScore"",
	    co.name AS ""competitionName"",
	    mr.""timestamp"" AS ""utcTimestamp""
	FROM team_tactics.user_tournament as ut 
	JOIN team_tactics.competition AS co
		ON co.id = ut.competition_id
	JOIN team_tactics.match_result AS mr 
		ON mr.competition_id = ut.competition_id
	JOIN team_tactics.club AS hc
		ON hc.id = mr.home_club_id
	JOIN team_tactics.club AS ac
		ON ac.id = mr.away_club_id
	WHERE ut.id = 1";

        var parameters = new DynamicParameters();
        parameters.Add("TournamentId", tournamentId);

        var result = await _dbConnection.QuerySingleOrDefaultAsync<(string HClubName, string AClubName, int HClubScore, int AClubScore, string competitionName, DateTime utcTimestamp)?>(sql, parameters);

        return result.HasValue ? new Match(result.Value.HClubName, result.Value.AClubName, result.Value.HClubScore, result.Value.AClubScore, result.Value.competitionName, result.Value.utcTimestamp) : null;
    }
}

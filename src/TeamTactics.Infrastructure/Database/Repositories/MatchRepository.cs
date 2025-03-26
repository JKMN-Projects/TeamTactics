using Dapper;
using System.Data;
using TeamTactics.Application.Matches;
using TeamTactics.Domain.Matches;

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

    public async Task<IEnumerable<MatchDto>> GetMatchesByTournamentIdAsync(int tournamentId)
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
	WHERE ut.id = @TournamentId";

        var parameters = new DynamicParameters();
        parameters.Add("TournamentId", tournamentId);

        var results = await _dbConnection.QueryAsync<(string hClubName, string aClubName, int hClubScore, int aClubScore, string competitionName, DateTime utcTimestamp)>(sql, parameters);

        return results.Select(r => new MatchDto(r.hClubName, r.aClubName, r.hClubScore, r.aClubScore, r.competitionName, r.utcTimestamp));
    }
}

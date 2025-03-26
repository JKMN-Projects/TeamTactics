using Dapper;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using TeamTactics.Application.Points;
using TeamTactics.Application.Teams;
using TeamTactics.Domain.Points;

namespace TeamTactics.Infrastructure.Database.Repositories;

class PointRepository(IDbConnection dbConnection) : IPointsRepository
{
    private IDbConnection _dbConnection = dbConnection;

    public async Task<IEnumerable<PointCategoryDto>> FindAllActiveAsync()
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = $@"
    SELECT 
        name, 
        description, 
        point_amount
    FROM team_tactics.point_category 
    WHERE active = true";

        var results = await _dbConnection.QueryAsync<(string name, string description, decimal pointAmount)>(sql);

        return results.Any() ? results.Select(pc => new PointCategoryDto(pc.name, pc.description, pc.pointAmount)) : new List<PointCategoryDto>();
    }

    public async Task<TeamPointsDto> FindTeamPointsAsync(int teamId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        var parameters = new DynamicParameters();
        parameters.Add("TeamId", teamId);

        string lockedSql = @"
    SELECT (ut.locked_date IS NOT NULL) AS is_locked
        FROM team_tactics.user_team AS ut
        WHERE ut.id = @TeamId";

        bool locked = await _dbConnection.QuerySingleAsync<bool>(lockedSql, parameters);

        if (!locked) return new TeamPointsDto(0);

        string sql = @"
    SELECT SUM(COALESCE(pc.point_amount * mpp.occurrences, 0))
    FROM team_tactics.player_user_team put
    JOIN team_tactics.match_player_point mpp 
    	ON put.player_id = mpp.player_id
    JOIN team_tactics.point_category pc 
    	ON mpp.point_category_id = pc.id
    WHERE put.user_team_id = @TeamId
        AND pc.active = true";

        decimal pointSum = await _dbConnection.QuerySingleOrDefaultAsync<decimal>(sql, parameters);
        return new TeamPointsDto(pointSum);
    }

    public async Task<IEnumerable<PointResultDto>> GetPointResultFromMatchIdAsync(int matchId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @$"
    SELECT
    c.name,
	p.first_name || ' ' || p.last_name,
	pc2.""name"",
	mpp.occurrences,
    pc2.point_amount,
	(pc2.point_amount * mpp.occurrences) as ""totalPoints""
	FROM team_tactics.match_result as mr 
	JOIN team_tactics.match_player_point as mpp 
		ON mpp.match_result_id = mr.id
	JOIN team_tactics.point_category as pc2
		ON pc2.id = mpp.point_category_id
	JOIN team_tactics.player as p 
		ON mpp.player_id = p.id
	JOIN team_tactics.player_contract as pc
		ON pc.player_id = p.id
	JOIN team_tactics.club as c 
		ON c.id  = pc.club_id AND 
			(mr.home_club_id = c.id OR mr.away_club_id = c.id)
	WHERE mr.id = MatchId;
";

        var parameters = new DynamicParameters();
        parameters.Add("MatchId", matchId);

        var results = await _dbConnection.QueryAsync<(string clubName, string playerName, string pointCategoryName, int occurrences, decimal pointAmount, decimal totalPoints)>(sql, parameters);

        return results.Select(r => new PointResultDto(r.clubName, r.playerName, r.pointCategoryName, r.occurrences, r.pointAmount, r.totalPoints));
    }

    public async Task<IEnumerable<PointResultDto>> GetPointResultFromTeamIdAsync(int teamId)
    {
        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        string sql = @$"
    SELECT
	c.name AS club_name,
	p.first_name || ' ' || p.last_name AS ""playerName"",
    pc2.""name"" AS point_category_name,
    SUM(mpp.occurrences) AS occurrences,
    pc2.point_amount AS point_amount,
    (pc2.point_amount * SUM(mpp.occurrences)) AS ""totalPoints""
FROM team_tactics.user_team AS uteam
JOIN team_tactics.player_user_team AS put ON put.user_team_id = uteam.id
JOIN team_tactics.player AS p ON p.id = put.player_id
LEFT JOIN team_tactics.user_tournament AS utourn ON utourn.id = uteam.user_tournament_id
LEFT JOIN team_tactics.competition AS comp ON comp.id = utourn.competition_id
LEFT JOIN team_tactics.player_contract AS pc ON pc.player_id = p.id
LEFT JOIN team_tactics.club AS c ON c.id = pc.club_id
JOIN team_tactics.match_result AS mr 
    ON (mr.home_club_id = c.id OR mr.away_club_id = c.id) 
    AND mr.competition_id = utourn.competition_id 
    AND DATE(mr.""timestamp"") >= DATE(uteam.locked_date)
JOIN team_tactics.match_player_point AS mpp 
    ON mpp.player_id = p.id 
    AND mpp.match_result_id = mr.id
JOIN team_tactics.point_category AS pc2 ON pc2.id = mpp.point_category_id
WHERE uteam.id = 27
GROUP BY 
    pc2.""name"",
    p.first_name, 
    p.last_name,
    c.name,
    pc2.point_amount,
    mr.""timestamp"",
    comp.start_date
ORDER BY 
    c.name, 
    p.first_name || ' ' || p.last_name, 
    pc2.""name"";
";


        //Temp change to base off competiton startDate
        sql = @$"
    SELECT
	c.name AS club_name,
	p.first_name || ' ' || p.last_name AS ""playerName"",
    pc2.""name"" AS point_category_name,
    SUM(mpp.occurrences) AS occurrences,
    pc2.point_amount AS point_amount,
    (pc2.point_amount * SUM(mpp.occurrences)) AS ""totalPoints""
FROM team_tactics.user_team AS uteam
JOIN team_tactics.player_user_team AS put ON put.user_team_id = uteam.id
JOIN team_tactics.player AS p ON p.id = put.player_id
LEFT JOIN team_tactics.user_tournament AS utourn ON utourn.id = uteam.user_tournament_id
LEFT JOIN team_tactics.competition AS comp ON comp.id = utourn.competition_id
LEFT JOIN team_tactics.player_contract AS pc ON pc.player_id = p.id
LEFT JOIN team_tactics.club AS c ON c.id = pc.club_id
JOIN team_tactics.match_result AS mr 
    ON (mr.home_club_id = c.id OR mr.away_club_id = c.id) 
    AND mr.competition_id = utourn.competition_id 
    AND DATE(mr.""timestamp"") >= DATE(comp.start_date)
JOIN team_tactics.match_player_point AS mpp 
    ON mpp.player_id = p.id 
    AND mpp.match_result_id = mr.id
JOIN team_tactics.point_category AS pc2 ON pc2.id = mpp.point_category_id
WHERE uteam.id = @TeamId
GROUP BY 
    pc2.""name"",
    p.first_name, 
    p.last_name,
    c.name,
    pc2.point_amount,
    mr.""timestamp"",
    comp.start_date
ORDER BY 
    c.name, 
    p.first_name || ' ' || p.last_name, 
    pc2.""name"";
";

        var parameters = new DynamicParameters();
        parameters.Add("TeamId", teamId);

        var results = await _dbConnection.QueryAsync<(string clubName, string playerName,  string pointCategoryName, int occurrences, decimal pointAmount, decimal totalPoints)>(sql, parameters);

        return results.Any() ? results.Select(r => new PointResultDto(r.clubName, r.playerName, r.pointCategoryName, r.occurrences, r.pointAmount, r.totalPoints)) : new List<PointResultDto>();
    }

    public Task<IEnumerable<PointCategory>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<PointCategory?> FindByIdAsync(int id)
    {
        throw new NotImplementedException();
    }
}

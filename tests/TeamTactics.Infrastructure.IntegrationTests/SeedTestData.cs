
using Dapper;
using System.Data;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Users;
using TeamTactics.Fixtures;

namespace TeamTactics.Infrastructure.IntegrationTests
{
    public static class SeedTestData
    {
        #region Seed User
        public static async Task<int> SeedUserAsync(this IDbConnection dbConnection)
        {
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();

            User user = new UserFaker()
                .RuleFor(u => u.Id, (int)default)
                .Generate();

            var faker = new Faker();
            var parameters = new DynamicParameters();
            parameters.Add("Email", user.Email);
            parameters.Add("Username", user.Username);
            parameters.Add("Salt", user.SecurityInfo.Salt);
            parameters.Add("PasswordHash", Convert.ToBase64String(faker.Random.Bytes(32)));

            string sql = @"
                INSERT INTO team_tactics.user_account (email, password_hash, username, salt)
                VALUES (@Email, @PasswordHash, @Username, @Salt)
                RETURNING id";

            int id = await dbConnection.QuerySingleAsync<int>(sql, parameters);
            return id;
        }

        #endregion

        #region Seed Competition
        public static async Task<int> SeedCompetitonAsync(this IDbConnection dbConnection)
        {
            Competition competition = new CompetitionFaker()
                .Generate();
            return await SeedCompetitonAsync(dbConnection, competition);
        }

        public static async Task<int> SeedCompetitonAsync(this IDbConnection dbConnection, Competition competition)
        {
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("Name", competition.Name);
            parameters.Add("StartDate", competition.StartDate);
            parameters.Add("EndDate", competition.EndDate);
            parameters.Add("ExternalId", Guid.NewGuid().ToString().Substring(0, 10));

            string sql = @"
                INSERT INTO team_tactics.competition (name, start_date, end_date, external_id)
                VALUES (@Name, @StartDate, @EndDate, @ExternalId)
                RETURNING id";

            int id = await dbConnection.QuerySingleAsync<int>(sql, parameters);
            return id;
        }
        #endregion

        #region Seed Team 
        public async Task<int> 

        #endregion
    }
}

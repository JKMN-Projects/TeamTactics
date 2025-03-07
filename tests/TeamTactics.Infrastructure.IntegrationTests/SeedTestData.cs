using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Users;

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
            parameters.Add("Salt", "Salt");
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

        #region Seed Players
        public static async Task<int> SeedPlayer(this IDbConnection dbConnection)
        {
            Player player = new PlayerFaker()
                .Generate();
            return await SeedPlayer(dbConnection, player);
        }

        public static async Task<int> SeedPlayer(this IDbConnection dbConnection, Player player)
        {
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();

            using var transaction = dbConnection.BeginTransaction();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("FirstName", player.FirstName);
                parameters.Add("LastName", player.LastName);
                parameters.Add("ExternalId", player.ExternalId);
                parameters.Add("PositionId", player.PositionId);
                parameters.Add("DateOfBirth", player.BirthDate);
                string sql = @"
                    INSERT INTO team_tactics.player (first_name, last_name, player_position_id, birthdate, external_id)
                    VALUES (@FirstName, @LastName, @PositionId, @DateOfBirth, @ExternalId)
                    RETURNING id";
                int id = await dbConnection.QuerySingleAsync<int>(sql, parameters);
                player.SetId(id);

                List<string> contractValues = [];
                var contractParameters = new DynamicParameters();
                for (int i = 0; i < player.PlayerContracts.Count; i++)
                {
                    var contract = player.PlayerContracts.ElementAt(i);
                    contractValues.Add($"(@ClubId{i}, @PlayerId{i}, @Active{i})");
                    contractParameters.Add($"ClubId{i}", contract.ClubId);
                    contractParameters.Add($"PlayerId{i}", id);
                    contractParameters.Add($"Active{i}", contract.Active);
                }
                string contractSql = $@"
                    INSERT INTO team_tactics.player_contract (club_id, player_id, active)
                    VALUES {string.Join(',', contractValues)}";

                await dbConnection.ExecuteAsync(contractSql, contractParameters);
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
        #endregion

        #region Seed Clubs
        public static async Task<int> SeedClub(this IDbConnection dbConnection)
        {
            Club club = new ClubFaker()
                .Generate();
            return await SeedClub(dbConnection, club);
        }

        public static async Task<int> SeedClub(this IDbConnection dbConnection, Club club)
        {
            if (dbConnection.State != ConnectionState.Open)
                dbConnection.Open();

            var parameters = new DynamicParameters();
            parameters.Add("Name", club.Name);
            parameters.Add("ExternalId", club.ExternalId);

            string sql = @"
                INSERT INTO team_tactics.club (name, external_id)
                VALUES (@Name, @ExternalId)
                RETURNING id";

            int id = await dbConnection.QuerySingleAsync<int>(sql, parameters);
            club.SetId(id);
            return id;
        }

        public static async Task<IEnumerable<Club>> SeedClubs(this IDbConnection dbConnection, int count)
        {
            List<Club> clubIds = new List<Club>();
            for (int i = 0; i < count; i++)
            {
                Club club = new ClubFaker()
                    .Generate();
                int id = await SeedClub(dbConnection, club);
                clubIds.Add(club);
            }
            return clubIds;
        }
        #endregion
    }
}

using TeamTactics.Domain.Clubs;
using TeamTactics.Domain.Competitions;
using TeamTactics.Domain.Players;
using TeamTactics.Domain.Users;

namespace TeamTactics.Infrastructure.IntegrationTests
{
    public sealed class DataSeeder
    {
        private readonly IDbConnection _dbConnection;

        public DataSeeder(IDbConnection dbConnection)
        {
            this._dbConnection = dbConnection;
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
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

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
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();


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
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

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
            List<Club> clubs = new List<Club>();
            for (int i = 0; i < count; i++)
            {
                Club club = new ClubFaker()
                    .Generate();
                await SeedClubAsync(club);
                clubs.Add(club);
            }
            return clubs;
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
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

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
                Player player = new PlayerFaker(clubs: club)
                    .Generate();
                await SeedPlayerAsync(player);
                players.Add(player);
            }
            return players;
        }
        #endregion
    }

    public static class DataSeederExtensions
    {
        public sealed record CompetitionSeedResult(
            Competition Competition,
            IReadOnlyCollection<ClubSeedResult> Clubs
            );
        public sealed record ClubSeedResult(
            Club Club,
            IReadOnlyCollection<Player> Players
            );

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
    }
}

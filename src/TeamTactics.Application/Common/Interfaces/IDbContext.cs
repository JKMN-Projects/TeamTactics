
using TeamTactics.Application.Competitions;
using TeamTactics.Application.Matches;
using TeamTactics.Application.Players;
using TeamTactics.Application.Points;
using TeamTactics.Application.Teams;
using TeamTactics.Application.Tournaments;
using TeamTactics.Application.Users;

namespace TeamTactics.Application.Common.Interfaces
{
    public interface IDbContext
    {
        ICompetitionRepository Competitions { get; }
        IMatchRepository Matches { get; }
        IPlayerRepository Players { get; }
        IPointsRepository Points { get; }
        ITeamRepository Teams { get; }
        ITournamentRepository Tournaments { get; }
        IUserRepository Users { get; }

        public void Commit();
        public void Rollback();
    }

    public interface IUnitOfWork
    {
        public ITransaction BeginTransaction();

        public void Commit();
        public void Rollback();
    }

    public interface ITransaction
    {
    }
}

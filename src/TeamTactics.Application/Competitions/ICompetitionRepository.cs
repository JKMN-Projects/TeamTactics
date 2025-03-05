using TeamTactics.Application.Common.Interfaces;
using TeamTactics.Domain.Competitions;

namespace TeamTactics.Application.Competitions
{
    public interface ICompetitionRepository : IRepository<Competition, int>
    {
    }
}
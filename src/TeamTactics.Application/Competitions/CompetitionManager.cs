using TeamTactics.Domain.Competitions;

namespace TeamTactics.Application.Competitions;

public sealed partial class CompetitionManager
{
    private readonly ICompetitionRepository _competitionRepository;

    public CompetitionManager(ICompetitionRepository competitionRepository)
    {
        _competitionRepository = competitionRepository;
    }

    public async Task<IEnumerable<CompetitionDto>> GetAllCompetitionsAsync()
    {
        var competitions = await _competitionRepository.FindAllAsync();

        return competitions.Select(c => new CompetitionDto
        {
            Id = c.Id,
            Name = c.Name,
            StartDate = c.StartDate,
            EndDate = c.EndDate
        });
    }
}
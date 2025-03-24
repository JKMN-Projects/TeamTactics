namespace TeamTactics.Application.Points;

public sealed class PointsManager
{
    private readonly IPointsRepository _pointsRepository;

    public PointsManager(IPointsRepository pointsRepository)
    {
        _pointsRepository = pointsRepository;
    }

    public async Task<IEnumerable<PointCategoryDto>> GetActivePointCategoriesAsync()
    {
        return await _pointsRepository.FindAllActiveAsync();
    }

    public async Task<IEnumerable<PointResultDto>> GetMatchPoints(int matchId)
    {
        return await _pointsRepository.GetPointResultFromMatchIdAsync(matchId);
    }

}

namespace TeamTactics.Application.Common.Interfaces
{
    public interface IRepository<TModel, TId>
        where TId : notnull
    {
        public Task<IEnumerable<TModel>> FindAllAsync();
        public Task<TModel?> FindByIdAsync(TId id);
    }
}

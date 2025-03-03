namespace TeamTactics.Application.Common.Interfaces
{
    public interface ICrudRepository<TModel, TId> : IRepository<TModel, TId>
        where TId : notnull
    {
        public Task<TId> InsertAsync(TModel model);
        public Task UpdateAsync(TModel model);
        public Task RemoveAsync(TId id);
    }
}

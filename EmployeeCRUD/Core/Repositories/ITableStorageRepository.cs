namespace Core.Repositories
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage.Table;

    #endregion

    public interface ITableStorageRepository<TEntity>
        where TEntity : TableEntity
    {
        IQueryable<TEntity> Entities { get; }

        CloudTable TableClient { get; }

        Task AddAsync(TEntity entity);

        Task AddAsync(List<TEntity> entities);

        Task DeleteAsync(TEntity entity);

        Task<TEntity> GetAsync(string partitionKey, string rowKey);
        

        Task<IList<TableResult>> RemoveAllAsync(List<TEntity> entities);

        Task<IList<TableResult>> RemoveAllAsync(IQueryable<TEntity> entities);

        Task<IList<TableResult>> RemoveAllAsync(string partitionKey);

        Task<TableResult> RemoveAsync(string partitionKey, string rowKey);

        Task<TableResult> RemoveAsync(TEntity entity);

        Task<TableResult> UpdateAsync(TEntity entity);
    }
}

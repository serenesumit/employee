using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Repositories;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Table;

    #endregion

    public abstract class BaseTableStorageRepository<TEntity> : ITableStorageRepository<TEntity>
        where TEntity : TableEntity
    {
        private static readonly object InitializeTablesLock = new object();

        private string _connectionString;

        private CloudTable _tableClient;

        private string _tableName;

        private bool _tablesInitialized;

        private TableRequestOptions _webUiRetryPolicy;

        protected BaseTableStorageRepository()
        {
        }

        public abstract IQueryable<TEntity> Entities { get; }

        public CloudTable TableClient
        {
            get
            {
                return this._tableClient;
            }
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            try
            {
                TableOperation insertOperation = TableOperation.Insert(entity);

                await this._tableClient.ExecuteAsync(insertOperation);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public virtual async Task AddAsync(List<TEntity> entities)
        {
            TableBatchOperation batchOperation = new TableBatchOperation();

            // Create a customer entity and add it to the table.
            foreach (var entity in entities)
            {
                batchOperation.Insert(entity);
            }

            await this._tableClient.ExecuteBatchAsync(batchOperation);
        }

        public abstract Task DeleteAsync(TEntity entity);

        public virtual async Task<TEntity> GetAsync(string partitionKey, string rowKey)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);
            TableResult retrievedResult = await this.TableClient.ExecuteAsync(retrieveOperation);
            return retrievedResult.Result as TEntity;
        }

        public abstract IQueryable<TEntity> GetAll(string partitionKey);

        

        public async Task ProvisionTable(string connectionString, string tableName)
        {
            this._connectionString = connectionString;
            this._tableName = tableName;

            var storageAccount = CloudStorageAccount.Parse(this._connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();

            this._tableClient = tableClient.GetTableReference(this._tableName);
            await this._tableClient.CreateIfNotExistsAsync();
        }

        public virtual async Task<IList<TableResult>> RemoveAllAsync(string partitionKey)
        {
            var batchOperation = new TableBatchOperation();

            // We need to pass at least one property to project or else 
            // all properties will be fetch in the operation
            var projectionQuery =
                new TableQuery<DynamicTableEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey))
                    .Select(new[] { "RowKey" });

            foreach (var e in this.TableClient.ExecuteQuery(projectionQuery))
            {
                batchOperation.Delete(e);
            }

            var result = await this.TableClient.ExecuteBatchAsync(batchOperation);
            return result;
        }

        public virtual async Task<IList<TableResult>> RemoveAllAsync(List<TEntity> entities)
        {
            if (entities == null || entities.Count == 0)
            {
                return new List<TableResult>();
            }

            var batchOperation = new TableBatchOperation();
            foreach (var e in entities)
            {
                batchOperation.Delete(e);
            }

            var result = await this.TableClient.ExecuteBatchAsync(batchOperation);
            return result;
        }

        public virtual async Task<IList<TableResult>> RemoveAllAsync(IQueryable<TEntity> entities)
        {
            var batchOperation = new TableBatchOperation();
            foreach (var e in entities)
            {
                batchOperation.Delete(e);
            }

            if (batchOperation.Count > 0)
            {
                var result = await this.TableClient.ExecuteBatchAsync(batchOperation);
                return result;
            }

            return new List<TableResult>();
        }

        public virtual async Task<TableResult> RemoveAsync(string partitionKey, string rowKey)
        {
            var entity = new DynamicTableEntity(partitionKey, rowKey) { ETag = "*" };

            var result = await this.TableClient.ExecuteAsync(TableOperation.Delete(entity));
            return result;
        }

        public virtual async Task<TableResult> RemoveAsync(TEntity entity)
        {
            var result = await this._tableClient.ExecuteAsync(TableOperation.Delete(entity));
            return result;
        }

        public virtual async Task<TableResult> UpdateAsync(TEntity entity)
        {
            TableOperation replaceOperation = TableOperation.Replace(entity);

            var result = await this._tableClient.ExecuteAsync(replaceOperation);
            return result;
        }

        protected string GetTableNameForIdea(Guid ideaId)
        {
            var tableName = "idea" + ideaId.ToString("N");
            return tableName;
        }

        protected void InitializeTableForIdea(string connectionString, Guid ideaId)
        {
            var tableName = this.GetTableNameForIdea(ideaId);
            this.InitializeTables(connectionString, tableName);
        }

        protected void InitializeTables(string connectionString, string tableName)
        {
            try
            {
                this._connectionString = connectionString;
                this._tableName = tableName;

                var storageAccount = CloudStorageAccount.Parse(this._connectionString);
                var tableClient = storageAccount.CreateCloudTableClient();

                this._tableClient = tableClient.GetTableReference(this._tableName);
                this._tableClient.CreateIfNotExists();
                this._webUiRetryPolicy = new TableRequestOptions()
                {
                    MaximumExecutionTime = TimeSpan.FromSeconds(1.5),
                    RetryPolicy =
                                                     new LinearRetry(TimeSpan.FromSeconds(3), 3)
                };
            }
            catch (Exception ex)
            {
                //// this._textLogger.Log(ex);
            }
        }

        private void InitializeTablesWithLock(string connectionString, string tableName)
        {
            if (!this._tablesInitialized)
            {
                lock (InitializeTablesLock)
                {
                    if (!this._tablesInitialized)
                    {
                        this.InitializeTables(connectionString, tableName);
                        this._tablesInitialized = true;
                    }
                }
            }
        }
    }
}

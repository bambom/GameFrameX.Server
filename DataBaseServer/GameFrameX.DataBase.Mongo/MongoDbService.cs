﻿using System.Linq.Expressions;
using GameFrameX.DataBase.State;
using GameFrameX.Extension;
using GameFrameX.Log;
using GameFrameX.Utility;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GameFrameX.DataBase.Mongo
{
    /// <summary>
    /// MongoDB服务连接类，实现了 <see cref="IGameDbService"/> 接口。
    /// </summary>
    public sealed class MongoDbService : IGameDbService
    {
        /// <summary>
        /// 获取或设置MongoDB客户端。
        /// </summary>
        public MongoClient Client { get; private set; }

        /// <summary>
        /// 获取或设置当前使用的MongoDB数据库。
        /// </summary>
        public IMongoDatabase CurrentDatabase { get; private set; }

        /// <summary>
        /// 打开MongoDB连接并指定URL和数据库名称。
        /// </summary>
        /// <param name="url">MongoDB连接URL。</param>
        /// <param name="dbName">要使用的数据库名称。</param>
        public void Open(string url, string dbName)
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString(url);
                Client          = new MongoClient(settings);
                CurrentDatabase = Client.GetDatabase(dbName);
                LogHelper.Info($"初始化MongoDB服务完成 Url:{url} DbName:{dbName}");
            }
            catch (Exception)
            {
                LogHelper.Error($"初始化MongoDB服务失败 Url:{url} DbName:{dbName}");
                throw;
            }
        }

        /// <summary>
        /// 获取指定类型的MongoDB集合。
        /// </summary>
        /// <typeparam name="TState">文档的类型。</typeparam>
        /// <param name="settings">集合的设置。</param>
        /// <returns>指定类型的MongoDB集合。</returns>
        private IMongoCollection<TState> GetCollection<TState>(MongoCollectionSettings settings = null) where TState : class, ICacheState, new()
        {
            var collectionName = typeof(TState).Name;
            var collection     = CurrentDatabase.GetCollection<TState>(collectionName, settings);
            return collection;
        }

        /// <summary>
        /// 获取指定类型的MongoDB集合。
        /// </summary>
        /// <param name="collectionName">集合名称。</param>
        /// <param name="settings">集合的设置。</param>
        /// <returns>指定类型的MongoDB集合。</returns>
        private IMongoCollection<BsonDocument> GetCollection(string collectionName, MongoCollectionSettings settings = null)
        {
            var collection = CurrentDatabase.GetCollection<BsonDocument>(collectionName, settings);
            return collection;
        }

        #region 插入

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        public async Task<long> AddAsync<TState>(TState state) where TState : class, ICacheState, new()
        {
            var collection = GetCollection<TState>();
            state.CreateTime = TimeHelper.UnixTimeSeconds();
            var filter = Builders<TState>.Filter.Eq(CacheState.UniqueId, state.Id);
            var result = await collection.ReplaceOneAsync(filter, state, ReplaceOptions);
            return result.ModifiedCount;
        }

        /// <summary>
        /// 增加一个列表数据
        /// </summary>
        /// <param name="states"></param>
        /// <typeparam name="TState"></typeparam>
        public async Task AddListAsync<TState>(IEnumerable<TState> states) where TState : class, ICacheState, new()
        {
            var collection  = GetCollection<TState>();
            var cacheStates = states.ToList();
            foreach (var cacheState in cacheStates)
            {
                cacheState.CreateTime = TimeHelper.UnixTimeSeconds();
            }

            await collection.InsertManyAsync(cacheStates);
        }

        /*
        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state">数据</param>
        public void InsertOne<TState>(TState state) where TState : ICacheState, new()
        {
            GetCollection<TState>().InsertOne(state);
        }*/

        /*/// <summary>
        /// 插入一条数据
        /// </summary>
        /// <param name="collectionName">表名</param>
        /// <param name="doc">文档</param>
        public void InsertOne(string collectionName, BsonDocument doc)
        {
            GetCollection(collectionName).InsertOne(doc);
        }*/

        /*
        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="t">数据</param>
        public Task InsertOneAsync<TState>(TState t) where TState : ICacheState, new()
        {
            return GetCollection<TState>().InsertOneAsync(t);
        }
        */

        /*
        /// <summary>
        /// 插入一条数据
        /// </summary>
        /// <param name="collectionName">表名</param>
        /// <param name="doc">文档</param>
        public Task InsertOneAsync(string collectionName, BsonDocument doc)
        {
            return GetCollection(collectionName).InsertOneAsync(doc);
        }*/

        /*/// <summary>
        /// 插入多条数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="list">集合</param>
        public void InsertMany<TState>(IEnumerable<TState> list) where TState : ICacheState, new()
        {
            GetCollection<TState>().InsertMany(list);
        }*/

        /*
        /// <summary>
        /// 插入多条数据
        /// </summary>
        /// <param name="collectionName">表名</param>
        /// <param name="list">Bson集合</param>
        public void InsertMany(string collectionName, IEnumerable<BsonDocument> list)
        {
            GetCollection(collectionName).InsertMany(list);
        }*/

        /*/// <summary>
        /// 插入多条数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="list">集合</param>
        public Task InsertManyAsync<TState>(IEnumerable<TState> list) where TState : ICacheState, new()
        {
            return GetCollection<TState>().InsertManyAsync(list);
        }*/

        /*
        /// <summary>
        /// 插入多条数据
        /// </summary>
        /// <param name="collectionName">表名</param>
        /// <param name="list">Bson集合</param>
        public Task InsertManyAsync(string collectionName, IEnumerable<BsonDocument> list)
        {
            return GetCollection(collectionName).InsertManyAsync(list);
        }*/

        /*
        /// <summary>
        /// 大批量插入数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="list">数据集合</param>
        /// <returns></returns>
        public List<WriteModel<TState>> BulkInsert<TState>(IEnumerable<WriteModel<TState>> list) where TState : ICacheState, new()
        {
            var result = GetCollection<TState>().BulkWrite(list);
            return result.ProcessedRequests.ToList();
        }

        /// <summary>
        /// 大批量插入数据
        /// </summary>
        /// <param name="collectionName">表名</param>
        /// <param name="list">Bson数据集合</param>
        /// <returns></returns>
        public List<WriteModel<BsonDocument>> BulkInsert(string collectionName, IEnumerable<WriteModel<BsonDocument>> list)
        {
            var result = GetCollection(collectionName).BulkWrite(list);
            return result.ProcessedRequests.ToList();
        }

        /// <summary>
        /// 大批量插入数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="list">数据集合</param>
        /// <returns></returns>
        public async Task<List<WriteModel<TState>>> BulkInsertAsync<TState>(IEnumerable<WriteModel<TState>> list) where TState : ICacheState, new()
        {
            var result = await GetCollection<TState>().BulkWriteAsync(list);
            return result.ProcessedRequests.ToList();
        }

        /// <summary>
        /// 大批量插入数据
        /// </summary>
        /// <param name="collectionName">表名</param>
        /// <param name="list">Bson数据集合</param>
        /// <returns></returns>
        public async Task<List<WriteModel<BsonDocument>>> BulkInsertAsync(string collectionName, IEnumerable<WriteModel<BsonDocument>> list)
        {
            var result = await GetCollection(collectionName).BulkWriteAsync(list);
            return result.ProcessedRequests.ToList();
        }*/

        #endregion 插入

        #region 更新

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="filter">条件</param>
        /// <param name="update">更新的数据</param>
        /// <param name="upsert">如果它不存在是否插入文档</param>
        /// <returns></returns>
        public UpdateResult UpdateOne<TState>(Expression<Func<TState, bool>> filter, UpdateDefinition<TState> update, bool upsert = true) where TState : class, ICacheState, new()
        {
            return GetCollection<TState>().UpdateOne(filter, update, new UpdateOptions()
                                                                     {
                                                                         IsUpsert = upsert
                                                                     });
        }

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新的数据</param>
        /// <param name="upsert">如果它不存在是否插入文档</param>
        /// <returns></returns>
        public UpdateResult UpdateOne(string collectionName, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update, bool upsert)
        {
            return GetCollection(collectionName).UpdateOne(filter, update, new UpdateOptions()
                                                                           {
                                                                               IsUpsert = upsert
                                                                           });
        }

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="filter">条件</param>
        /// <param name="update">更新的数据</param>
        /// <param name="upsert">如果它不存在是否插入文档</param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdateOneAsync<TState>(Expression<Func<TState, bool>> filter, UpdateDefinition<TState> update, bool upsert) where TState : class, ICacheState, new()
        {
            return await GetCollection<TState>().UpdateOneAsync(filter, update, new UpdateOptions()
                                                                                {
                                                                                    IsUpsert = upsert
                                                                                });
        }

        /// <summary>
        /// 修改一条数据
        /// </summary>
        /// <param name="collectionName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新的数据</param>
        /// <param name="upsert">如果它不存在是否插入文档</param>
        /// <returns></returns>
        public async Task<UpdateResult> UpdateOneAsync(string collectionName, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update, bool upsert)
        {
            return await GetCollection(collectionName).UpdateOneAsync(filter, update, new UpdateOptions()
                                                                                      {
                                                                                          IsUpsert = upsert
                                                                                      });
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <param name="filter">修改条件</param>
        /// <param name="update">修改结果</param>
        /// <param name="upsert">是否插入新文档（filter条件满足就更新，否则插入新文档）</param>
        /// <returns></returns>
        public long UpdateMany<TState>(Expression<Func<TState, bool>> filter, UpdateDefinition<TState> update, bool upsert = false) where TState : class, ICacheState, new()
        {
            UpdateResult result = GetCollection<TState>().UpdateMany(filter, update, new UpdateOptions
                                                                                     {
                                                                                         IsUpsert = upsert
                                                                                     });
            return result.ModifiedCount;
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">修改条件</param>
        /// <param name="update">修改结果</param>
        /// <param name="upsert">是否插入新文档（filter条件满足就更新，否则插入新文档）</param>
        /// <returns></returns>
        public long UpdateMany(string collName, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update, bool upsert = false)
        {
            UpdateResult result = GetCollection(collName).UpdateMany(filter, update, new UpdateOptions
                                                                                     {
                                                                                         IsUpsert = upsert
                                                                                     });
            return result.ModifiedCount;
        }

        /// <summary>
        /// 修改多个文档
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="filter">修改条件</param>
        /// <param name="update">修改结果</param>
        /// <param name="upsert">是否插入新文档（filter条件满足就更新，否则插入新文档）</param>
        /// <returns></returns>
        public async Task<long> UpdateManyAsync<TState>(Expression<Func<TState, bool>> filter, UpdateDefinition<TState> update, bool upsert = false) where TState : class, ICacheState, new()
        {
            UpdateResult result = await GetCollection<TState>().UpdateManyAsync(filter, update, new UpdateOptions
                                                                                                {
                                                                                                    IsUpsert = upsert
                                                                                                });
            return result.ModifiedCount;
        }

        /// <summary>
        /// 修改多个文档
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">修改条件</param>
        /// <param name="update">修改结果</param>
        /// <param name="upsert">是否插入新文档（filter条件满足就更新，否则插入新文档）</param>
        /// <returns></returns>
        public async Task<long> UpdateManyAsync(string collName, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update, bool upsert = false)
        {
            UpdateResult result = await GetCollection(collName).UpdateManyAsync(filter, update, new UpdateOptions
                                                                                                {
                                                                                                    IsUpsert = upsert
                                                                                                });
            return result.ModifiedCount;
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新后的数据</param>
        /// <returns></returns>
        public TState UpdateOne<TState>(string collName, Expression<Func<TState, bool>> filter, UpdateDefinition<TState> update) where TState : class, ICacheState, new()
        {
            TState result = GetCollection<TState>().FindOneAndUpdate(filter, update);
            return result;
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新后的Bson数据</param>
        /// <returns></returns>
        public BsonDocument UpdateOne(string collName, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update)
        {
            BsonDocument result = GetCollection(collName).FindOneAndUpdate(filter, update);
            return result;
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="filter">条件</param>
        /// <param name="update">更新后的数据</param>
        /// <returns></returns>
        public async Task<TState> UpdateOneAsync<TState>(Expression<Func<TState, bool>> filter, UpdateDefinition<TState> update) where TState : class, ICacheState, new()
        {
            TState result = await GetCollection<TState>().FindOneAndUpdateAsync(filter, update);
            return result;
        }

        /// <summary>
        /// 修改文档
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <param name="update">更新后的Bson数据</param>
        /// <returns></returns>
        public async Task<BsonDocument> UpdateOneAsync(string collName, Expression<Func<BsonDocument, bool>> filter, UpdateDefinition<BsonDocument> update)
        {
            BsonDocument result = await GetCollection(collName).FindOneAndUpdateAsync(filter, update);
            return result;
        }

        #endregion 更新

        #region 删除

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collectionName">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public long Delete<TState>(string collectionName, BsonDocument document)
        {
            DeleteResult result = CurrentDatabase.GetCollection<TState>(collectionName).DeleteOne(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public long DeleteMany<TState>(BsonDocument document) where TState : class, ICacheState, new()
        {
            DeleteResult result = GetCollection<TState>().DeleteMany(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collectionName">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public long Delete(string collectionName, BsonDocument document)
        {
            DeleteResult result = GetCollection(collectionName).DeleteOne(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collectionName">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public long DeleteMany(string collectionName, BsonDocument document)
        {
            DeleteResult result = GetCollection(collectionName).DeleteMany(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync<TState>(BsonDocument document) where TState : class, ICacheState, new()
        {
            DeleteResult result = await GetCollection<TState>().DeleteOneAsync(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync<TState>(BsonDocument document) where TState : class, ICacheState, new()
        {
            DeleteResult result = await GetCollection<TState>().DeleteManyAsync(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collectionName">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(string collectionName, BsonDocument document)
        {
            DeleteResult result = await GetCollection(collectionName).DeleteOneAsync(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按BsonDocument条件删除
        /// </summary>
        /// <param name="collectionName">集合名称</param>
        /// <param name="document">文档</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync(string collectionName, BsonDocument document)
        {
            DeleteResult result = await GetCollection(collectionName).DeleteManyAsync(document);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public long Delete<TState>(string json) where TState : class, ICacheState, new()
        {
            var result = GetCollection<TState>().DeleteOne(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public long DeleteMany<TState>(string json) where TState : class, ICacheState, new()
        {
            var result = GetCollection<TState>().DeleteMany(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public long Delete(string collName, string json)
        {
            var result = GetCollection(collName).DeleteOne(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public long DeleteMany(string collName, string json)
        {
            var result = GetCollection(collName).DeleteMany(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync<TState>(string collName, string json)
        {
            var result = await CurrentDatabase.GetCollection<TState>(collName).DeleteOneAsync(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync<TState>(string json) where TState : class, ICacheState, new()
        {
            var result = await GetCollection<TState>().DeleteManyAsync(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(string collName, string json)
        {
            var result = await GetCollection(collName).DeleteOneAsync(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按json字符串删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="json">json字符串</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync(string collName, string json)
        {
            var result = await GetCollection(collName).DeleteManyAsync(json);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public long Delete<TState>(Expression<Func<TState, bool>> predicate) where TState : class, ICacheState, new()
        {
            var result = GetCollection<TState>().DeleteOne(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public long DeleteMany<TState>(Expression<Func<TState, bool>> predicate) where TState : class, ICacheState, new()
        {
            var result = GetCollection<TState>().DeleteMany(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public long Delete(string collName, Expression<Func<BsonDocument, bool>> predicate)
        {
            var result = GetCollection(collName).DeleteOne(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public long DeleteMany(string collName, Expression<Func<BsonDocument, bool>> predicate)
        {
            var result = GetCollection(collName).DeleteMany(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync<TState>(Expression<Func<TState, bool>> predicate) where TState : class, ICacheState, new()
        {
            var result = await GetCollection<TState>().DeleteManyAsync(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(string collName, Expression<Func<BsonDocument, bool>> predicate)
        {
            var result = await GetCollection(collName).DeleteOneAsync(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按条件表达式删除
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="predicate">条件表达式</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync(string collName, Expression<Func<BsonDocument, bool>> predicate)
        {
            var result = await GetCollection(collName).DeleteManyAsync(predicate);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public long Delete<TState>(string collName, FilterDefinition<TState> filter) where TState : class, ICacheState, new()
        {
            var result = GetCollection<TState>().DeleteOne(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public long DeleteMany<TState>(FilterDefinition<TState> filter) where TState : class, ICacheState, new()
        {
            var result = GetCollection<TState>().DeleteMany(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public long Delete(string collName, FilterDefinition<BsonDocument> filter)
        {
            var result = GetCollection(collName).DeleteOne(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public long DeleteMany(string collName, FilterDefinition<BsonDocument> filter)
        {
            var result = GetCollection(collName).DeleteMany(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync<TState>(FilterDefinition<TState> filter) where TState : class, ICacheState, new()
        {
            var result = await GetCollection<TState>().DeleteOneAsync(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync<TState>(FilterDefinition<TState> filter) where TState : class, ICacheState, new()
        {
            var result = await GetCollection<TState>().DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(string collName, FilterDefinition<BsonDocument> filter)
        {
            var result = await GetCollection(collName).DeleteOneAsync(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 按检索条件删除
        /// 建议用Builders&lt;T&gt;构建复杂的查询条件
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<long> DeleteManyAsync(string collName, FilterDefinition<BsonDocument> filter)
        {
            var result = await GetCollection(collName).DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public TState DeleteOne<TState>(Expression<Func<TState, bool>> filter) where TState : class, ICacheState, new()
        {
            var result = GetCollection<TState>().FindOneAndDelete(filter);
            return result;
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public BsonDocument DeleteOne(string collName, Expression<Func<BsonDocument, bool>> filter)
        {
            var result = GetCollection(collName).FindOneAndDelete(filter);
            return result;
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<TState> DeleteOneAsync<TState>(Expression<Func<TState, bool>> filter) where TState : class, ICacheState, new()
        {
            TState result = await GetCollection<TState>().FindOneAndDeleteAsync(filter);
            return result;
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<BsonDocument> DeleteOneAsync(string collName, Expression<Func<BsonDocument, bool>> filter)
        {
            var result = await GetCollection(collName).FindOneAndDeleteAsync(filter);
            return result;
        }

        #endregion 删除

        #region 查询

        /*/// <summary>
        /// 查询，复杂查询直接用Linq处理
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <returns>要查询的对象</returns>
        public IMongoQueryable<TState> GetQueryable<TState>(string collName)
        {
            return CurrentDatabase.GetCollection<TState>(collName).AsQueryable();
        }*/

        /*
        /// <summary>
        /// 查询，复杂查询直接用Linq处理
        /// </summary>
        /// <param name="collName">集合名称</param>
        /// <returns>要查询的对象</returns>
        public IMongoQueryable<BsonDocument> GetQueryable(string collName)
        {
            return GetCollection(collName).AsQueryable();
        }*/

        /*
        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public TState Get<TState>(FilterDefinition<TState> filter) where TState : ICacheState, new()
        {
            var find = GetCollection<TState>().Find(filter);
            return find.FirstOrDefault();
        }*/

        /*/// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public BsonDocument Get(string collName, FilterDefinition<BsonDocument> filter)
        {
            var find = GetCollection(collName).Find(filter);
            return find.FirstOrDefault();
        }*/

        /*/// <summary>
        /// 获取一条数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<TState> GetAsync<TState>(string collName, FilterDefinition<TState> filter) where TState : ICacheState, new()
        {
            var find = await GetCollection<TState>().FindAsync(filter);
            return await find.FirstOrDefaultAsync();
        }*/

        /*
        /// <summary>
        /// 获取一条数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<BsonDocument> GetAsync(string collName, FilterDefinition<BsonDocument> filter)
        {
            var find = await GetCollection(collName).FindAsync(filter);
            return await find.FirstOrDefaultAsync();
        }
        */

        /*
        /// <summary>
        /// 获取多条数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public IEnumerable<TState> GetMany<TState>(string collName, FilterDefinition<TState> filter) where TState : ICacheState, new()
        {
            var find = GetCollection<TState>().Find(filter);
            return find.ToEnumerable();
        }*/

        /*
        /// <summary>
        /// 获取多条数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public IEnumerable<BsonDocument> GetMany(string collName, FilterDefinition<BsonDocument> filter)
        {
            var find = GetCollection(collName).Find(filter);
            return find.ToEnumerable();
        }
        */

        /*
        /// <summary>
        /// 获取多条数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<IEnumerable<TState>> GetManyAsync<TState>(string collName, FilterDefinition<TState> filter) where TState : ICacheState, new()
        {
            var find = await GetCollection<TState>().FindAsync(filter);
            return find.ToEnumerable();
        }
        */

        /*
        /// <summary>
        /// 获取多条数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<IEnumerable<BsonDocument>> GetManyAsync(string collName, FilterDefinition<BsonDocument> filter)
        {
            var find = await GetCollection(collName).FindAsync(filter);
            return find.ToEnumerable();
        }
        */

        /// <summary>
        /// 判断是否存在符合条件的数据
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public bool Any<TState>(Expression<Func<TState, bool>> filter) where TState : class, ICacheState, new()
        {
            filter = GetDefaultFindExpression(filter);
            var find = GetCollection<TState>().Find(filter);
            return find.Any();
        }

        /*
        /// <summary>
        /// 判断是否存在符合条件的数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public bool Any(string collName, FilterDefinition<BsonDocument> filter)
        {
            var find = GetCollection(collName).Find(filter);
            return find.Any();
        }
        */

        /// <summary>
        /// 判断是否存在符合条件的数据
        /// </summary>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<bool> AnyAsync<TState>(Expression<Func<TState, bool>> filter) where TState : class, ICacheState, new()
        {
            filter = GetDefaultFindExpression(filter);
            var find = await GetCollection<TState>().FindAsync(filter);
            return await find.AnyAsync();
        }

        /*
        /// <summary>
        /// 判断是否存在符合条件的数据
        /// </summary>
        /// <param name="collName">表名</param>
        /// <param name="filter">条件</param>
        /// <returns></returns>
        public async Task<bool> AnyAsync(string collName, FilterDefinition<BsonDocument> filter)
        {
            var find = await GetCollection(collName).FindAsync(filter);
            return await find.AnyAsync();
        }
        */

        #endregion 查询

        #region 索引

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="collectionName">集合名</param>
        /// <param name="index">索引键</param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public string CreateIndex(string collectionName, string index, bool asc = true)
        {
            var mgr  = GetCollection(collectionName).Indexes;
            var list = mgr.List();
            while (list.MoveNext())
            {
                if (!list.Current.Any(doc => doc["name"].AsString.StartsWith(index)))
                {
                    return mgr.CreateOne(new CreateIndexModel<BsonDocument>(asc ? Builders<BsonDocument>.IndexKeys.Ascending(doc => doc[index]) : Builders<BsonDocument>.IndexKeys.Descending(doc => doc[index])));
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="collectionName">集合名</param>
        /// <param name="index">索引键</param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public async Task<string> CreateIndexAsync(string collectionName, string index, bool asc = true)
        {
            var mgr  = GetCollection(collectionName).Indexes;
            var list = await mgr.ListAsync();
            while (await list.MoveNextAsync())
            {
                if (!list.Current.Any(doc => doc["name"].AsString.StartsWith(index)))
                {
                    return await mgr.CreateOneAsync(new CreateIndexModel<BsonDocument>(asc ? Builders<BsonDocument>.IndexKeys.Ascending(doc => doc[index]) : Builders<BsonDocument>.IndexKeys.Descending(doc => doc[index])));
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="collectionName">集合名</param>
        /// <param name="index">索引键</param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public string UpdateIndex(string collectionName, string index, bool asc = true)
        {
            var mgr = GetCollection(collectionName).Indexes;
            return mgr.CreateOne(new CreateIndexModel<BsonDocument>(asc ? Builders<BsonDocument>.IndexKeys.Ascending(doc => doc[index]) : Builders<BsonDocument>.IndexKeys.Descending(doc => doc[index])));
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="collectionName">集合名</param>
        /// <param name="index">索引键</param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public async Task<string> UpdateIndexAsync(string collectionName, string index, bool asc = true)
        {
            var mgr = GetCollection(collectionName).Indexes;
            return await mgr.CreateOneAsync(new CreateIndexModel<BsonDocument>(asc ? Builders<BsonDocument>.IndexKeys.Ascending(doc => doc[index]) : Builders<BsonDocument>.IndexKeys.Descending(doc => doc[index])));
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="collectionName">集合名</param>
        /// <param name="index">索引键</param>
        /// <returns></returns>
        public void DropIndex(string collectionName, string index)
        {
            GetCollection(collectionName).Indexes.DropOne(index);
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="collectionName">集合名</param>
        /// <param name="index">索引键</param>
        /// <returns></returns>
        public Task DropIndexAsync(string collectionName, string index)
        {
            return GetCollection(collectionName).Indexes.DropOneAsync(index);
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="index">索引键</param>
        /// <param name="key"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public string CreateIndex<TState>(string index, Expression<Func<TState, object>> key, bool asc = true) where TState : class, ICacheState, new()
        {
            var mgr  = GetCollection<TState>().Indexes;
            var list = mgr.List();
            while (list.MoveNext())
            {
                if (!list.Current.Any(doc => doc["name"].AsString.StartsWith(index)))
                {
                    return mgr.CreateOne(new CreateIndexModel<TState>(asc ? Builders<TState>.IndexKeys.Ascending(key) : Builders<TState>.IndexKeys.Descending(key)));
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="index">索引键</param>
        /// <param name="key"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public async Task<string> CreateIndexAsync<TState>(string index, Expression<Func<TState, object>> key, bool asc = true) where TState : class, ICacheState, new()
        {
            var mgr  = GetCollection<TState>().Indexes;
            var list = await mgr.ListAsync();
            while (await list.MoveNextAsync())
            {
                if (!list.Current.Any(doc => doc["name"].AsString.StartsWith(index)))
                {
                    return await mgr.CreateOneAsync(new CreateIndexModel<TState>(asc ? Builders<TState>.IndexKeys.Ascending(key) : Builders<TState>.IndexKeys.Descending(key)));
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="key"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public string UpdateIndex<TState>(Expression<Func<TState, object>> key, bool asc = true) where TState : class, ICacheState, new()
        {
            var mgr = GetCollection<TState>().Indexes;
            return mgr.CreateOne(new CreateIndexModel<TState>(asc ? Builders<TState>.IndexKeys.Ascending(key) : Builders<TState>.IndexKeys.Descending(key)));
        }

        /// <summary>
        /// 更新索引
        /// </summary>
        /// <param name="key"></param>
        /// <param name="asc"></param>
        /// <returns></returns>
        public async Task<string> UpdateIndexAsync<TState>(Expression<Func<TState, object>> key, bool asc = true) where TState : class, ICacheState, new()
        {
            var mgr = GetCollection<TState>().Indexes;
            return await mgr.CreateOneAsync(new CreateIndexModel<TState>(asc ? Builders<TState>.IndexKeys.Ascending(key) : Builders<TState>.IndexKeys.Descending(key)));
        }

        #endregion 索引

        /// <summary>
        /// 加载指定ID的缓存状态。
        /// </summary>
        /// <typeparam name="TState">缓存状态的类型。</typeparam>
        /// <param name="id">要加载的缓存状态的ID。</param>
        /// <param name="defaultGetter">默认值获取器。</param>
        /// <returns>加载的缓存状态。</returns>
        public async Task<TState> LoadState<TState>(long id, Func<TState> defaultGetter = null) where TState : class, ICacheState, new()
        {
            var filter = Builders<TState>.Filter.Eq(BaseCacheState.UniqueId, id);

            var       col    = GetCollection<TState>();
            using var cursor = await col.FindAsync(filter);
            var       state  = await cursor.FirstOrDefaultAsync();
            bool      isNew  = state == null;
            if (state == null && defaultGetter != null)
            {
                state = defaultGetter();
            }

            if (state == null)
            {
                state = new TState { Id = id };
            }

            state.AfterLoadFromDb(isNew);
            return state;
        }

        /// <summary>
        /// 获取默认的查询表达式。
        /// </summary>
        /// <typeparam name="TState">缓存状态的类型。</typeparam>
        /// <param name="filter">自定义查询表达式。</param>
        /// <returns>默认的查询表达式。</returns>
        private static Expression<Func<TState, bool>> GetDefaultFindExpression<TState>(Expression<Func<TState, bool>> filter) where TState : class, ICacheState, new()
        {
            Expression<Func<TState, bool>> expression = m => m.IsDeleted == false;
            if (filter != null)
            {
                expression = expression.And(filter);
            }

            return expression;
        }

        /// <summary>
        /// 异步查找满足指定条件的缓存状态列表。
        /// </summary>
        /// <typeparam name="TState">缓存状态的类型。</typeparam>
        /// <param name="filter">查询条件。</param>
        /// <returns>满足条件的缓存状态列表。</returns>
        public async Task<List<TState>> FindListAsync<TState>(Expression<Func<TState, bool>> filter) where TState : class, ICacheState, new()
        {
            var       result     = new List<TState>();
            var       collection = GetCollection<TState>();
            using var cursor     = await collection.FindAsync<TState>(GetDefaultFindExpression(filter));
            while (await cursor.MoveNextAsync())
            {
                result.AddRange(cursor.Current);
            }

            return result;
        }

        /// <summary>
        /// 异步查找满足指定条件的缓存状态。
        /// </summary>
        /// <typeparam name="TState">缓存状态的类型。</typeparam>
        /// <param name="filter">查询条件。</param>
        /// <returns>满足条件的缓存状态。</returns>
        public async Task<TState> FindAsync<TState>(Expression<Func<TState, bool>> filter) where TState : class, ICacheState, new()
        {
            var       collection       = GetCollection<TState>();
            var       findExpression   = GetDefaultFindExpression(filter);
            var       filterDefinition = Builders<TState>.Filter.Where(findExpression);
            using var cursor           = await collection.FindAsync<TState>(filterDefinition);
            var       state            = await cursor.FirstOrDefaultAsync();
            return state;
        }

        /// <summary>
        /// 以升序方式查找符合条件的第一个元素。
        /// </summary>
        /// <typeparam name="TState">实现ICacheState接口的类型。</typeparam>
        /// <param name="filter">过滤表达式。</param>
        /// <param name="sortExpression">排序字段表达式。</param>
        /// <returns>符合条件的第一个元素。</returns>
        public async Task<TState> FindSortAscendingFirstOneAsync<TState>(Expression<Func<TState, bool>> filter, Expression<Func<TState, object>> sortExpression) where TState : class, ICacheState, new()
        {
            var collection     = GetCollection<TState>();
            var findExpression = GetDefaultFindExpression(filter);
            var sortDefinition = Builders<TState>.Sort.Ascending(sortExpression);
            var cursor         = collection.Aggregate().Match(findExpression).Sort(sortDefinition).Limit(1);
            var state          = await cursor.FirstOrDefaultAsync();
            return state;
        }

        /// <summary>
        /// 以降序方式查找符合条件的第一个元素。
        /// </summary>
        /// <typeparam name="TState">实现ICacheState接口的类型。</typeparam>
        /// <param name="filter">过滤表达式。</param>
        /// <param name="sortExpression">排序字段表达式。</param>
        /// <returns>符合条件的第一个元素。</returns>
        public async Task<TState> FindSortDescendingFirstOneAsync<TState>(Expression<Func<TState, bool>> filter, Expression<Func<TState, object>> sortExpression) where TState : class, ICacheState, new()
        {
            var collection     = GetCollection<TState>();
            var findExpression = GetDefaultFindExpression(filter);
            var sortDefinition = Builders<TState>.Sort.Descending(sortExpression);
            var cursor         = collection.Aggregate().Match(findExpression).Sort(sortDefinition).Limit(1);
            var state          = await cursor.FirstOrDefaultAsync();
            return state;
        }

        /// <summary>
        /// 以降序方式查找符合条件的元素并进行分页。
        /// </summary>
        /// <typeparam name="TState">实现ICacheState接口的类型。</typeparam>
        /// <param name="filter">过滤表达式。</param>
        /// <param name="sortExpression">排序字段表达式。</param>
        /// <param name="pageIndex">页码，从0开始。</param>
        /// <param name="pageSize">每页数量，默认为10。</param>
        /// <returns>符合条件的元素列表。</returns>
        public async Task<List<TState>> FindSortDescendingAsync<TState>(Expression<Func<TState, bool>> filter, Expression<Func<TState, object>> sortExpression, long pageIndex = 0, long pageSize = 10) where TState : class, ICacheState, new()
        {
            if (pageIndex < 0)
            {
                pageIndex = 0;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var result         = new List<TState>();
            var collection     = GetCollection<TState>();
            var findExpression = GetDefaultFindExpression(filter);
            var sortDefinition = Builders<TState>.Sort.Descending(sortExpression);
            var cursor         = await collection.Aggregate().Match(findExpression).Sort(sortDefinition).Skip(pageIndex * pageSize).Limit(pageSize).ToCursorAsync();
            while (await cursor.MoveNextAsync())
            {
                result.AddRange(cursor.Current);
            }

            return result;
        }

        /// <summary>
        /// 以升序方式查找符合条件的元素并进行分页。
        /// </summary>
        /// <typeparam name="TState">实现ICacheState接口的类型。</typeparam>
        /// <param name="filter">过滤表达式。</param>
        /// <param name="sortExpression">排序字段表达式。</param>
        /// <param name="pageIndex">页码，从0开始。</param>
        /// <param name="pageSize">每页数量，默认为10。</param>
        /// <returns>符合条件的元素列表。</returns>
        public async Task<List<TState>> FindSortAscendingAsync<TState>(Expression<Func<TState, bool>> filter, Expression<Func<TState, object>> sortExpression, long pageIndex = 0, long pageSize = 10) where TState : class, ICacheState, new()
        {
            if (pageIndex < 0)
            {
                pageIndex = 0;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var result         = new List<TState>();
            var collection     = GetCollection<TState>();
            var findExpression = GetDefaultFindExpression(filter);
            var sortDefinition = Builders<TState>.Sort.Ascending(sortExpression);
            var cursor         = await collection.Aggregate().Match(findExpression).Sort(sortDefinition).Skip(pageIndex * pageSize).Limit(pageSize).ToCursorAsync();
            while (await cursor.MoveNextAsync())
            {
                result.AddRange(cursor.Current);
            }

            return result;
        }

        /// <summary>
        /// 查询数据长度
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public async Task<long> CountAsync<TState>(Expression<Func<TState, bool>> filter) where TState : class, ICacheState, new()
        {
            var collection = GetCollection<TState>();
            var newFilter  = GetDefaultFindExpression(filter);
            var count      = await collection.CountDocumentsAsync<TState>(newFilter);
            return count;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="filter">查询条件</param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public async Task<long> DeleteAsync<TState>(Expression<Func<TState, bool>> filter) where TState : class, ICacheState, new()
        {
            var collection = GetCollection<TState>();
            var state      = await FindAsync(filter);
            var newFilter  = Builders<TState>.Filter.Eq(BaseCacheState.UniqueId, state.Id);
            state.DeleteTime = TimeHelper.UnixTimeMilliseconds();
            state.IsDeleted  = true;
            var result = await collection.ReplaceOneAsync(newFilter, state, ReplaceOptions);
            return result.ModifiedCount;
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        public async Task<long> DeleteAsync<TState>(TState state) where TState : class, ICacheState, new()
        {
            var filter     = Builders<TState>.Filter.Eq(BaseCacheState.UniqueId, state.Id);
            var collection = GetCollection<TState>();
            state.DeleteTime = TimeHelper.UnixTimeMilliseconds();
            state.IsDeleted  = true;
            var result = await collection.ReplaceOneAsync(filter, state, ReplaceOptions);
            return result.ModifiedCount;
        }


        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public async Task<TState> UpdateAsync<TState>(TState state) where TState : class, ICacheState, new()
        {
            var isChanged = state.IsModify;
            if (isChanged)
            {
                state.UpdateTime = TimeHelper.UnixTimeMilliseconds();
                state.UpdateCount++;
                var filter     = Builders<TState>.Filter.Eq(BaseCacheState.UniqueId, state.Id);
                var collection = GetCollection<TState>();
                var result     = await collection.ReplaceOneAsync(filter, state, ReplaceOptions);
                if (result.IsAcknowledged)
                {
                    state.AfterSaveToDb();
                }
            }

            return state;
        }

        /// <summary>
        /// 替换选项，用于替换文档。设置 <see cref="IsUpsert"/> 属性为 true 可以在找不到匹配的文档时插入新文档。
        /// </summary>
        public static readonly ReplaceOptions ReplaceOptions = new() { IsUpsert = true };

        /// <summary>
        /// 更新选项，用于更新文档。设置 <see cref="IsUpsert"/> 属性为 true 可以在找不到匹配的文档时插入新文档。
        /// </summary>
        public static readonly UpdateOptions UpdateOptions = new() { IsUpsert = true };

        /// <summary>
        /// 批量写入选项，用于批量写入文档。设置 <see cref="IsOrdered"/> 属性为 false 可以并行执行写入操作。
        /// </summary>
        public static readonly BulkWriteOptions BulkWriteOptions = new() { IsOrdered = false };

        /// <summary>
        /// 创建指定字段的索引。
        /// </summary>
        /// <typeparam name="TState">缓存状态的类型。</typeparam>
        /// <param name="indexKey">要创建索引的字段。</param>
        /// <returns>表示异步操作的任务。</returns>
        public Task CreateIndex<TState>(string indexKey) where TState : CacheState, new()
        {
            var collection = GetCollection<TState>();
            var key        = Builders<TState>.IndexKeys.Ascending(indexKey);
            var model      = new CreateIndexModel<TState>(key);
            return collection.Indexes.CreateOneAsync(model);
        }

        /// <summary>
        /// 关闭MongoDB连接。
        /// </summary>
        public void Close()
        {
            Client.Cluster.Dispose();
        }
    }
}
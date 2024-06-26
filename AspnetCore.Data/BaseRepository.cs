using AspnetCore.Data.Entities;
using AspnetCore.Utilities.Constans;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AspnetCore.Data;

public class BaseRepository<TEntity> where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _dbSet;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BaseRepository(DataContext context, IHttpContextAccessor httpContextAccessor)
    {
        _dbSet = context.Set<TEntity>();
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual IQueryable<TEntity> QueryAll => _dbSet;

    public virtual IQueryable<TEntity> Query => _dbSet.Where(x => x.IsDeleted == false);

    public virtual async Task<TEntity> GetByIdAsync(int id) =>
        await Query.Where(x => x.Id == id).FirstOrDefaultAsync();

    public virtual TEntity Insert(TEntity entity)
    {
        entity.CreatedUserId = GetExecutorId();
        _dbSet.Add(entity);
        return entity;
    }

    public virtual List<TEntity> Insert(List<TEntity> entities)
    {
        var executorId = GetExecutorId();
        entities = entities.Select(x =>
        {
            x.CreatedUserId = executorId;
            return x;
        }).ToList();
        _dbSet.AddRange(entities);
        return entities;
    }

    public virtual async Task<TEntity> InsertAsync(TEntity entity)
    {
        entity.CreatedUserId = GetExecutorId();
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual async Task<List<TEntity>> InsertAsync(List<TEntity> entities)
    {
        var executorId = GetExecutorId();
        entities = entities.Select(x =>
        {
            x.CreatedUserId = executorId;
            return x;
        }).ToList();
        await _dbSet.AddRangeAsync(entities);
        return entities;
    }

    public virtual TEntity Update(TEntity entity)
    {
        entity.ModifiedUserId = GetExecutorId();
        entity.ModifiedDate = DateTime.Now;
        _dbSet.Update(entity);
        return entity;
    }

    public virtual List<TEntity> Update(List<TEntity> entities)
    {
        var executorId = GetExecutorId();
        entities = entities.Select(x =>
        {
            x.ModifiedUserId = executorId;
            x.ModifiedDate = DateTime.Now;
            return x;
        }).ToList();
        _dbSet.UpdateRange(entities);
        return entities;
    }

    public virtual TEntity Delete(TEntity entity)
    {
        entity.IsDeleted = true;
        entity.DeletedUserId = GetExecutorId();
        entity.DeletedDate = DateTime.Now;
        _dbSet.Update(entity);
        return entity;
    }

    public virtual List<TEntity> Delete(List<TEntity> entities)
    {
        var executorId = GetExecutorId();
        entities = entities.Select(x =>
        {
            x.IsDeleted = true;
            x.DeletedUserId = executorId;
            x.DeletedDate = DateTime.Now;
            return x;
        }).ToList();
        _dbSet.UpdateRange(entities);
        return entities;
    }

    private int? GetExecutorId()
    {
        int? creatorId = null;
        var httpContextUserId = _httpContextAccessor.HttpContext?.Items[SystemConstant.ExecutorIdKey];
        if (httpContextUserId != null)
            creatorId = (int?)httpContextUserId;
        return creatorId;
    }
}
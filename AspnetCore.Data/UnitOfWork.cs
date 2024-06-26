using AspnetCore.Data.Entities;
using AspnetCore.Data.Models;
using AspnetCore.Utilities.Constans;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AspnetCore.Data;

public class UnitOfWork : IDisposable
{
    private readonly DataContext _context;
    private IDbContextTransaction _contextTransaction;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UnitOfWork(DataContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    #region Core
    public bool SaveChanges()
    {
        int? createdUserId = null;
        var httpContextUserId = _httpContextAccessor.HttpContext?.Items[SystemConstant.ExecutorIdKey];
        if (httpContextUserId != null)
            createdUserId = (int?)httpContextUserId;
        var auditLogCreateDto = new AuditLogCreatingModel
        {
            Method = _httpContextAccessor.HttpContext?.Request.Method ?? string.Empty,
            CreatedUserId = createdUserId
        };
        return _context.SaveChanges(auditLogCreateDto) > 0;
    }

    public async Task<bool> SaveChangesAsync()
    {
        int? createdUserId = null;
        var httpContextUserId = _httpContextAccessor.HttpContext?.Items[SystemConstant.ExecutorIdKey];
        if (httpContextUserId != null)
            createdUserId = (int?)httpContextUserId;
        var auditLogCreateDto = new AuditLogCreatingModel
        {
            Method = _httpContextAccessor.HttpContext?.Request.Method ?? string.Empty,
            CreatedUserId = createdUserId
        };
        return await _context.SaveChangesAsync(auditLogCreateDto) > 0;
    }

    public async Task<List<T>> ExecuteStoredProcedureAsync<T>(string procedureName, object parametersObject) where T : class, new()
    {
        _context.Database.OpenConnection();

        var connection = _context.Database.GetDbConnection();
        var result = await connection.QueryAsync<T>(procedureName, parametersObject, commandType: System.Data.CommandType.StoredProcedure);

        _context.Database.CloseConnection();

        return result.ToList();
    }

    public UnitOfWork BeginTransaction()
    {
        _contextTransaction = _context.Database.BeginTransaction();
        return this;
    }

    public void EndTransaction()
    {
        SaveChanges();
        _contextTransaction.Commit();
    }

    public async Task EndTransactionAsync()
    {
        await SaveChangesAsync();
        _contextTransaction.Commit();
    }

    public void RollBack()
    {
        _contextTransaction.Rollback();
        Dispose();
    }
    #endregion

    #region Dispose
    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                _contextTransaction?.Dispose();
                _context.Dispose();
            }
        }
        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion

    private BaseRepository<UserCredential> userCredentialRepository;
    public BaseRepository<UserCredential> UserCredentialRepository
    {
        get
        {
            userCredentialRepository ??= new BaseRepository<UserCredential>(_context, _httpContextAccessor);
            return userCredentialRepository;
        }
    }
}
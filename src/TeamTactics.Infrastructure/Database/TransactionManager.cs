using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTactics.Infrastructure.Database;
public class TransactionManager : ITransactionManager, IDisposable
{
    private IDbConnection _connection;
    private IDbTransaction _transaction;

    public TransactionManager(IDbConnection connection)
    {
        _connection = connection;
    }

    public IDbConnection Connection => _connection;
    public bool HasActiveTransaction => _transaction != null;

    public async Task<IDbTransaction> BeginTransactionAsync()
    {
        if (_transaction != null)
            return _transaction;

        if (_connection.State != ConnectionState.Open)
            _connection.Open();

        _transaction = _connection.BeginTransaction();

        return _transaction;
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction to commit");

        _transaction.Commit();
        _transaction = null;

        await CleanupAsync();
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null)
            return;

        _transaction.Rollback();
        _transaction = null;

        await CleanupAsync();
    }

    private async Task CleanupAsync()
    {
        if (_connection != null)
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();

            _connection.Dispose();
            _connection = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection?.Dispose();
    }
}

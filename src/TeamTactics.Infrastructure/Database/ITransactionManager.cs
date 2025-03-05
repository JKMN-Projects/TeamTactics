using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTactics.Infrastructure.Database;

public interface ITransactionManager
{
    Task<IDbTransaction> BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    bool HasActiveTransaction { get; }
    IDbConnection Connection { get; }
}


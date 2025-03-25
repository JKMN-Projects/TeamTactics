using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTactics.Infrastructure.Database.TypeHandlers;
class DateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override DateTime Parse(object value)
    {
        // Convert database datetime to UTC DateTime
        if (value is DateTime dbDateTime)
        {
            // If the Kind is unspecified, assume it's UTC (common with databases)
            if (dbDateTime.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(dbDateTime, DateTimeKind.Utc);
            }
            // If it's already Local, convert to UTC
            else if (dbDateTime.Kind == DateTimeKind.Local)
            {
                return dbDateTime.ToUniversalTime();
            }
            // If it's already UTC, return as is
            return dbDateTime;
        }

        throw new InvalidCastException($"Unable to convert {value?.GetType().Name ?? "null"} to DateTime");
    }

    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        // Always store UTC dates in the database
        parameter.Value = value.Kind == DateTimeKind.Utc
            ? value
            : value.ToUniversalTime();
    }
}

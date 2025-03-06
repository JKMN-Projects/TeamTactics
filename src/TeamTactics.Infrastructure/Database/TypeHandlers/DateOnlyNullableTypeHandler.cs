using Dapper;
using System.Data;

namespace TeamTactics.Infrastructure.Database.TypeHandlers;

class DateOnlyNullableTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value)
    {
        return value == null || value is DBNull
            ? null
            : DateOnly.FromDateTime((DateTime)value);
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly? value)
    {
        parameter.Value = value.HasValue
            ? value.Value.ToDateTime(TimeOnly.MinValue)
            : DBNull.Value;
    }
}

namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Functions of statement of work exception from mapping.</summary>
public static class StatementOfWorkExceptionFromMapping
{
    public static Dictionary<string, double> SummarizeStatementOfWorkMappingsForException(List<StatementOfWorkMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<StatementOfWorkException> DeriveStatementOfWorkExceptionsFromMapping(List<StatementOfWorkMapping> items)
    {
        var outValue = new List<StatementOfWorkException>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkException.NewStatementOfWorkException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankStatementOfWorkException(StatementOfWorkException item)
    {
        return StatementOfWorkMappingPolicy.ClassifyStatementOfWorkMapping(item.MeasuredValue());
    }
}

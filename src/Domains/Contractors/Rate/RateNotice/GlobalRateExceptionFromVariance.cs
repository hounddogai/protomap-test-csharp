namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>Functions of global rate exception from variance.</summary>
public static class GlobalRateExceptionFromVariance
{
    public static Dictionary<string, double> SummarizeRollingStatementOfWorkVariancesForException(List<RollingStatementOfWorkVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<GlobalRateException> DeriveGlobalRateExceptionsFromVariance(List<RollingStatementOfWorkVariance> items)
    {
        var outValue = new List<GlobalRateException>();
        foreach (var item in items)
        {
            var derived = GlobalRateException.NewGlobalRateException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeGlobalRateException(GlobalRateException item)
    {
        return StatementOfWorkCyclePolicy.NormalizeStatementOfWorkCycleCode(item.Code);
    }
}

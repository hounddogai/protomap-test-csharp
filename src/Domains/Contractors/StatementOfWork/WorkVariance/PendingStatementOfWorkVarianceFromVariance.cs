namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Functions of pending statement of work variance from variance.</summary>
public static class PendingStatementOfWorkVarianceFromVariance
{
    public static Dictionary<string, double> SummarizeScheduledStatementOfWorkVariancesForVariance(List<ScheduledStatementOfWorkVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<PendingStatementOfWorkVariance> DerivePendingStatementOfWorkVariancesFromVariance(List<ScheduledStatementOfWorkVariance> items)
    {
        var outValue = new List<PendingStatementOfWorkVariance>();
        foreach (var item in items)
        {
            var derived = PendingStatementOfWorkVariance.NewPendingStatementOfWorkVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimatePendingStatementOfWorkVariance(PendingStatementOfWorkVariance item)
    {
        return ScheduledStatementOfWorkVariancePolicy.NormalizeScheduledStatementOfWorkVarianceCode(item.Code);
    }
}

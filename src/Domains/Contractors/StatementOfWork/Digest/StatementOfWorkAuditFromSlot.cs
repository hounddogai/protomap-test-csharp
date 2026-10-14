namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Functions of statement of work audit from slot.</summary>
public static class StatementOfWorkAuditFromSlot
{
    public static Dictionary<string, double> SummarizeRateSlotsForAudit(List<RateSlot> items)
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

    public static List<StatementOfWorkAudit> DeriveStatementOfWorkAuditsFromSlot(List<RateSlot> items)
    {
        var outValue = new List<StatementOfWorkAudit>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkAudit.NewStatementOfWorkAudit(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceStatementOfWorkAudit(StatementOfWorkAudit item)
    {
        return RateWindowPolicy.ClassifyRateWindow(item.WeightedTotal());
    }
}

namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>Functions of primary engagement summary from summary.</summary>
public static class PrimaryEngagementSummaryFromSummary
{
    public static Dictionary<string, double> SummarizeStatementOfWorkSummariesForSummary(List<StatementOfWorkSummary> items)
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

    public static List<PrimaryEngagementSummary> DerivePrimaryEngagementSummariesFromSummary(List<StatementOfWorkSummary> items)
    {
        var outValue = new List<PrimaryEngagementSummary>();
        foreach (var item in items)
        {
            var derived = PrimaryEngagementSummary.NewPrimaryEngagementSummary(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalancePrimaryEngagementSummary(PrimaryEngagementSummary item)
    {
        return StatementOfWorkQuotaPolicy.NormalizeStatementOfWorkQuotaCode(item.Code);
    }
}

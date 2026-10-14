namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Contractors.Engagement.Score;

/// <summary>Functions of default statement of work audit from score.</summary>
public static class DefaultStatementOfWorkAuditFromScore
{
    public static Dictionary<string, double> SummarizeGlobalEngagementScoresForAudit(List<GlobalEngagementScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<DefaultStatementOfWorkAudit> DeriveDefaultStatementOfWorkAuditsFromScore(List<GlobalEngagementScore> items)
    {
        var outValue = new List<DefaultStatementOfWorkAudit>();
        foreach (var item in items)
        {
            var derived = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleDefaultStatementOfWorkAudit(DefaultStatementOfWorkAudit item)
    {
        return DefaultEngagementBudgetPolicy.DefaultEngagementBudgetWeight(6, true);
    }
}

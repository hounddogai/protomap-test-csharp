namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Contractors.StatementOfWork.Variance;

/// <summary>Functions of default engagement budget from rollup.</summary>
public static class DefaultEngagementBudgetFromRollup
{
    public static Dictionary<string, double> SummarizeStatementOfWorkRollupsForBudget(List<StatementOfWorkRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<DefaultEngagementBudget> DeriveDefaultEngagementBudgetsFromRollup(List<StatementOfWorkRollup> items)
    {
        var outValue = new List<DefaultEngagementBudget>();
        foreach (var item in items)
        {
            var derived = DefaultEngagementBudget.NewDefaultEngagementBudget(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreDefaultEngagementBudget(DefaultEngagementBudget item)
    {
        return StatementOfWorkVariancePolicy.StatementOfWorkVarianceWeight(9, true);
    }
}

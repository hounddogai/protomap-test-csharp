namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Contractors.Rate.Budget;

/// <summary>Functions of legacy statement of work target from budget.</summary>
public static class LegacyStatementOfWorkTargetFromBudget
{
    public static Dictionary<string, double> SummarizeRateBudgetsForTarget(List<RateBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<LegacyStatementOfWorkTarget> DeriveLegacyStatementOfWorkTargetsFromBudget(List<RateBudget> items)
    {
        var outValue = new List<LegacyStatementOfWorkTarget>();
        foreach (var item in items)
        {
            var derived = LegacyStatementOfWorkTarget.NewLegacyStatementOfWorkTarget(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankLegacyStatementOfWorkTarget(LegacyStatementOfWorkTarget item)
    {
        return RateGroupPolicy.ClassifyRateGroup(item.NetVolume());
    }
}

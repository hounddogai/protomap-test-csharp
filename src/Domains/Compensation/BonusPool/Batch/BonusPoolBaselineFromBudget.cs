namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Functions of bonus pool baseline from budget.</summary>
public static class BonusPoolBaselineFromBudget
{
    public static Dictionary<string, double> SummarizeEquityGrantBudgetsForBaseline(List<EquityGrantBudget> items)
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

    public static List<BonusPoolBaseline> DeriveBonusPoolBaselinesFromBudget(List<EquityGrantBudget> items)
    {
        var outValue = new List<BonusPoolBaseline>();
        foreach (var item in items)
        {
            var derived = BonusPoolBaseline.NewBonusPoolBaseline(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignBonusPoolBaseline(BonusPoolBaseline item)
    {
        return EquityGrantBaselinePolicy.EquityGrantBaselineWeight(4, true);
    }
}

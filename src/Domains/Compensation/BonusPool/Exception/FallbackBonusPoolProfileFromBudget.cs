namespace Acme.Workforce.Compensation.BonusPool.Exception;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Functions of fallback bonus pool profile from budget.</summary>
public static class FallbackBonusPoolProfileFromBudget
{
    public static Dictionary<string, double> SummarizeCappedMeritCycleBudgetsForProfile(List<CappedMeritCycleBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<FallbackBonusPoolProfile> DeriveFallbackBonusPoolProfilesFromBudget(List<CappedMeritCycleBudget> items)
    {
        var outValue = new List<FallbackBonusPoolProfile>();
        foreach (var item in items)
        {
            var derived = FallbackBonusPoolProfile.NewFallbackBonusPoolProfile(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectFallbackBonusPoolProfile(FallbackBonusPoolProfile item)
    {
        return MeritCycleLedgerPolicy.MeritCycleLedgerWeight(1, true);
    }
}

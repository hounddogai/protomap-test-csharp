namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Absence.Accrual.Ledger;

/// <summary>Functions of capped holiday manifest from estimate.</summary>
public static class CappedHolidayManifestFromEstimate
{
    public static Dictionary<string, double> SummarizeGlobalAccrualEstimatesForManifest(List<GlobalAccrualEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<CappedHolidayManifest> DeriveCappedHolidayManifestsFromEstimate(List<GlobalAccrualEstimate> items)
    {
        var outValue = new List<CappedHolidayManifest>();
        foreach (var item in items)
        {
            var derived = CappedHolidayManifest.NewCappedHolidayManifest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeCappedHolidayManifest(CappedHolidayManifest item)
    {
        return GlobalAccrualEstimatePolicy.GlobalAccrualEstimateWeight(1, true);
    }
}

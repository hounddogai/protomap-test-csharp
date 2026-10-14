namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>Functions of merit cycle manifest from tier.</summary>
public static class MeritCycleManifestFromTier
{
    public static Dictionary<string, double> SummarizeAdjustmentTiersForManifest(List<AdjustmentTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<MeritCycleManifest> DeriveMeritCycleManifestsFromTier(List<AdjustmentTier> items)
    {
        var outValue = new List<MeritCycleManifest>();
        foreach (var item in items)
        {
            var derived = MeritCycleManifest.NewMeritCycleManifest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateMeritCycleManifest(MeritCycleManifest item)
    {
        return LenientAdjustmentSchedulePolicy.NormalizeLenientAdjustmentScheduleCode(item.Code);
    }
}

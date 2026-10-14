namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Functions of bonus pool manifest from claim.</summary>
public static class BonusPoolManifestFromClaim
{
    public static Dictionary<string, double> SummarizeLenientMeritCycleClaimsForManifest(List<LenientMeritCycleClaim> items)
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

    public static List<BonusPoolManifest> DeriveBonusPoolManifestsFromClaim(List<LenientMeritCycleClaim> items)
    {
        var outValue = new List<BonusPoolManifest>();
        foreach (var item in items)
        {
            var derived = BonusPoolManifest.NewBonusPoolManifest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleBonusPoolManifest(BonusPoolManifest item)
    {
        return ManualMeritCycleOverridePolicy.ManualMeritCycleOverrideBucket(item.WeightedExposure());
    }
}

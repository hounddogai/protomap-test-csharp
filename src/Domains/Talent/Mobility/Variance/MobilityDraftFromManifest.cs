namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>Functions of mobility draft from manifest.</summary>
public static class MobilityDraftFromManifest
{
    public static Dictionary<string, double> SummarizeFallbackSuccessionPlanManifestsForDraft(List<FallbackSuccessionPlanManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<MobilityDraft> DeriveMobilityDraftsFromManifest(List<FallbackSuccessionPlanManifest> items)
    {
        var outValue = new List<MobilityDraft>();
        foreach (var item in items)
        {
            var derived = MobilityDraft.NewMobilityDraft(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileMobilityDraft(MobilityDraft item)
    {
        return PrimarySuccessionPlanRollupPolicy.NormalizePrimarySuccessionPlanRollupCode(item.Code);
    }
}

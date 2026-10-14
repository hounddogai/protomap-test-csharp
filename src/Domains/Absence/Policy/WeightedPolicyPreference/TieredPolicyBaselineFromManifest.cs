namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.ReturnPlan.PlanLedger;

/// <summary>Functions of tiered policy baseline from manifest.</summary>
public static class TieredPolicyBaselineFromManifest
{
    public static Dictionary<string, double> SummarizeLenientReturnPlanManifestsForBaseline(List<LenientReturnPlanManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<TieredPolicyBaseline> DeriveTieredPolicyBaselinesFromManifest(List<LenientReturnPlanManifest> items)
    {
        var outValue = new List<TieredPolicyBaseline>();
        foreach (var item in items)
        {
            var derived = TieredPolicyBaseline.NewTieredPolicyBaseline(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeTieredPolicyBaseline(TieredPolicyBaseline item)
    {
        return LenientReturnPlanManifestPolicy.ClassifyLenientReturnPlanManifest(item.MeasuredValue());
    }
}

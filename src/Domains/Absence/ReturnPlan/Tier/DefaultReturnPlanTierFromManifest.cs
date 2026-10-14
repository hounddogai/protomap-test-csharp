namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.PlanLedger;

/// <summary>Functions of default return plan tier from manifest.</summary>
public static class DefaultReturnPlanTierFromManifest
{
    public static Dictionary<string, double> SummarizeLenientReturnPlanManifestsForTier(List<LenientReturnPlanManifest> items)
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

    public static List<DefaultReturnPlanTier> DeriveDefaultReturnPlanTiersFromManifest(List<LenientReturnPlanManifest> items)
    {
        var outValue = new List<DefaultReturnPlanTier>();
        foreach (var item in items)
        {
            var derived = DefaultReturnPlanTier.NewDefaultReturnPlanTier(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyDefaultReturnPlanTier(DefaultReturnPlanTier item)
    {
        return LenientReturnPlanManifestPolicy.ClassifyLenientReturnPlanManifest(item.ProjectedExposure());
    }
}

namespace Acme.Workforce.Absence.ReturnPlan.PlanLedger;

using Acme.Workforce.Absence.Accrual.Checkpoint;

/// <summary>Functions of lenient return plan manifest from template.</summary>
public static class LenientReturnPlanManifestFromTemplate
{
    public static Dictionary<string, double> SummarizeStrictAccrualTemplatesForManifest(List<StrictAccrualTemplate> items)
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

    public static List<LenientReturnPlanManifest> DeriveLenientReturnPlanManifestsFromTemplate(List<StrictAccrualTemplate> items)
    {
        var outValue = new List<LenientReturnPlanManifest>();
        foreach (var item in items)
        {
            var derived = LenientReturnPlanManifest.NewLenientReturnPlanManifest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitLenientReturnPlanManifest(LenientReturnPlanManifest item)
    {
        return StrictAccrualTemplatePolicy.StrictAccrualTemplateWeight(9, true);
    }
}

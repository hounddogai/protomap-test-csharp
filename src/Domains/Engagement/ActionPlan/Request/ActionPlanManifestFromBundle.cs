namespace Acme.Workforce.Engagement.ActionPlan.Request;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Functions of action plan manifest from bundle.</summary>
public static class ActionPlanManifestFromBundle
{
    public static Dictionary<string, double> SummarizeActionPlanBundlesForManifest(List<ActionPlanBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<ActionPlanManifest> DeriveActionPlanManifestsFromBundle(List<ActionPlanBundle> items)
    {
        var outValue = new List<ActionPlanManifest>();
        foreach (var item in items)
        {
            var derived = ActionPlanManifest.NewActionPlanManifest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveActionPlanManifest(ActionPlanManifest item)
    {
        return PrimaryActionPlanSlotPolicy.NormalizePrimaryActionPlanSlotCode(item.Code);
    }
}

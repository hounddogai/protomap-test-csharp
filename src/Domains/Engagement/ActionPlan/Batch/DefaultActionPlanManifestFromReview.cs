namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Functions of default action plan manifest from review.</summary>
public static class DefaultActionPlanManifestFromReview
{
    public static Dictionary<string, double> SummarizeTownHallReviewsForManifest(List<TownHallReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<DefaultActionPlanManifest> DeriveDefaultActionPlanManifestsFromReview(List<TownHallReview> items)
    {
        var outValue = new List<DefaultActionPlanManifest>();
        foreach (var item in items)
        {
            var derived = DefaultActionPlanManifest.NewDefaultActionPlanManifest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanDefaultActionPlanManifest(DefaultActionPlanManifest item)
    {
        return LegacyTownHallSchedulePolicy.ClassifyLegacyTownHallSchedule(item.NetExposure());
    }
}

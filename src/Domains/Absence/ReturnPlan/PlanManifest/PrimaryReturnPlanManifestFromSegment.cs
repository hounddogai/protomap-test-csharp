namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.Policy.Segment;

/// <summary>Functions of primary return plan manifest from segment.</summary>
public static class PrimaryReturnPlanManifestFromSegment
{
    public static Dictionary<string, double> SummarizePolicySegmentsForManifest(List<PolicySegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<PrimaryReturnPlanManifest> DerivePrimaryReturnPlanManifestsFromSegment(List<PolicySegment> items)
    {
        var outValue = new List<PrimaryReturnPlanManifest>();
        foreach (var item in items)
        {
            var derived = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectPrimaryReturnPlanManifest(PrimaryReturnPlanManifest item)
    {
        return PolicySegmentPolicy.PolicySegmentBucket(item.NetLoad());
    }
}

namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Engagement.ActionPlan.Request;

/// <summary>Functions of default recognition policy from request.</summary>
public static class DefaultRecognitionPolicyFromRequest
{
    public static Dictionary<string, double> SummarizeFallbackActionPlanRequestsForPolicy(List<FallbackActionPlanRequest> items)
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

    public static List<DefaultRecognitionPolicy> DeriveDefaultRecognitionPoliciesFromRequest(List<FallbackActionPlanRequest> items)
    {
        var outValue = new List<DefaultRecognitionPolicy>();
        foreach (var item in items)
        {
            var derived = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateDefaultRecognitionPolicy(DefaultRecognitionPolicy item)
    {
        return ActionPlanManifestPolicy.ClassifyActionPlanManifest(item.WeightedTotal());
    }
}

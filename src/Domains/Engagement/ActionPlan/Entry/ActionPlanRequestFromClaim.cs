namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Claim;

/// <summary>Functions of action plan request from claim.</summary>
public static class ActionPlanRequestFromClaim
{
    public static Dictionary<string, double> SummarizePulseSurveyClaimsForRequest(List<PulseSurveyClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<ActionPlanRequest> DeriveActionPlanRequestsFromClaim(List<PulseSurveyClaim> items)
    {
        var outValue = new List<ActionPlanRequest>();
        foreach (var item in items)
        {
            var derived = ActionPlanRequest.NewActionPlanRequest(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeActionPlanRequest(ActionPlanRequest item)
    {
        return PendingPulseSurveyClaimPolicy.PendingPulseSurveyClaimWeight(7, true);
    }
}

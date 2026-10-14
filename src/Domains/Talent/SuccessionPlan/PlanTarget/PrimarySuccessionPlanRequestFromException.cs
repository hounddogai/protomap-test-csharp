namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Functions of primary succession plan request from exception.</summary>
public static class PrimarySuccessionPlanRequestFromException
{
    public static Dictionary<string, double> SummarizeAdaptiveCareerPathExceptionsForRequest(List<AdaptiveCareerPathException> items)
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

    public static List<PrimarySuccessionPlanRequest> DerivePrimarySuccessionPlanRequestsFromException(List<AdaptiveCareerPathException> items)
    {
        var outValue = new List<PrimarySuccessionPlanRequest>();
        foreach (var item in items)
        {
            var derived = PrimarySuccessionPlanRequest.NewPrimarySuccessionPlanRequest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitPrimarySuccessionPlanRequest(PrimarySuccessionPlanRequest item)
    {
        return AdaptiveCareerPathExceptionPolicy.AdaptiveCareerPathExceptionWeight(4, true);
    }
}

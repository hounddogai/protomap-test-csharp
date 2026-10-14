namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Functions of rolling succession plan request from override.</summary>
public static class RollingSuccessionPlanRequestFromOverride
{
    public static Dictionary<string, double> SummarizePrimaryPoolOverridesForRequest(List<PrimaryPoolOverride> items)
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

    public static List<RollingSuccessionPlanRequest> DeriveRollingSuccessionPlanRequestsFromOverride(List<PrimaryPoolOverride> items)
    {
        var outValue = new List<RollingSuccessionPlanRequest>();
        foreach (var item in items)
        {
            var derived = RollingSuccessionPlanRequest.NewRollingSuccessionPlanRequest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleRollingSuccessionPlanRequest(RollingSuccessionPlanRequest item)
    {
        return PrimaryPoolOverridePolicy.PrimaryPoolOverrideBucket(item.MeasuredValue());
    }
}

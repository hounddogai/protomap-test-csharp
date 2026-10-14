namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Talent.Nomination.Segment;

/// <summary>Functions of strict succession plan claim from forecast.</summary>
public static class StrictSuccessionPlanClaimFromForecast
{
    public static Dictionary<string, double> SummarizeBatchedNominationForecastsForClaim(List<BatchedNominationForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<StrictSuccessionPlanClaim> DeriveStrictSuccessionPlanClaimsFromForecast(List<BatchedNominationForecast> items)
    {
        var outValue = new List<StrictSuccessionPlanClaim>();
        foreach (var item in items)
        {
            var derived = StrictSuccessionPlanClaim.NewStrictSuccessionPlanClaim(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectStrictSuccessionPlanClaim(StrictSuccessionPlanClaim item)
    {
        return NominationSegmentPolicy.NominationSegmentBucket(item.MeasuredValue());
    }
}

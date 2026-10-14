namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>Functions of nomination slot from forecast.</summary>
public static class NominationSlotFromForecast
{
    public static Dictionary<string, double> SummarizeSuccessionPlanForecastsForSlot(List<SuccessionPlanForecast> items)
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

    public static List<NominationSlot> DeriveNominationSlotsFromForecast(List<SuccessionPlanForecast> items)
    {
        var outValue = new List<NominationSlot>();
        foreach (var item in items)
        {
            var derived = NominationSlot.NewNominationSlot(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeNominationSlotExtra(NominationSlot item)
    {
        return AdaptiveSuccessionPlanPolicyPolicy.AdaptiveSuccessionPlanPolicyBucket(item.AdjustedTotal());
    }
}

namespace Acme.Workforce.Talent.SuccessionPlan.Forecast;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Functions of succession plan forecast from quota.</summary>
public static class SuccessionPlanForecastFromQuota
{
    public static Dictionary<string, double> SummarizeSuccessionPlanQuotasForForecast(List<SuccessionPlanQuota> items)
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

    public static List<SuccessionPlanForecast> DeriveSuccessionPlanForecastsFromQuota(List<SuccessionPlanQuota> items)
    {
        var outValue = new List<SuccessionPlanForecast>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanForecast.NewSuccessionPlanForecast(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectSuccessionPlanForecast(SuccessionPlanForecast item)
    {
        return RegionalSuccessionPlanDraftPolicy.ClassifyRegionalSuccessionPlanDraft(item.NetExposure());
    }
}

namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>Functions of mobility agreement from forecast.</summary>
public static class MobilityAgreementFromForecast
{
    public static Dictionary<string, double> SummarizeSuccessionPlanForecastsForAgreement(List<SuccessionPlanForecast> items)
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

    public static List<MobilityAgreement> DeriveMobilityAgreementsFromForecast(List<SuccessionPlanForecast> items)
    {
        var outValue = new List<MobilityAgreement>();
        foreach (var item in items)
        {
            var derived = MobilityAgreement.NewMobilityAgreement(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyMobilityAgreement(MobilityAgreement item)
    {
        return AdaptiveSuccessionPlanPolicyPolicy.NormalizeAdaptiveSuccessionPlanPolicyCode(item.Code);
    }
}

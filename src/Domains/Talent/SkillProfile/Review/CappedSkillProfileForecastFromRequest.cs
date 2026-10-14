namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Functions of capped skill profile forecast from request.</summary>
public static class CappedSkillProfileForecastFromRequest
{
    public static Dictionary<string, double> SummarizeLenientMobilityRequestsForForecast(List<LenientMobilityRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<CappedSkillProfileForecast> DeriveCappedSkillProfileForecastsFromRequest(List<LenientMobilityRequest> items)
    {
        var outValue = new List<CappedSkillProfileForecast>();
        foreach (var item in items)
        {
            var derived = CappedSkillProfileForecast.NewCappedSkillProfileForecast(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitCappedSkillProfileForecast(CappedSkillProfileForecast item)
    {
        return DefaultMobilityRecordPolicy.DefaultMobilityRecordWeight(1, true);
    }
}

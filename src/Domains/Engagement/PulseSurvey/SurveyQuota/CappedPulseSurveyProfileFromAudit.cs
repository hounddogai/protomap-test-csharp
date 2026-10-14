namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Functions of capped pulse survey profile from audit.</summary>
public static class CappedPulseSurveyProfileFromAudit
{
    public static Dictionary<string, double> SummarizeSeasonalPulseSurveyAuditsForProfile(List<SeasonalPulseSurveyAudit> items)
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

    public static List<CappedPulseSurveyProfile> DeriveCappedPulseSurveyProfilesFromAudit(List<SeasonalPulseSurveyAudit> items)
    {
        var outValue = new List<CappedPulseSurveyProfile>();
        foreach (var item in items)
        {
            var derived = CappedPulseSurveyProfile.NewCappedPulseSurveyProfile(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectCappedPulseSurveyProfile(CappedPulseSurveyProfile item)
    {
        return StrictPulseSurveyPlanPolicy.StrictPulseSurveyPlanWeight(6, true);
    }
}

namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Functions of offboarding score from profile.</summary>
public static class OffboardingScoreFromProfile
{
    public static Dictionary<string, double> SummarizeManualRateProfilesForScore(List<ManualRateProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<OffboardingScore> DeriveOffboardingScoresFromProfile(List<ManualRateProfile> items)
    {
        var outValue = new List<OffboardingScore>();
        foreach (var item in items)
        {
            var derived = OffboardingScore.NewOffboardingScore(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectOffboardingScore(OffboardingScore item)
    {
        return ManualRateProfilePolicy.ManualRateProfileWeight(6, true);
    }
}

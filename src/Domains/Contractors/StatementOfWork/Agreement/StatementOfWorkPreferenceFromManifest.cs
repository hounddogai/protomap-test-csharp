namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Functions of statement of work preference from manifest.</summary>
public static class StatementOfWorkPreferenceFromManifest
{
    public static Dictionary<string, double> SummarizeOffboardingManifestsForPreference(List<OffboardingManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<StatementOfWorkPreference> DeriveStatementOfWorkPreferencesFromManifest(List<OffboardingManifest> items)
    {
        var outValue = new List<StatementOfWorkPreference>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkPreference.NewStatementOfWorkPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreStatementOfWorkPreference(StatementOfWorkPreference item)
    {
        return OffboardingThresholdPolicy.OffboardingThresholdBucket(item.WeightedVolume());
    }
}

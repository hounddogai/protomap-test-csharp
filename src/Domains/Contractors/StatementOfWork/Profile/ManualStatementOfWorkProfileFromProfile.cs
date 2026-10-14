namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Contractors.Offboarding.Draft;

/// <summary>Functions of manual statement of work profile from profile.</summary>
public static class ManualStatementOfWorkProfileFromProfile
{
    public static Dictionary<string, double> SummarizeCappedOffboardingProfilesForProfile(List<CappedOffboardingProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<ManualStatementOfWorkProfile> DeriveManualStatementOfWorkProfilesFromProfile(List<CappedOffboardingProfile> items)
    {
        var outValue = new List<ManualStatementOfWorkProfile>();
        foreach (var item in items)
        {
            var derived = ManualStatementOfWorkProfile.NewManualStatementOfWorkProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveManualStatementOfWorkProfile(ManualStatementOfWorkProfile item)
    {
        return CappedOffboardingProfilePolicy.CappedOffboardingProfileBucket(item.EffectiveLoad());
    }
}

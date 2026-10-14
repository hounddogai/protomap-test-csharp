namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Functions of career path profile from checkpoint.</summary>
public static class CareerPathProfileFromCheckpoint
{
    public static Dictionary<string, double> SummarizeNominationCheckpointsForProfile(List<NominationCheckpoint> items)
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

    public static List<CareerPathProfile> DeriveCareerPathProfilesFromCheckpoint(List<NominationCheckpoint> items)
    {
        var outValue = new List<CareerPathProfile>();
        foreach (var item in items)
        {
            var derived = CareerPathProfile.NewCareerPathProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCareerPathProfile(CareerPathProfile item)
    {
        return NominationVariancePolicy.NormalizeNominationVarianceCode(item.Code);
    }
}

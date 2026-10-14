namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>Functions of nomination assignment from digest.</summary>
public static class NominationAssignmentFromDigest
{
    public static Dictionary<string, double> SummarizeCareerPathDigestsForAssignment(List<CareerPathDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<NominationAssignment> DeriveNominationAssignmentsFromDigest(List<CareerPathDigest> items)
    {
        var outValue = new List<NominationAssignment>();
        foreach (var item in items)
        {
            var derived = NominationAssignment.NewNominationAssignment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeNominationAssignment(NominationAssignment item)
    {
        return CareerPathOverridePolicy.CareerPathOverrideBucket(item.AdjustedExposure());
    }
}

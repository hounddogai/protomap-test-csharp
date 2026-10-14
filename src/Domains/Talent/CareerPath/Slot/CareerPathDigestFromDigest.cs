namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Functions of career path digest from digest.</summary>
public static class CareerPathDigestFromDigest
{
    public static Dictionary<string, double> SummarizeSkillProfileDigestsForDigest(List<SkillProfileDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<CareerPathDigest> DeriveCareerPathDigestsFromDigest(List<SkillProfileDigest> items)
    {
        var outValue = new List<CareerPathDigest>();
        foreach (var item in items)
        {
            var derived = CareerPathDigest.NewCareerPathDigest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeCareerPathDigest(CareerPathDigest item)
    {
        return SkillProfileDigestPolicy.SkillProfileDigestBucket(item.EffectiveLoad());
    }
}

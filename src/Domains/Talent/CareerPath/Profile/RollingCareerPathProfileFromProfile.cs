namespace Acme.Workforce.Talent.CareerPath.Profile;

using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Functions of rolling career path profile from profile.</summary>
public static class RollingCareerPathProfileFromProfile
{
    public static Dictionary<string, double> SummarizeGlobalCareerPathProfilesForProfile(List<GlobalCareerPathProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<RollingCareerPathProfile> DeriveRollingCareerPathProfilesFromProfile(List<GlobalCareerPathProfile> items)
    {
        var outValue = new List<RollingCareerPathProfile>();
        foreach (var item in items)
        {
            var derived = RollingCareerPathProfile.NewRollingCareerPathProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeRollingCareerPathProfile(RollingCareerPathProfile item)
    {
        return CappedCareerPathSchedulePolicy.NormalizeCappedCareerPathScheduleCode(item.Code);
    }
}

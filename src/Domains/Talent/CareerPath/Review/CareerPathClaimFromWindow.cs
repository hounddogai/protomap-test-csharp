namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Talent.SkillProfile.Window;

/// <summary>Functions of career path claim from window.</summary>
public static class CareerPathClaimFromWindow
{
    public static Dictionary<string, double> SummarizeSkillProfileWindowsForClaim(List<SkillProfileWindow> items)
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

    public static List<CareerPathClaim> DeriveCareerPathClaimsFromWindow(List<SkillProfileWindow> items)
    {
        var outValue = new List<CareerPathClaim>();
        foreach (var item in items)
        {
            var derived = CareerPathClaim.NewCareerPathClaim(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeCareerPathClaim(CareerPathClaim item)
    {
        return DefaultSkillProfileSegmentPolicy.DefaultSkillProfileSegmentBucket(item.AdjustedVolume());
    }
}

namespace Acme.Workforce.Talent.SkillProfile.Mapping;

using Acme.Workforce.Talent.CareerPath.Exception;

/// <summary>Functions of regional skill profile manifest from plan.</summary>
public static class RegionalSkillProfileManifestFromPlan
{
    public static Dictionary<string, double> SummarizeBatchedCareerPathPlansForManifest(List<BatchedCareerPathPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<RegionalSkillProfileManifest> DeriveRegionalSkillProfileManifestsFromPlan(List<BatchedCareerPathPlan> items)
    {
        var outValue = new List<RegionalSkillProfileManifest>();
        foreach (var item in items)
        {
            var derived = RegionalSkillProfileManifest.NewRegionalSkillProfileManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyRegionalSkillProfileManifest(RegionalSkillProfileManifest item)
    {
        return CareerPathNoticePolicy.CareerPathNoticeWeight(9, true);
    }
}

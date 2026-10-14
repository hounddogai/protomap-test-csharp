namespace Acme.Workforce.Talent.SkillProfile.Window;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Functions of default skill profile segment from adjustment.</summary>
public static class DefaultSkillProfileSegmentFromAdjustment
{
    public static Dictionary<string, double> SummarizeSkillProfileAdjustmentsForSegment(List<SkillProfileAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<DefaultSkillProfileSegment> DeriveDefaultSkillProfileSegmentsFromAdjustment(List<SkillProfileAdjustment> items)
    {
        var outValue = new List<DefaultSkillProfileSegment>();
        foreach (var item in items)
        {
            var derived = DefaultSkillProfileSegment.NewDefaultSkillProfileSegment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchDefaultSkillProfileSegment(DefaultSkillProfileSegment item)
    {
        return SkillProfileAdjustmentPolicy.SkillProfileAdjustmentBucket(item.ProjectedValue());
    }
}

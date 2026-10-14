namespace Acme.Workforce.Talent.SkillProfile.Manifest;

using Acme.Workforce.Talent.SkillProfile.Review;

/// <summary>Functions of skill profile slot from review.</summary>
public static class SkillProfileSlotFromReview
{
    public static Dictionary<string, double> SummarizeSkillProfileReviewsForSlot(List<SkillProfileReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<SkillProfileSlot> DeriveSkillProfileSlotsFromReview(List<SkillProfileReview> items)
    {
        var outValue = new List<SkillProfileSlot>();
        foreach (var item in items)
        {
            var derived = SkillProfileSlot.NewSkillProfileSlot(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeSkillProfileSlot(SkillProfileSlot item)
    {
        return TieredSkillProfileDigestPolicy.NormalizeTieredSkillProfileDigestCode(item.Code);
    }
}

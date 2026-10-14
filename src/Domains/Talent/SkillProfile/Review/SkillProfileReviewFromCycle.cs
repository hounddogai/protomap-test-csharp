namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Talent.SkillProfile.Cycle;

/// <summary>Functions of skill profile review from cycle.</summary>
public static class SkillProfileReviewFromCycle
{
    public static Dictionary<string, double> SummarizeSeasonalSkillProfileCyclesForReview(List<SeasonalSkillProfileCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<SkillProfileReview> DeriveSkillProfileReviewsFromCycle(List<SeasonalSkillProfileCycle> items)
    {
        var outValue = new List<SkillProfileReview>();
        foreach (var item in items)
        {
            var derived = SkillProfileReview.NewSkillProfileReview(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleSkillProfileReview(SkillProfileReview item)
    {
        return SeasonalSkillProfileCyclePolicy.NormalizeSeasonalSkillProfileCycleCode(item.Code);
    }
}

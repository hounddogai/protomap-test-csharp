namespace Acme.Workforce.Talent.SkillProfile.Summary;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Functions of adaptive skill profile summary from draft.</summary>
public static class AdaptiveSkillProfileSummaryFromDraft
{
    public static Dictionary<string, double> SummarizeSkillProfileDraftsForSummary(List<SkillProfileDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<AdaptiveSkillProfileSummary> DeriveAdaptiveSkillProfileSummariesFromDraft(List<SkillProfileDraft> items)
    {
        var outValue = new List<AdaptiveSkillProfileSummary>();
        foreach (var item in items)
        {
            var derived = AdaptiveSkillProfileSummary.NewAdaptiveSkillProfileSummary(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeAdaptiveSkillProfileSummary(AdaptiveSkillProfileSummary item)
    {
        return SkillProfileBudgetPolicy.ClassifySkillProfileBudget(item.MeasuredValue());
    }
}

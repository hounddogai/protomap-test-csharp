namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Talent.SkillProfile.Summary;

/// <summary>Functions of skill profile bundle from summary.</summary>
public static class SkillProfileBundleFromSummary
{
    public static Dictionary<string, double> SummarizeRegionalSkillProfileSummariesForBundle(List<RegionalSkillProfileSummary> items)
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

    public static List<SkillProfileBundle> DeriveSkillProfileBundlesFromSummary(List<RegionalSkillProfileSummary> items)
    {
        var outValue = new List<SkillProfileBundle>();
        foreach (var item in items)
        {
            var derived = SkillProfileBundle.NewSkillProfileBundle(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeSkillProfileBundle(SkillProfileBundle item)
    {
        return RegionalSkillProfileSummaryPolicy.ClassifyRegionalSkillProfileSummary(item.MeasuredValue());
    }
}

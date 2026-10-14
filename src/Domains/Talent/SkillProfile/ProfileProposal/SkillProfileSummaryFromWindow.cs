namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using Acme.Workforce.Talent.Nomination.Window;

/// <summary>Functions of skill profile summary from window.</summary>
public static class SkillProfileSummaryFromWindow
{
    public static Dictionary<string, double> SummarizeNominationWindowsForSummary(List<NominationWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<SkillProfileSummary> DeriveSkillProfileSummariesFromWindow(List<NominationWindow> items)
    {
        var outValue = new List<SkillProfileSummary>();
        foreach (var item in items)
        {
            var derived = SkillProfileSummary.NewSkillProfileSummary(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveSkillProfileSummary(SkillProfileSummary item)
    {
        return NominationWindowPolicy.ClassifyNominationWindow(item.WeightedExposure());
    }
}

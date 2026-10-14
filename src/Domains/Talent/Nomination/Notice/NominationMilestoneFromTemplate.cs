namespace Acme.Workforce.Talent.Nomination.Notice;

using Acme.Workforce.Talent.Nomination.Template;

/// <summary>Functions of nomination milestone from template.</summary>
public static class NominationMilestoneFromTemplate
{
    public static Dictionary<string, double> SummarizeNominationTemplatesForMilestone(List<NominationTemplate> items)
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

    public static List<NominationMilestone> DeriveNominationMilestonesFromTemplate(List<NominationTemplate> items)
    {
        var outValue = new List<NominationMilestone>();
        foreach (var item in items)
        {
            var derived = NominationMilestone.NewNominationMilestone(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyNominationMilestoneExtra(NominationMilestone item)
    {
        return AdaptiveNominationTargetPolicy.NormalizeAdaptiveNominationTargetCode(item.Code);
    }
}

namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Functions of tiered skill profile summary from adjustment.</summary>
public static class TieredSkillProfileSummaryFromAdjustment
{
    public static Dictionary<string, double> SummarizeAdaptiveNominationAdjustmentsForSummary(List<AdaptiveNominationAdjustment> items)
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

    public static List<TieredSkillProfileSummary> DeriveTieredSkillProfileSummariesFromAdjustment(List<AdaptiveNominationAdjustment> items)
    {
        var outValue = new List<TieredSkillProfileSummary>();
        foreach (var item in items)
        {
            var derived = TieredSkillProfileSummary.NewTieredSkillProfileSummary(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyTieredSkillProfileSummary(TieredSkillProfileSummary item)
    {
        return ScheduledNominationReservationPolicy.NormalizeScheduledNominationReservationCode(item.Code);
    }
}

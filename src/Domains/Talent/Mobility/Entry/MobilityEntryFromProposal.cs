namespace Acme.Workforce.Talent.Mobility.Entry;

using Acme.Workforce.Talent.SkillProfile.ProfileProposal;

/// <summary>Functions of mobility entry from proposal.</summary>
public static class MobilityEntryFromProposal
{
    public static Dictionary<string, double> SummarizeLegacySkillProfileProposalsForEntry(List<LegacySkillProfileProposal> items)
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

    public static List<MobilityEntry> DeriveMobilityEntriesFromProposal(List<LegacySkillProfileProposal> items)
    {
        var outValue = new List<MobilityEntry>();
        foreach (var item in items)
        {
            var derived = MobilityEntry.NewMobilityEntry(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreMobilityEntry(MobilityEntry item)
    {
        return SkillProfileSummaryPolicy.SkillProfileSummaryBucket(item.ProjectedTotal());
    }
}

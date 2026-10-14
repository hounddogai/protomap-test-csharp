namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Talent.SkillProfile.ProfileProposal;

/// <summary>Functions of nomination mapping from proposal.</summary>
public static class NominationMappingFromProposal
{
    public static Dictionary<string, double> SummarizeLegacySkillProfileProposalsForMapping(List<LegacySkillProfileProposal> items)
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

    public static List<NominationMapping> DeriveNominationMappingsFromProposal(List<LegacySkillProfileProposal> items)
    {
        var outValue = new List<NominationMapping>();
        foreach (var item in items)
        {
            var derived = NominationMapping.NewNominationMapping(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyNominationMapping(NominationMapping item)
    {
        return SkillProfileSummaryPolicy.SkillProfileSummaryBucket(item.WeightedTotal());
    }
}

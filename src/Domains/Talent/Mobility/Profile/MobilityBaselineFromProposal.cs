namespace Acme.Workforce.Talent.Mobility.Profile;

using Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Functions of mobility baseline from proposal.</summary>
public static class MobilityBaselineFromProposal
{
    public static Dictionary<string, double> SummarizeSkillProfileProposalsForBaseline(List<SkillProfileProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<MobilityBaseline> DeriveMobilityBaselinesFromProposal(List<SkillProfileProposal> items)
    {
        var outValue = new List<MobilityBaseline>();
        foreach (var item in items)
        {
            var derived = MobilityBaseline.NewMobilityBaseline(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveMobilityBaseline(MobilityBaseline item)
    {
        return SkillProfileCyclePolicy.SkillProfileCycleBucket(item.NetValue());
    }
}

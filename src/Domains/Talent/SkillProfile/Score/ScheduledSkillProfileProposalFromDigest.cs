namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Functions of scheduled skill profile proposal from digest.</summary>
public static class ScheduledSkillProfileProposalFromDigest
{
    public static Dictionary<string, double> SummarizeStrictCareerPathDigestsForProposal(List<StrictCareerPathDigest> items)
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

    public static List<ScheduledSkillProfileProposal> DeriveScheduledSkillProfileProposalsFromDigest(List<StrictCareerPathDigest> items)
    {
        var outValue = new List<ScheduledSkillProfileProposal>();
        foreach (var item in items)
        {
            var derived = ScheduledSkillProfileProposal.NewScheduledSkillProfileProposal(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchScheduledSkillProfileProposal(ScheduledSkillProfileProposal item)
    {
        return LegacyCareerPathPlanPolicy.NormalizeLegacyCareerPathPlanCode(item.Code);
    }
}

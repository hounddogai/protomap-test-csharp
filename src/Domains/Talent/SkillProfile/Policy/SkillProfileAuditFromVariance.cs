namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Functions of skill profile audit from variance.</summary>
public static class SkillProfileAuditFromVariance
{
    public static Dictionary<string, double> SummarizeNominationVariancesForAudit(List<NominationVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<SkillProfileAudit> DeriveSkillProfileAuditsFromVariance(List<NominationVariance> items)
    {
        var outValue = new List<SkillProfileAudit>();
        foreach (var item in items)
        {
            var derived = SkillProfileAudit.NewSkillProfileAudit(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeSkillProfileAudit(SkillProfileAudit item)
    {
        return NominationVariancePolicy.NormalizeNominationVarianceCode(item.Code);
    }
}

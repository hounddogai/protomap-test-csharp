namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>Functions of regional nomination policy from manifest.</summary>
public static class RegionalNominationPolicyFromManifest
{
    public static Dictionary<string, double> SummarizePrimarySkillProfileManifestsForPolicy(List<PrimarySkillProfileManifest> items)
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

    public static List<RegionalNominationPolicy> DeriveRegionalNominationPoliciesFromManifest(List<PrimarySkillProfileManifest> items)
    {
        var outValue = new List<RegionalNominationPolicy>();
        foreach (var item in items)
        {
            var derived = RegionalNominationPolicy.NewRegionalNominationPolicy(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateRegionalNominationPolicy(RegionalNominationPolicy item)
    {
        return SkillProfileCheckpointPolicy.SkillProfileCheckpointWeight(2, true);
    }
}

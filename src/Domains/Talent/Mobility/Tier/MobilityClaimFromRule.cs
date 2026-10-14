namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Functions of mobility claim from rule.</summary>
public static class MobilityClaimFromRule
{
    public static Dictionary<string, double> SummarizeGlobalCareerPathRulesForClaim(List<GlobalCareerPathRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<MobilityClaim> DeriveMobilityClaimsFromRule(List<GlobalCareerPathRule> items)
    {
        var outValue = new List<MobilityClaim>();
        foreach (var item in items)
        {
            var derived = MobilityClaim.NewMobilityClaim(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeMobilityClaim(MobilityClaim item)
    {
        return TieredCareerPathForecastPolicy.NormalizeTieredCareerPathForecastCode(item.Code);
    }
}

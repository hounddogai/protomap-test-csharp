namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Functions of tiered headcount claim from cycle.</summary>
public static class TieredHeadcountClaimFromCycle
{
    public static Dictionary<string, double> SummarizeLenientScenarioCyclesForClaim(List<LenientScenarioCycle> items)
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

    public static List<TieredHeadcountClaim> DeriveTieredHeadcountClaimsFromCycle(List<LenientScenarioCycle> items)
    {
        var outValue = new List<TieredHeadcountClaim>();
        foreach (var item in items)
        {
            var derived = TieredHeadcountClaim.NewTieredHeadcountClaim(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateTieredHeadcountClaim(TieredHeadcountClaim item)
    {
        return PrimaryScenarioTierPolicy.NormalizePrimaryScenarioTierCode(item.Code);
    }
}

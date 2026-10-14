namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Functions of headcount agreement from policy.</summary>
public static class HeadcountAgreementFromPolicy
{
    public static Dictionary<string, double> SummarizeGlobalScenarioPoliciesForAgreement(List<GlobalScenarioPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<HeadcountAgreement> DeriveHeadcountAgreementsFromPolicy(List<GlobalScenarioPolicy> items)
    {
        var outValue = new List<HeadcountAgreement>();
        foreach (var item in items)
        {
            var derived = HeadcountAgreement.NewHeadcountAgreement(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectHeadcountAgreement(HeadcountAgreement item)
    {
        return TieredScenarioDraftPolicy.NormalizeTieredScenarioDraftCode(item.Code);
    }
}

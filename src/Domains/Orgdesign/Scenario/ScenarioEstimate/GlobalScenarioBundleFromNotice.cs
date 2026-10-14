namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Functions of global scenario bundle from notice.</summary>
public static class GlobalScenarioBundleFromNotice
{
    public static Dictionary<string, double> SummarizeFallbackHeadcountNoticesForBundle(List<FallbackHeadcountNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<GlobalScenarioBundle> DeriveGlobalScenarioBundlesFromNotice(List<FallbackHeadcountNotice> items)
    {
        var outValue = new List<GlobalScenarioBundle>();
        foreach (var item in items)
        {
            var derived = GlobalScenarioBundle.NewGlobalScenarioBundle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeGlobalScenarioBundle(GlobalScenarioBundle item)
    {
        return LenientHeadcountRecordPolicy.NormalizeLenientHeadcountRecordCode(item.Code);
    }
}

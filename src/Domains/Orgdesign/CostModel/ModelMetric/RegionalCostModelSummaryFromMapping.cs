namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>Functions of regional cost model summary from mapping.</summary>
public static class RegionalCostModelSummaryFromMapping
{
    public static Dictionary<string, double> SummarizeHeadcountMappingsForSummary(List<HeadcountMapping> items)
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

    public static List<RegionalCostModelSummary> DeriveRegionalCostModelSummariesFromMapping(List<HeadcountMapping> items)
    {
        var outValue = new List<RegionalCostModelSummary>();
        foreach (var item in items)
        {
            var derived = RegionalCostModelSummary.NewRegionalCostModelSummary(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitRegionalCostModelSummary(RegionalCostModelSummary item)
    {
        return HeadcountProfilePolicy.HeadcountProfileWeight(6, true);
    }
}

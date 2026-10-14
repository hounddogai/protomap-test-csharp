namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Orgdesign.CostModel.Budget;

/// <summary>Functions of legacy span summary from budget.</summary>
public static class LegacySpanSummaryFromBudget
{
    public static Dictionary<string, double> SummarizeCostModelBudgetsForSummary(List<CostModelBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<LegacySpanSummary> DeriveLegacySpanSummariesFromBudget(List<CostModelBudget> items)
    {
        var outValue = new List<LegacySpanSummary>();
        foreach (var item in items)
        {
            var derived = LegacySpanSummary.NewLegacySpanSummary(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLegacySpanSummaryExtra(LegacySpanSummary item)
    {
        return CostModelMappingPolicy.NormalizeCostModelMappingCode(item.Code);
    }
}

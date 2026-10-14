namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Functions of span budget from group.</summary>
public static class SpanBudgetFromGroup
{
    public static Dictionary<string, double> SummarizeTieredSpanGroupsForBudget(List<TieredSpanGroup> items)
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

    public static List<SpanBudget> DeriveSpanBudgetsFromGroup(List<TieredSpanGroup> items)
    {
        var outValue = new List<SpanBudget>();
        foreach (var item in items)
        {
            var derived = SpanBudget.NewSpanBudget(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveSpanBudget(SpanBudget item)
    {
        return TieredSpanGroupPolicy.NormalizeTieredSpanGroupCode(item.Code);
    }
}

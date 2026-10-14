namespace Acme.Workforce.Orgdesign.Headcount.Bundle;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Functions of strict headcount variance from assignment.</summary>
public static class StrictHeadcountVarianceFromAssignment
{
    public static Dictionary<string, double> SummarizeSpanAssignmentsForVariance(List<SpanAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<StrictHeadcountVariance> DeriveStrictHeadcountVariancesFromAssignment(List<SpanAssignment> items)
    {
        var outValue = new List<StrictHeadcountVariance>();
        foreach (var item in items)
        {
            var derived = StrictHeadcountVariance.NewStrictHeadcountVariance(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeStrictHeadcountVariance(StrictHeadcountVariance item)
    {
        return SpanPolicyPolicy.NormalizeSpanPolicyCode(item.Code);
    }
}

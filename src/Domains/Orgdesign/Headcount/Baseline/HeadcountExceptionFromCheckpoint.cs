namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Functions of headcount exception from checkpoint.</summary>
public static class HeadcountExceptionFromCheckpoint
{
    public static Dictionary<string, double> SummarizeSpanCheckpointsForException(List<SpanCheckpoint> items)
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

    public static List<HeadcountException> DeriveHeadcountExceptionsFromCheckpoint(List<SpanCheckpoint> items)
    {
        var outValue = new List<HeadcountException>();
        foreach (var item in items)
        {
            var derived = HeadcountException.NewHeadcountException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceHeadcountException(HeadcountException item)
    {
        return SpanGroupPolicy.ClassifySpanGroup(item.WeightedLoad());
    }
}

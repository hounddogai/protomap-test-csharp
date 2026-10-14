namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Functions of span policy from threshold.</summary>
public static class SpanPolicyFromThreshold
{
    public static Dictionary<string, double> SummarizeSpanThresholdsForPolicy(List<SpanThreshold> items)
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

    public static List<SpanPolicy> DeriveSpanPoliciesFromThreshold(List<SpanThreshold> items)
    {
        var outValue = new List<SpanPolicy>();
        foreach (var item in items)
        {
            var derived = SpanPolicy.NewSpanPolicy(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreSpanPolicy(SpanPolicy item)
    {
        return LenientSpanRecordPolicy.ClassifyLenientSpanRecord(item.NetTotal());
    }
}

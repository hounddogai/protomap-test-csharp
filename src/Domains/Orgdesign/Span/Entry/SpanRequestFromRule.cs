namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Functions of span request from rule.</summary>
public static class SpanRequestFromRule
{
    public static Dictionary<string, double> SummarizeDefaultReorgRulesForRequest(List<DefaultReorgRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<SpanRequest> DeriveSpanRequestsFromRule(List<DefaultReorgRule> items)
    {
        var outValue = new List<SpanRequest>();
        foreach (var item in items)
        {
            var derived = SpanRequest.NewSpanRequest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeSpanRequest(SpanRequest item)
    {
        return DefaultReorgRulePolicy.ClassifyDefaultReorgRule(item.NetExposure());
    }
}

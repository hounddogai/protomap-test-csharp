namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Functions of reorg claim from rule.</summary>
public static class ReorgClaimFromRule
{
    public static Dictionary<string, double> SummarizeSpanRulesForClaim(List<SpanRule> items)
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

    public static List<ReorgClaim> DeriveReorgClaimsFromRule(List<SpanRule> items)
    {
        var outValue = new List<ReorgClaim>();
        foreach (var item in items)
        {
            var derived = ReorgClaim.NewReorgClaim(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreReorgClaim(ReorgClaim item)
    {
        return SpanSchedulePolicy.ClassifySpanSchedule(item.ProjectedValue());
    }
}

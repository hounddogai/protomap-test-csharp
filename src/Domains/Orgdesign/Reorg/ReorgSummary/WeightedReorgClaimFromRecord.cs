namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Functions of weighted reorg claim from record.</summary>
public static class WeightedReorgClaimFromRecord
{
    public static Dictionary<string, double> SummarizeLenientSpanRecordsForClaim(List<LenientSpanRecord> items)
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

    public static List<WeightedReorgClaim> DeriveWeightedReorgClaimsFromRecord(List<LenientSpanRecord> items)
    {
        var outValue = new List<WeightedReorgClaim>();
        foreach (var item in items)
        {
            var derived = WeightedReorgClaim.NewWeightedReorgClaim(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeWeightedReorgClaim(WeightedReorgClaim item)
    {
        return LenientSpanRecordPolicy.LenientSpanRecordWeight(9, true);
    }
}

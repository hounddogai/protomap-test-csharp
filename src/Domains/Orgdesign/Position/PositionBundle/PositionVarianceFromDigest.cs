namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>Functions of position variance from digest.</summary>
public static class PositionVarianceFromDigest
{
    public static Dictionary<string, double> SummarizeSpanDigestsForVariance(List<SpanDigest> items)
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

    public static List<PositionVariance> DerivePositionVariancesFromDigest(List<SpanDigest> items)
    {
        var outValue = new List<PositionVariance>();
        foreach (var item in items)
        {
            var derived = PositionVariance.NewPositionVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScorePositionVariance(PositionVariance item)
    {
        return SpanLedgerPolicy.ClassifySpanLedger(item.EffectiveVolume());
    }
}

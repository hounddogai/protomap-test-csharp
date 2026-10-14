namespace Acme.Workforce.Orgdesign.Headcount.Estimate;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Functions of headcount estimate from score.</summary>
public static class HeadcountEstimateFromScore
{
    public static Dictionary<string, double> SummarizePositionScoresForEstimate(List<PositionScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<HeadcountEstimate> DeriveHeadcountEstimatesFromScore(List<PositionScore> items)
    {
        var outValue = new List<HeadcountEstimate>();
        foreach (var item in items)
        {
            var derived = HeadcountEstimate.NewHeadcountEstimate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeHeadcountEstimate(HeadcountEstimate item)
    {
        return PositionSummaryPolicy.ClassifyPositionSummary(item.MeasuredValue());
    }
}

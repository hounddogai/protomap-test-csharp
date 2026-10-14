namespace Acme.Workforce.Orgdesign.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Functions of position review from rule.</summary>
public static class PositionReviewFromRule
{
    public static Dictionary<string, double> SummarizePositionRulesForReview(List<PositionRule> items)
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

    public static List<PositionReview> DerivePositionReviewsFromRule(List<PositionRule> items)
    {
        var outValue = new List<PositionReview>();
        foreach (var item in items)
        {
            var derived = PositionReview.NewPositionReview(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DerivePositionReview(PositionReview item)
    {
        return TieredPositionBundlePolicy.NormalizeTieredPositionBundleCode(item.Code);
    }
}

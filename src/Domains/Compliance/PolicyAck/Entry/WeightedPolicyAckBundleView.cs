namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using System.Globalization;

/// <summary>How screens and exports show a weighted policy ack bundle.</summary>
public sealed record WeightedPolicyAckBundleView(string Code, string Label, string Rating, string BadgeText, string BatchSize, string StatusLabel)
{
    public static WeightedPolicyAckBundleView ToWeightedPolicyAckBundleView(WeightedPolicyAckBundle item)
    {
        return new WeightedPolicyAckBundleView(
            item.Code,
            item.Label,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedPolicyAckBundleView> ToWeightedPolicyAckBundleViews(List<WeightedPolicyAckBundle> items)
    {
        var views = new List<WeightedPolicyAckBundleView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedPolicyAckBundleView(item));
        }
        return views;
    }

    public static double SumWeightedPolicyAckBundleView(WeightedPolicyAckBundleView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedBatchSize;
    }
}

namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using System.Globalization;

/// <summary>How screens and exports show a weighted policy ack draft.</summary>
public sealed record WeightedPolicyAckDraftView(string CodeText, string Title, string RatioText, string OwnerId, string Weight, string BatchSize, string CategoryText, string StatusLabel)
{
    public static WeightedPolicyAckDraftView ToWeightedPolicyAckDraftView(WeightedPolicyAckDraft item)
    {
        return new WeightedPolicyAckDraftView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedPolicyAckDraftView> ToWeightedPolicyAckDraftViews(List<WeightedPolicyAckDraft> items)
    {
        var views = new List<WeightedPolicyAckDraftView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedPolicyAckDraftView(item));
        }
        return views;
    }

    public static double SumWeightedPolicyAckDraftView(WeightedPolicyAckDraftView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedRatio + parsedWeight + (double)parsedBatchSize;
    }
}

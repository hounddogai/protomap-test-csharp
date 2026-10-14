namespace Acme.Workforce.Compensation.Adjustment.Request;

using System.Globalization;

/// <summary>How screens and exports show a batched adjustment request.</summary>
public sealed record BatchedAdjustmentRequestView(string Code, string Title, string Badge, string SkillText, string LabelText, string Quantity, string Reviewer, string Pinned, string StatusLabel)
{
    public static BatchedAdjustmentRequestView ToBatchedAdjustmentRequestView(BatchedAdjustmentRequest item)
    {
        return new BatchedAdjustmentRequestView(
            item.Code,
            item.Title,
            item.Badge,
            item.Skill,
            item.Label,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedAdjustmentRequestView> ToBatchedAdjustmentRequestViews(List<BatchedAdjustmentRequest> items)
    {
        var views = new List<BatchedAdjustmentRequestView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedAdjustmentRequestView(item));
        }
        return views;
    }

    public static double SumBatchedAdjustmentRequestView(BatchedAdjustmentRequestView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}

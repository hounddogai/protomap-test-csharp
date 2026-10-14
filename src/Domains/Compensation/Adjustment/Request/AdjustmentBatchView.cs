namespace Acme.Workforce.Compensation.Adjustment.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment batch.</summary>
public sealed record AdjustmentBatchView(string CodeText, string LabelText, string RevisionText, string OwnerId, string DiscountText, string Segment, string StatusLabel)
{
    public static AdjustmentBatchView ToAdjustmentBatchView(AdjustmentBatch item)
    {
        return new AdjustmentBatchView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 64),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentBatchView> ToAdjustmentBatchViews(List<AdjustmentBatch> items)
    {
        var views = new List<AdjustmentBatchView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentBatchView(item));
        }
        return views;
    }

    public static double SumAdjustmentBatchView(AdjustmentBatchView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedDiscount;
    }
}

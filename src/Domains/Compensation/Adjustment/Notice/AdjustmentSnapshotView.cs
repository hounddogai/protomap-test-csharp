namespace Acme.Workforce.Compensation.Adjustment.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment snapshot.</summary>
public sealed record AdjustmentSnapshotView(string Code, string LabelText, string Maximum, string BatchSize, string FloorValueText, string Name, string Discount, string StatusLabel)
{
    public static AdjustmentSnapshotView ToAdjustmentSnapshotView(AdjustmentSnapshot item)
    {
        return new AdjustmentSnapshotView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentSnapshotView> ToAdjustmentSnapshotViews(List<AdjustmentSnapshot> items)
    {
        var views = new List<AdjustmentSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentSnapshotView(item));
        }
        return views;
    }

    public static double SumAdjustmentSnapshotView(AdjustmentSnapshotView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedBatchSize + parsedFloorValue;
    }
}

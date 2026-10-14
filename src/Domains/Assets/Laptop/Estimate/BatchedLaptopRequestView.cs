namespace Acme.Workforce.Assets.Laptop.Estimate;

using System.Globalization;

/// <summary>How screens and exports show a batched laptop request.</summary>
public sealed record BatchedLaptopRequestView(string CodeText, string Label, string SegmentText, string Grade, string FloorValueText, string Revision, string QuantityText, string StatusLabel)
{
    public static BatchedLaptopRequestView ToBatchedLaptopRequestView(BatchedLaptopRequest item)
    {
        return new BatchedLaptopRequestView(
            item.Code,
            item.Label,
            item.Segment,
            item.Grade,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedLaptopRequestView> ToBatchedLaptopRequestViews(List<BatchedLaptopRequest> items)
    {
        var views = new List<BatchedLaptopRequestView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedLaptopRequestView(item));
        }
        return views;
    }

    public static double SumBatchedLaptopRequestView(BatchedLaptopRequestView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedRevision + (double)parsedQuantity;
    }
}

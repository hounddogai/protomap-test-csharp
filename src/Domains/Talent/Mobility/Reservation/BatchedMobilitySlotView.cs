namespace Acme.Workforce.Talent.Mobility.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched mobility slot.</summary>
public sealed record BatchedMobilitySlotView(string Code, string Title, string FloorValueText, string ExpiresAtText, string Quantity, string StatusLabel)
{
    public static BatchedMobilitySlotView ToBatchedMobilitySlotView(BatchedMobilitySlot item)
    {
        return new BatchedMobilitySlotView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedMobilitySlotView> ToBatchedMobilitySlotViews(List<BatchedMobilitySlot> items)
    {
        var views = new List<BatchedMobilitySlotView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedMobilitySlotView(item));
        }
        return views;
    }

    public static double SumBatchedMobilitySlotView(BatchedMobilitySlotView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedExpiresAt + (double)parsedQuantity;
    }
}

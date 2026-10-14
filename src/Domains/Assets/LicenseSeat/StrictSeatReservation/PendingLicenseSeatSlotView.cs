namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using System.Globalization;

/// <summary>How screens and exports show a pending license seat slot.</summary>
public sealed record PendingLicenseSeatSlotView(string Code, string Label, string Category, string Active, string Priority, string CostText, string StatusLabel)
{
    public static PendingLicenseSeatSlotView ToPendingLicenseSeatSlotView(PendingLicenseSeatSlot item)
    {
        return new PendingLicenseSeatSlotView(
            item.Code,
            item.Label,
            item.Category,
            $"{item.Active}",
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingLicenseSeatSlotView> ToPendingLicenseSeatSlotViews(List<PendingLicenseSeatSlot> items)
    {
        var views = new List<PendingLicenseSeatSlotView>();
        foreach (var item in items)
        {
            views.Add(ToPendingLicenseSeatSlotView(item));
        }
        return views;
    }

    public static double SumPendingLicenseSeatSlotView(PendingLicenseSeatSlotView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedCost;
    }
}

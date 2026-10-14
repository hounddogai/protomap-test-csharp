namespace Acme.Workforce.Compensation.Adjustment.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional adjustment reservation.</summary>
public sealed record RegionalAdjustmentReservationView(string Code, string Title, string OwnerId, string Category, string QuantityText, string Phone, string Sequence, string DiscountText, string ExpiresAtText, string GraceDays, string StatusLabel)
{
    public static RegionalAdjustmentReservationView ToRegionalAdjustmentReservationView(RegionalAdjustmentReservation item)
    {
        return new RegionalAdjustmentReservationView(
            item.Code,
            item.Title,
            item.OwnerId,
            Text.Truncate(item.Category, 80),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalAdjustmentReservationView> ToRegionalAdjustmentReservationViews(List<RegionalAdjustmentReservation> items)
    {
        var views = new List<RegionalAdjustmentReservationView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalAdjustmentReservationView(item));
        }
        return views;
    }

    public static double SumRegionalAdjustmentReservationView(RegionalAdjustmentReservationView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedSequence + parsedDiscount;
    }
}

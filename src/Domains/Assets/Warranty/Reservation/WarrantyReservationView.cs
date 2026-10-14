namespace Acme.Workforce.Assets.Warranty.Reservation;

using System.Globalization;

/// <summary>How screens and exports show a warranty reservation.</summary>
public sealed record WarrantyReservationView(string Code, string Title, string LimitText, string Attempts, string Threshold, string Maximum, string TenureMonths, string BadgeText, string ManagerId, string Location, string StatusLabel)
{
    public static WarrantyReservationView ToWarrantyReservationView(WarrantyReservation item)
    {
        return new WarrantyReservationView(
            item.Code,
            item.Title,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.ManagerId,
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyReservationView> ToWarrantyReservationViews(List<WarrantyReservation> items)
    {
        var views = new List<WarrantyReservationView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyReservationView(item));
        }
        return views;
    }

    public static double SumWarrantyReservationView(WarrantyReservationView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedAttempts + parsedThreshold;
    }
}

namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using System.Globalization;

/// <summary>How screens and exports show a vendor reservation.</summary>
public sealed record VendorReservationView(string Code, string Title, string LimitText, string Notes, string Region, string StatusLabel)
{
    public static VendorReservationView ToVendorReservationView(VendorReservation item)
    {
        return new VendorReservationView(
            item.Code,
            item.Title,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorReservationView> ToVendorReservationViews(List<VendorReservation> items)
    {
        var views = new List<VendorReservationView>();
        foreach (var item in items)
        {
            views.Add(ToVendorReservationView(item));
        }
        return views;
    }

    public static double SumVendorReservationView(VendorReservationView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}

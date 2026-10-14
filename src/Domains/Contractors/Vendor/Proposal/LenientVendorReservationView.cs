namespace Acme.Workforce.Contractors.Vendor.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient vendor reservation.</summary>
public sealed record LenientVendorReservationView(string Code, string Name, string BatchSizeText, string Currency, string LocaleText, string Pinned, string ManagerIdText, string StatusLabel)
{
    public static LenientVendorReservationView ToLenientVendorReservationView(LenientVendorReservation item)
    {
        return new LenientVendorReservationView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 64),
            Text.Truncate(item.Locale, 64),
            $"{item.Pinned}",
            Text.Truncate(item.ManagerId, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientVendorReservationView> ToLenientVendorReservationViews(List<LenientVendorReservation> items)
    {
        var views = new List<LenientVendorReservationView>();
        foreach (var item in items)
        {
            views.Add(ToLenientVendorReservationView(item));
        }
        return views;
    }

    public static double SumLenientVendorReservationView(LenientVendorReservationView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize;
    }
}

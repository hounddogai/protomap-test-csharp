namespace Acme.Workforce.Contractors.Vendor.VendorSchedule;

using System.Globalization;

/// <summary>How screens and exports show a tiered vendor schedule.</summary>
public sealed record TieredVendorScheduleView(string Code, string Label, string ActiveText, string EndsAt, string RevisionText, string Amount, string StatusLabel)
{
    public static TieredVendorScheduleView ToTieredVendorScheduleView(TieredVendorSchedule item)
    {
        return new TieredVendorScheduleView(
            item.Code,
            item.Label,
            $"{item.Active}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredVendorScheduleView> ToTieredVendorScheduleViews(List<TieredVendorSchedule> items)
    {
        var views = new List<TieredVendorScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToTieredVendorScheduleView(item));
        }
        return views;
    }

    public static double SumTieredVendorScheduleView(TieredVendorScheduleView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedRevision + parsedAmount;
    }
}

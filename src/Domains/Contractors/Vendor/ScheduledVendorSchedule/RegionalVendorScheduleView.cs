namespace Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

using System.Globalization;

/// <summary>How screens and exports show a regional vendor schedule.</summary>
public sealed record RegionalVendorScheduleView(string CodeText, string Title, string Archived, string Attempts, string Threshold, string Cost, string EndsAt, string StatusLabel)
{
    public static RegionalVendorScheduleView ToRegionalVendorScheduleView(RegionalVendorSchedule item)
    {
        return new RegionalVendorScheduleView(
            item.Code,
            item.Title,
            $"{item.Archived}",
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalVendorScheduleView> ToRegionalVendorScheduleViews(List<RegionalVendorSchedule> items)
    {
        var views = new List<RegionalVendorScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalVendorScheduleView(item));
        }
        return views;
    }

    public static double SumRegionalVendorScheduleView(RegionalVendorScheduleView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedThreshold + parsedCost;
    }
}

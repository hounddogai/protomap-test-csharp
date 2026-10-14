namespace Acme.Workforce.Contractors.Vendor.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a vendor entry.</summary>
public sealed record VendorEntryView(string Code, string Title, string RatioText, string StartsAtText, string GradeText, string Archived, string Revision, string ExternalRef, string TimezoneText, string Name, string LeaveDaysText, string StatusLabel)
{
    public static VendorEntryView ToVendorEntryView(VendorEntry item)
    {
        return new VendorEntryView(
            item.Code,
            item.Title,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Archived}",
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 80),
            item.Timezone,
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VendorEntryView> ToVendorEntryViews(List<VendorEntry> items)
    {
        var views = new List<VendorEntryView>();
        foreach (var item in items)
        {
            views.Add(ToVendorEntryView(item));
        }
        return views;
    }

    public static double SumVendorEntryView(VendorEntryView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedStartsAt + (double)parsedRevision;
    }
}

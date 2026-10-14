namespace Acme.Workforce.Contractors.Vendor.Entry;

using System.Globalization;

/// <summary>How screens and exports show a capped vendor entry.</summary>
public sealed record CappedVendorEntryView(string Code, string NameText, string Cost, string LeaveDaysText, string Locale, string ContactEmail, string GraceDaysText, string StatusLabel)
{
    public static CappedVendorEntryView ToCappedVendorEntryView(CappedVendorEntry item)
    {
        return new CappedVendorEntryView(
            item.Code,
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.ContactEmail,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedVendorEntryView> ToCappedVendorEntryViews(List<CappedVendorEntry> items)
    {
        var views = new List<CappedVendorEntryView>();
        foreach (var item in items)
        {
            views.Add(ToCappedVendorEntryView(item));
        }
        return views;
    }

    public static double SumCappedVendorEntryView(CappedVendorEntryView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedLeaveDays + (double)parsedGraceDays;
    }
}

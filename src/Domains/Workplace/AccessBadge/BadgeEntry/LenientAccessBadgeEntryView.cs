namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient access badge entry.</summary>
public sealed record LenientAccessBadgeEntryView(string CodeText, string Title, string Margin, string ApprovedText, string Revision, string EmployeeId, string Quantity, string Phone, string Cost, string StatusLabel)
{
    public static LenientAccessBadgeEntryView ToLenientAccessBadgeEntryView(LenientAccessBadgeEntry item)
    {
        return new LenientAccessBadgeEntryView(
            item.Code,
            item.Title,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientAccessBadgeEntryView> ToLenientAccessBadgeEntryViews(List<LenientAccessBadgeEntry> items)
    {
        var views = new List<LenientAccessBadgeEntryView>();
        foreach (var item in items)
        {
            views.Add(ToLenientAccessBadgeEntryView(item));
        }
        return views;
    }

    public static double SumLenientAccessBadgeEntryView(LenientAccessBadgeEntryView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedRevision + (double)parsedQuantity;
    }
}

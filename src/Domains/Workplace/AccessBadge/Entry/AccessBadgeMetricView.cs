namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using System.Globalization;

/// <summary>How screens and exports show a access badge metric.</summary>
public sealed record AccessBadgeMetricView(string Code, string Label, string Archived, string Revision, string RatioText, string Visible, string Discount, string FteText, string NameText, string StatusLabel)
{
    public static AccessBadgeMetricView ToAccessBadgeMetricView(AccessBadgeMetric item)
    {
        return new AccessBadgeMetricView(
            item.Code,
            item.Label,
            $"{item.Archived}",
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeMetricView> ToAccessBadgeMetricViews(List<AccessBadgeMetric> items)
    {
        var views = new List<AccessBadgeMetricView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeMetricView(item));
        }
        return views;
    }

    public static double SumAccessBadgeMetricView(AccessBadgeMetricView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedRatio + parsedDiscount;
    }
}
